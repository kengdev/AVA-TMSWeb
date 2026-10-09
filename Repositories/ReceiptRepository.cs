using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class ReceiptRepository
{
    private readonly DbavalogContext db;
    private readonly CodeRepository codeRepo;
    private readonly IAuditLogService auditLogService;
    private readonly ILogger<ReceiptRepository> logger;

    public ReceiptRepository(
        DbavalogContext context,
        IAuditLogService _auditLogService,
        CodeRepository _codeRepo,
        ILogger<ReceiptRepository> _logger)
    {
        db = context;
        auditLogService = _auditLogService;
        codeRepo = _codeRepo;
        logger = _logger;
    }

    public async Task<List<CoverSheetViewModel>> GetBillingDataList(
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from a in db.TblBillingMasters.AsNoTracking()
            join b in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals b.CustomerCode
            where a.IsClearing == false &&
                  a.IsEnabled == true
            select new
            {
                a.BillingCode,
                b.CustomerAcronym,
                a.BillingDate,
                a.BillingNetTotal
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(a => new CoverSheetViewModel
            {
                billing_code = a.BillingCode,
                customer_name = a.CustomerAcronym,
                billing_date = a.BillingDate.ToString("yyyy-MM-dd"),
                billing_total = a.BillingNetTotal
            })
            .ToList();
    }

    public async Task<CoverSheetViewModel?> GetBillingBycode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await (
            from a in db.TblBillingMasters.AsNoTracking()
            join b in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals b.CustomerCode
            where a.BillingCode == code
            select new CoverSheetViewModel
            {
                billing_code = a.BillingCode,
                create_date = a.CreateDate,
                customer_name = b.CustomerAcronym,
                transport_value = a.TransportTotal,
                advance_value = a.AdvanceTotal,
                billing_total = a.BillingNetTotal
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ResponseViewModel> AddReceipt(
        TblReceipt modelToAdd,
        CancellationToken cancellationToken = default)
    {
        if (modelToAdd == null)
        {
            return Failure("ไม่พบข้อมูลใบเสร็จรับเงิน");
        }

        var receiptDetails = modelToAdd.TblReceiptDetails?
            .ToList() ?? new List<TblReceiptDetail>();

        var validation = ValidateReceiptDetails(receiptDetails);
        if (validation != null)
        {
            return validation;
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var billingCodes = receiptDetails
                .Select(x => x.BillingCode)
                .Distinct()
                .ToList();

            var billings = await db.TblBillingMasters
                .Where(x => billingCodes.Contains(x.BillingCode))
                .ToListAsync(cancellationToken);

            if (billings.Count != billingCodes.Count)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("พบ Billing บางรายการไม่อยู่ในฐานข้อมูล");
            }

            var unavailableBilling = billings.FirstOrDefault(x =>
                x.IsEnabled != true ||
                x.IsClearing == true);

            if (unavailableBilling != null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Failure(
                    $"Billing {unavailableBilling.BillingCode} ไม่สามารถออกใบเสร็จได้ " +
                    "เนื่องจากถูกยกเลิกหรือถูกเคลียร์ไปแล้ว");
            }

            var customerCodes = billings
                .Select(x => x.CustomerCode)
                .Distinct()
                .ToList();

            if (customerCodes.Count != 1)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("Billing ที่เลือกต้องเป็นลูกค้ารายเดียวกันทั้งหมด");
            }

            var customerCode = customerCodes[0];

            var customer = await db.TblCustomers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CustomerCode == customerCode,
                    cancellationToken);

            if (customer == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure($"ไม่พบข้อมูลลูกค้า {customerCode}");
            }

            await RecalculateBillingTotalsAsync(
                billings,
                cancellationToken);

            var billingMap = billings
                .ToDictionary(x => x.BillingCode);

            if (string.IsNullOrWhiteSpace(modelToAdd.ReceiptCode))
            {
                modelToAdd.ReceiptCode =
                    await codeRepo.GetReceiptCode();
            }

            modelToAdd.ReceiptPartCode =
                modelToAdd.ReceiptCode;

            modelToAdd.ReceiptName =
                string.IsNullOrWhiteSpace(modelToAdd.ReceiptName)
                    ? "Transport Charge"
                    : modelToAdd.ReceiptName;

            modelToAdd.CustomerName = customer.CustomerNameTh;
            modelToAdd.CustomerTax = customer.CustomerVatNo;
            modelToAdd.CustomerCode = customerCode;
            modelToAdd.CustomerAddress = customer.CustomerAddress;
            modelToAdd.ReceiptAmount = receiptDetails.Count;
            modelToAdd.IsExcharge = false;
            modelToAdd.IsEnabled = true;

            var now = DateTime.Now;

            foreach (var item in receiptDetails)
            {
                var billing = billingMap[item.BillingCode];

                item.ReceiptCode = modelToAdd.ReceiptCode;
                item.ReceiptAdvanceValue = billing.AdvanceTotal;
                item.ReceiptTransportValue = billing.TransportTotal;
                item.ReceiptValue = billing.BillingNetTotal;
                item.BillingType = billing.BillingType;

                billing.IsClearing = true;
                billing.ClearingDate = modelToAdd.ReceiptDate;
                billing.UpdateDate = now;
                billing.UpdateBy = modelToAdd.UpdateBy;
            }

            ApplyReceiptTotals(
                modelToAdd,
                receiptDetails);

            await db.TblReceipts.AddAsync(
                modelToAdd,
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "AddReceipt",
                modelToAdd.ReceiptCode);
        }
    }

    public async Task<ResponseViewModel> RemoveReceipt(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบเลขที่ใบเสร็จรับเงิน");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var now = DateTime.Now;

            var receipt = await db.TblReceipts
                .FirstOrDefaultAsync(
                    x => x.ReceiptCode == code,
                    cancellationToken);

            if (receipt == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure($"ไม่พบใบเสร็จรับเงิน {code}");
            }

            if (receipt.IsEnabled != true)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ใบเสร็จรับเงินรายการนี้ถูกยกเลิกไปแล้ว");
            }

            var receiptDetails = await db.TblReceiptDetails
                .Where(x => x.ReceiptCode == code)
                .ToListAsync(cancellationToken);

            var oldValues = new
            {
                Receipt = GetScalarSnapshot(receipt),
                Details = receiptDetails
                    .Select(GetScalarSnapshot)
                    .ToList()
            };

            var billingCodes = receiptDetails
                .Select(x => x.BillingCode)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var billings = billingCodes.Count == 0
                ? new Dictionary<string, TblBillingMaster>()
                : await db.TblBillingMasters
                    .Where(x => billingCodes.Contains(x.BillingCode))
                    .ToDictionaryAsync(
                        x => x.BillingCode,
                        cancellationToken);

            foreach (var item in receiptDetails)
            {
                if (billings.TryGetValue(
                    item.BillingCode,
                    out var billing))
                {
                    billing.IsClearing = false;
                    billing.ClearingDate = null;
                    billing.UpdateDate = now;
                    billing.UpdateBy = username;
                }
            }

            if (receiptDetails.Count > 0)
            {
                db.TblReceiptDetails.RemoveRange(
                    receiptDetails);
            }

            receipt.IsEnabled = false;
            receipt.UpdateDate = now;
            receipt.UpdateBy = username;

            var newValues = new
            {
                Receipt = GetScalarSnapshot(receipt),
                ReleasedBillingCodes = billingCodes
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Remove receipt.",
                    ActionName = AuditActions.Delete,
                    EntityName = nameof(TblReceipt),
                    EntityId = code,
                    Description =
                        $"ลบข้อมูลใบเสร็จรับเงิน {code} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "RemoveReceipt",
                code);
        }
    }

    public async Task<List<TblReceiptDetail>> GetReceiptDetail(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new List<TblReceiptDetail>();
        }

        return await db.TblReceiptDetails
            .AsNoTracking()
            .Where(x => x.ReceiptCode == code)
            .OrderBy(x => x.BillingCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ReceiptViewModel>> GetReceiptDataList(
        string month,
        string year,
        CancellationToken cancellationToken = default)
    {
        var (startDate, endDate) =
            GetMonthRange(month, year);

        var rows = await (
            from a in db.TblReceipts.AsNoTracking()
            join b in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals b.CustomerCode
            where a.IsEnabled == true &&
                  a.ReceiptDate >= startDate &&
                  a.ReceiptDate < endDate
            select new
            {
                a.ReceiptCode,
                a.ReceiptPartCode,
                b.CustomerAcronym,
                a.ReceiptNetTotal,
                a.ReceiptDate,
                a.ReceiptAmount
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(a => new ReceiptViewModel
            {
                receipt_code = a.ReceiptCode,
                receipt_part_code = a.ReceiptPartCode,
                customer_name = a.CustomerAcronym,
                receipt_total = a.ReceiptNetTotal,
                receipt_date = a.ReceiptDate.ToString("yyyy-MM-dd"),
                receipt_amount = a.ReceiptAmount
            })
            .ToList();
    }

    public async Task<TblReceipt?> GetReceiptByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblReceipts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ReceiptCode == code,
                cancellationToken);
    }

    public async Task<ResponseViewModel> EditReceipt(
        TblReceipt receiptToEdit,
        CancellationToken cancellationToken = default)
    {
        if (receiptToEdit == null ||
            string.IsNullOrWhiteSpace(receiptToEdit.ReceiptCode))
        {
            return Failure("ไม่พบข้อมูลใบเสร็จรับเงิน");
        }

        var newInputDetails = receiptToEdit.TblReceiptDetails?
            .ToList() ?? new List<TblReceiptDetail>();

        var validation = ValidateReceiptDetails(
            newInputDetails);

        if (validation != null)
        {
            return validation;
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var now = DateTime.Now;

            var receipt = await db.TblReceipts
                .FirstOrDefaultAsync(
                    x => x.ReceiptCode ==
                         receiptToEdit.ReceiptCode,
                    cancellationToken);

            if (receipt == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Failure(
                    $"ไม่พบใบเสร็จรับเงิน {receiptToEdit.ReceiptCode}");
            }

            if (receipt.IsEnabled != true)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ใบเสร็จรับเงินรายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldDetails = await db.TblReceiptDetails
                .Where(x =>
                    x.ReceiptCode ==
                    receiptToEdit.ReceiptCode)
                .ToListAsync(cancellationToken);

            var oldValues = new
            {
                Receipt = GetScalarSnapshot(receipt),
                Details = oldDetails
                    .Select(GetScalarSnapshot)
                    .ToList()
            };

            var oldBillingCodes = oldDetails
                .Select(x => x.BillingCode)
                .Distinct()
                .ToHashSet();

            var newBillingCodes = newInputDetails
                .Select(x => x.BillingCode)
                .Distinct()
                .ToList();

            var allBillingCodes = oldBillingCodes
                .Union(newBillingCodes)
                .ToList();

            var billings = await db.TblBillingMasters
                .Where(x =>
                    allBillingCodes.Contains(
                        x.BillingCode))
                .ToListAsync(cancellationToken);

            var billingMap = billings
                .ToDictionary(x => x.BillingCode);

            var selectedBillings = billings
                .Where(x =>
                    newBillingCodes.Contains(
                        x.BillingCode))
                .ToList();

            if (selectedBillings.Count !=
                newBillingCodes.Count)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("พบ Billing บางรายการไม่อยู่ในฐานข้อมูล");
            }

            var unavailableBilling =
                selectedBillings.FirstOrDefault(x =>
                    x.IsEnabled != true ||
                    (x.IsClearing == true &&
                     !oldBillingCodes.Contains(
                         x.BillingCode)));

            if (unavailableBilling != null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Failure(
                    $"Billing {unavailableBilling.BillingCode} ไม่สามารถใช้กับใบเสร็จนี้ได้ " +
                    "เนื่องจากถูกยกเลิกหรืออยู่ในใบเสร็จอื่นแล้ว");
            }

            var customerCodes = selectedBillings
                .Select(x => x.CustomerCode)
                .Distinct()
                .ToList();

            if (customerCodes.Count != 1)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("Billing ที่เลือกต้องเป็นลูกค้ารายเดียวกันทั้งหมด");
            }

            var customerCode = customerCodes[0];

            var customer = await db.TblCustomers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CustomerCode ==
                         customerCode,
                    cancellationToken);

            if (customer == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure($"ไม่พบข้อมูลลูกค้า {customerCode}");
            }

            await RecalculateBillingTotalsAsync(
                selectedBillings,
                cancellationToken);

            foreach (var oldBillingCode in oldBillingCodes)
            {
                if (!newBillingCodes.Contains(
                    oldBillingCode) &&
                    billingMap.TryGetValue(
                        oldBillingCode,
                        out var oldBilling))
                {
                    oldBilling.IsClearing = false;
                    oldBilling.ClearingDate = null;
                    oldBilling.UpdateDate = now;
                    oldBilling.UpdateBy =
                        receiptToEdit.UpdateBy;
                }
            }

            CopyEditableScalarValues(
                receipt,
                receiptToEdit,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    nameof(TblReceipt.ReceiptCode),
                    nameof(TblReceipt.CustomerCode),
                    nameof(TblReceipt.ReceiptAmount),
                    nameof(TblReceipt.ReceiptAdvance),
                    nameof(TblReceipt.ReceiptTransport),
                    nameof(TblReceipt.ReceiptTotal),
                    nameof(TblReceipt.ReceiptNetTotal),
                    nameof(TblReceipt.IsEnabled),
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            var customerChanged =
                !string.Equals(
                    receipt.CustomerCode,
                    customerCode,
                    StringComparison.OrdinalIgnoreCase);

            receipt.CustomerCode = customerCode;

            if (customerChanged)
            {
                receipt.CustomerName =
                    customer.CustomerNameTh;

                receipt.CustomerTax =
                    customer.CustomerVatNo;

                receipt.CustomerAddress =
                    customer.CustomerAddress;
            }

            var oldDetailMap = oldDetails
                .ToDictionary(x => x.BillingCode);

            var newBillingCodeSet =
                newBillingCodes.ToHashSet();

            var removedDetails = oldDetails
                .Where(x =>
                    !newBillingCodeSet.Contains(
                        x.BillingCode))
                .ToList();

            if (removedDetails.Count > 0)
            {
                db.TblReceiptDetails.RemoveRange(
                    removedDetails);
            }

            var finalDetails =
                new List<TblReceiptDetail>(
                    newInputDetails.Count);

            foreach (var input in newInputDetails)
            {
                var billing =
                    billingMap[input.BillingCode];

                TblReceiptDetail detail;

                if (oldDetailMap.TryGetValue(
                    input.BillingCode,
                    out var existingDetail))
                {
                    detail = existingDetail;
                }
                else
                {
                    detail = new TblReceiptDetail
                    {
                        ReceiptCode =
                            receipt.ReceiptCode,
                        BillingCode =
                            input.BillingCode
                    };

                    db.TblReceiptDetails.Add(
                        detail);
                }

                detail.ReceiptCode =
                    receipt.ReceiptCode;

                detail.ReceiptAdvanceValue =
                    billing.AdvanceTotal;

                detail.ReceiptTransportValue =
                    billing.TransportTotal;

                detail.ReceiptValue =
                    billing.BillingNetTotal;

                detail.BillingType =
                    billing.BillingType;

                billing.IsClearing = true;
                billing.ClearingDate =
                    receipt.ReceiptDate;
                billing.UpdateDate = now;
                billing.UpdateBy =
                    receiptToEdit.UpdateBy;

                finalDetails.Add(detail);
            }

            receipt.ReceiptAmount =
                finalDetails.Count;

            ApplyReceiptTotals(
                receipt,
                finalDetails);

            var newValues = new
            {
                Receipt =
                    GetScalarSnapshot(receipt),

                Details =
                    finalDetails
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Update receipt.",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblReceipt),
                    EntityId = receipt.ReceiptCode,
                    Description =
                        $"แก้ไขข้อมูลใบเสร็จรับเงิน {receipt.ReceiptCode} โดย {receipt.UpdateBy}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "EditReceipt",
                receiptToEdit.ReceiptCode);
        }
    }

    public Task<ResponseViewModel> EditReceiptName(
        string code,
        string name,
        string username,
        CancellationToken cancellationToken = default)
    {
        return UpdateReceiptField(
            code,
            username,
            "Update receipt name.",
            receipt => receipt.ReceiptName = name,
            cancellationToken);
    }

    public Task<ResponseViewModel> EditCustomerName(
        string code,
        string name,
        string username,
        CancellationToken cancellationToken = default)
    {
        return UpdateReceiptField(
            code,
            username,
            "Update receipt at customer name.",
            receipt => receipt.CustomerName = name,
            cancellationToken);
    }

    public Task<ResponseViewModel> EditTax(
        string code,
        string tax,
        string username,
        CancellationToken cancellationToken = default)
    {
        return UpdateReceiptField(
            code,
            username,
            "Update receipt at tax.",
            receipt => receipt.CustomerTax = tax,
            cancellationToken);
    }

    public Task<ResponseViewModel> EditAddress(
        string code,
        string address,
        string username,
        CancellationToken cancellationToken = default)
    {
        return UpdateReceiptField(
            code,
            username,
            "Update receipt at customer address.",
            receipt => receipt.CustomerAddress = address,
            cancellationToken);
    }

    public async Task<TblReceiptDetail?> GetReceiptDetailData(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblReceiptDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.BillingCode == code,
                cancellationToken);
    }

    public async Task<ResponseViewModel> EditBillingType(
        string type,
        string billing_code,
        string receipt_code,
        CancellationToken cancellationToken = default)
    {
        if (type is not ("A" or "T" or "O"))
        {
            return Failure("ประเภท Billing ไม่ถูกต้อง");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var billing = await db.TblBillingMasters
                .FirstOrDefaultAsync(
                    x => x.BillingCode ==
                         billing_code,
                    cancellationToken);

            var receipt = await db.TblReceipts
                .FirstOrDefaultAsync(
                    x => x.ReceiptCode ==
                         receipt_code,
                    cancellationToken);

            var receiptDetails = await db.TblReceiptDetails
                .Where(x =>
                    x.ReceiptCode ==
                    receipt_code)
                .ToListAsync(cancellationToken);

            var data = receiptDetails
                .FirstOrDefault(x =>
                    x.BillingCode ==
                    billing_code);

            if (billing == null ||
                receipt == null ||
                data == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Failure(
                    "ไม่พบข้อมูล Billing หรือใบเสร็จรับเงิน");
            }

            var amount =
                data.ReceiptTransportValue != 0
                    ? data.ReceiptTransportValue
                    : data.ReceiptAdvanceValue;

            if (type == "T")
            {
                data.ReceiptTransportValue = amount;
                data.ReceiptAdvanceValue = 0;

                billing.TransportTotal = amount;
                billing.AdvanceTotal = 0;
                billing.BillingTotal = amount;
                billing.BillingVat =
                    amount * 1m / 100m;
                billing.BillingNetTotal =
                    amount - billing.BillingVat;
                billing.IsVat = true;
            }
            else
            {
                data.ReceiptAdvanceValue = amount;
                data.ReceiptTransportValue = 0;

                billing.AdvanceTotal = amount;
                billing.TransportTotal = 0;
                billing.BillingTotal = amount;
                billing.BillingVat = 0;
                billing.BillingNetTotal = amount;
                billing.IsVat = false;
            }

            data.BillingType = type;
            data.ReceiptValue =
                billing.BillingNetTotal;

            billing.BillingType = type;

            ApplyReceiptTotals(
                receipt,
                receiptDetails);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "EditBillingType",
                $"{receipt_code}/{billing_code}");
        }
    }

    public async Task<ResponseViewModel> ProveData(
        string receipt_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(receipt_code))
        {
            return Failure("ไม่พบเลขที่ใบเสร็จรับเงิน");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var synchronized =
                await SynchronizeReceiptFromBillingAsync(
                    receipt_code,
                    cancellationToken);

            if (!synchronized)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure(
                    $"ไม่พบใบเสร็จรับเงิน {receipt_code}");
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "ProveData",
                receipt_code);
        }
    }

    public async Task<List<TempReceiptViewModel>> GetTemp(
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TmpRecepts.AsNoTracking()
            join b in db.TblBillingMasters.AsNoTracking()
                on a.Code equals b.BillingCode
            select new TempReceiptViewModel
            {
                billing_code = a.Code,
                transport_total = b.TransportTotal,
                advance_total = b.AdvanceTotal,
                total = b.BillingNetTotal,
                tmp_transport_total = a.TransportTotal,
                tmp_advance_total = a.AdvanceTotal,
                tmp_total = a.Total,
                status = b.IsClearing,
                receipt_date = b.ClearingDate
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ResponseViewModel> ClearTemp(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await db.TmpRecepts
                .ExecuteDeleteAsync(
                    cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "ClearTemp");
        }
    }

    public async Task<ResponseViewModel> ConfirmTemp(
        string username,
        DateOnly receiptdate,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var tmpList = await (
                from a in db.TmpRecepts.AsNoTracking()
                join b in db.TblBillingMasters
                    on a.Code equals b.BillingCode
                select new
                {
                    b.BillingCode,
                    b.BillingType,
                    b.CustomerCode,
                    b.IsEnabled,
                    b.IsClearing,
                    Billing = b,
                    a.AdvanceTotal,
                    a.TransportTotal,
                    a.Total
                })
                .ToListAsync(cancellationToken);

            if (tmpList.Count == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูล Temp Receipt");
            }

            var duplicateBilling = tmpList
                .GroupBy(x => x.BillingCode)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateBilling != null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Failure(
                    $"Billing {duplicateBilling.Key} อยู่ใน Temp ซ้ำ");
            }

            var unavailableBilling = tmpList
                .FirstOrDefault(x =>
                    x.IsEnabled != true ||
                    x.IsClearing == true);

            if (unavailableBilling != null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Failure(
                    $"Billing {unavailableBilling.BillingCode} " +
                    "ถูกยกเลิกหรือถูกออกใบเสร็จไปแล้ว");
            }

            var customerCodes = tmpList
                .Select(x => x.CustomerCode)
                .Distinct()
                .ToList();

            if (customerCodes.Count != 1)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure(
                    "Billing ใน Temp ต้องเป็นลูกค้ารายเดียวกันทั้งหมด");
            }

            var customerCode = customerCodes[0];

            var customer = await db.TblCustomers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CustomerCode ==
                         customerCode,
                    cancellationToken);

            if (customer == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure(
                    $"ไม่พบข้อมูลลูกค้า {customerCode}");
            }

            var now = DateTime.Now;
            var receiptCode =
                await codeRepo.GetReceiptCode();

            var model = new TblReceipt
            {
                ReceiptCode = receiptCode,
                ReceiptPartCode = receiptCode,
                ReceiptName = "Transport Charge",
                CustomerCode = customerCode,
                CustomerName = customer.CustomerNameTh,
                CustomerAddress = customer.CustomerAddress,
                CustomerTax = customer.CustomerVatNo,
                ReceiptAmount = tmpList.Count,
                ReceiptAdvance =
                    tmpList.Sum(x => x.AdvanceTotal),
                ReceiptTransport =
                    tmpList.Sum(x => x.TransportTotal),
                ReceiptTotal =
                    tmpList.Sum(
                        x => x.AdvanceTotal +
                             x.TransportTotal),
                ReceiptNetTotal =
                    tmpList.Sum(x => x.Total),
                ReceiptRemark = "-",
                ReceiptDate = receiptdate,
                ReceiptType = "02",
                IsExcharge = false,
                IsEnabled = true,
                CreateDate = now,
                CreateBy = username,
                UpdateDate = now,
                UpdateBy = username
            };

            var details =
                new List<TblReceiptDetail>(
                    tmpList.Count);

            foreach (var item in tmpList)
            {
                details.Add(
                    new TblReceiptDetail
                    {
                        ReceiptCode =
                            receiptCode,
                        BillingCode =
                            item.BillingCode,
                        BillingType =
                            item.BillingType,
                        ReceiptAdvanceValue =
                            item.AdvanceTotal,
                        ReceiptTransportValue =
                            item.TransportTotal,
                        ReceiptValue =
                            item.Total
                    });

                item.Billing.IsClearing = true;
                item.Billing.ClearingDate = receiptdate;
                item.Billing.UpdateDate = now;
                item.Billing.UpdateBy = username;
            }

            model.TblReceiptDetails = details;

            await db.TblReceipts.AddAsync(
                model,
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            await db.TmpRecepts
                .ExecuteDeleteAsync(
                    cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "ConfirmTemp");
        }
    }

    private async Task<ResponseViewModel> UpdateReceiptField(
        string code,
        string username,
        string logMessage,
        Action<TblReceipt> updateAction,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบเลขที่ใบเสร็จรับเงิน");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var receipt = await db.TblReceipts
                .FirstOrDefaultAsync(
                    x => x.ReceiptCode == code,
                    cancellationToken);

            if (receipt == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure($"ไม่พบใบเสร็จรับเงิน {code}");
            }

            if (receipt.IsEnabled != true)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ใบเสร็จรับเงินรายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues =
                GetScalarSnapshot(receipt);

            updateAction(receipt);

            receipt.UpdateDate = DateTime.Now;
            receipt.UpdateBy = username;

            var newValues =
                GetScalarSnapshot(receipt);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = logMessage,
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblReceipt),
                    EntityId = code,
                    Description =
                        $"{logMessage} {code} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "UpdateReceiptField",
                code);
        }
    }

    private async Task<bool> SynchronizeReceiptFromBillingAsync(
        string receiptCode,
        CancellationToken cancellationToken)
    {
        var receipt = await db.TblReceipts
            .FirstOrDefaultAsync(
                x => x.ReceiptCode == receiptCode,
                cancellationToken);

        if (receipt == null)
        {
            return false;
        }

        var receiptDetails = await db.TblReceiptDetails
            .Where(x => x.ReceiptCode == receiptCode)
            .ToListAsync(cancellationToken);

        if (receiptDetails.Count == 0)
        {
            receipt.ReceiptAmount = 0;
            receipt.ReceiptAdvance = 0;
            receipt.ReceiptTransport = 0;
            receipt.ReceiptTotal = 0;
            receipt.ReceiptNetTotal = 0;
            return true;
        }

        var billingCodes = receiptDetails
            .Select(x => x.BillingCode)
            .Distinct()
            .ToList();

        var billings = await db.TblBillingMasters
            .Where(x =>
                billingCodes.Contains(
                    x.BillingCode))
            .ToListAsync(cancellationToken);

        if (billings.Count != billingCodes.Count)
        {
            throw new InvalidOperationException(
                "พบ Billing ในใบเสร็จบางรายการไม่อยู่ในฐานข้อมูล");
        }

        await RecalculateBillingTotalsAsync(
            billings,
            cancellationToken);

        var billingMap =
            billings.ToDictionary(
                x => x.BillingCode);

        foreach (var detail in receiptDetails)
        {
            var billing =
                billingMap[detail.BillingCode];

            detail.BillingType =
                billing.BillingType;

            detail.ReceiptAdvanceValue =
                billing.AdvanceTotal;

            detail.ReceiptTransportValue =
                billing.TransportTotal;

            detail.ReceiptValue =
                billing.BillingNetTotal;
        }

        receipt.ReceiptAmount =
            receiptDetails.Count;

        ApplyReceiptTotals(
            receipt,
            receiptDetails);

        return true;
    }

    private async Task RecalculateBillingTotalsAsync(
        IReadOnlyCollection<TblBillingMaster> billings,
        CancellationToken cancellationToken = default)
    {
        if (billings.Count == 0)
        {
            return;
        }

        var normalCodes = billings
            .Where(x => x.BillingType != "O")
            .Select(x => x.BillingCode)
            .Distinct()
            .ToList();

        var otherCodes = billings
            .Where(x => x.BillingType == "O")
            .Select(x => x.BillingCode)
            .Distinct()
            .ToList();

        var normalTotals =
            normalCodes.Count == 0
                ? new Dictionary<
                    string,
                    (decimal Advance, decimal Transport)>()
                : (await db.TblBillingDetails
                    .AsNoTracking()
                    .Where(x =>
                        normalCodes.Contains(
                            x.BillingCode))
                    .GroupBy(x => x.BillingCode)
                    .Select(g => new
                    {
                        BillingCode = g.Key,
                        Advance =
                            g.Sum(x => x.AdvanceValue),
                        Transport =
                            g.Sum(x => x.BillingValue)
                    })
                    .ToListAsync(cancellationToken))
                    .ToDictionary(
                        x => x.BillingCode,
                        x => (
                            Advance: x.Advance,
                            Transport: x.Transport));

        var otherTotals =
            otherCodes.Count == 0
                ? new Dictionary<string, decimal>()
                : (await db.TblBillingOthers
                    .AsNoTracking()
                    .Where(x =>
                        otherCodes.Contains(
                            x.BillingCode))
                    .GroupBy(x => x.BillingCode)
                    .Select(g => new
                    {
                        BillingCode = g.Key,
                        Advance =
                            g.Sum(x => x.BillingValue)
                    })
                    .ToListAsync(cancellationToken))
                    .ToDictionary(
                        x => x.BillingCode,
                        x => x.Advance);

        foreach (var billing in billings)
        {
            decimal totalAdvance = 0;
            decimal totalTransport = 0;

            if (billing.BillingType != "O")
            {
                if (normalTotals.TryGetValue(
                    billing.BillingCode,
                    out var total))
                {
                    totalAdvance =
                        total.Advance;

                    totalTransport =
                        total.Transport;
                }
            }
            else if (otherTotals.TryGetValue(
                billing.BillingCode,
                out var otherAdvance))
            {
                totalAdvance =
                    otherAdvance;
            }

            var vat =
                totalTransport * 1m / 100m;

            billing.AdvanceTotal =
                totalAdvance;

            billing.TransportTotal =
                totalTransport;

            billing.BillingTotal =
                totalTransport +
                totalAdvance;

            if (billing.IsVat == true)
            {
                billing.BillingVat = vat;
                billing.BillingNetTotal =
                    (totalTransport - vat) +
                    totalAdvance;
            }
            else
            {
                billing.BillingVat = 0;
                billing.BillingNetTotal =
                    totalTransport +
                    totalAdvance;
            }
        }
    }

    private static void ApplyReceiptTotals(
        TblReceipt receipt,
        IReadOnlyCollection<TblReceiptDetail> details)
    {
        receipt.ReceiptAdvance =
            details.Sum(
                x => x.ReceiptAdvanceValue);

        receipt.ReceiptTransport =
            details.Sum(
                x => x.ReceiptTransportValue);

        receipt.ReceiptTotal =
            receipt.ReceiptAdvance +
            receipt.ReceiptTransport;

        receipt.ReceiptNetTotal =
            details.Sum(
                x => x.ReceiptValue);
    }

    private Dictionary<string, object?> GetScalarSnapshot(
        object entity)
    {
        return db.Entry(entity)
            .Properties
            .ToDictionary(
                p => p.Metadata.Name,
                p => p.CurrentValue);
    }

    private void CopyEditableScalarValues<TEntity>(
        TEntity target,
        TEntity source,
        ISet<string> protectedProperties)
        where TEntity : class
    {
        var targetEntry = db.Entry(target);
        var sourceEntry = db.Entry(source);

        var sourceProperties = sourceEntry
            .Properties
            .ToDictionary(
                p => p.Metadata.Name,
                p => p,
                StringComparer.OrdinalIgnoreCase);

        foreach (var property in targetEntry.Properties)
        {
            var propertyName =
                property.Metadata.Name;

            if (protectedProperties.Contains(
                propertyName))
            {
                continue;
            }

            if (sourceProperties.TryGetValue(
                propertyName,
                out var sourceProperty))
            {
                property.CurrentValue =
                    sourceProperty.CurrentValue;
            }
        }
    }

    private static ResponseViewModel? ValidateReceiptDetails(
        IReadOnlyCollection<TblReceiptDetail> details)
    {
        if (details.Count == 0)
        {
            return Failure(
                "ไม่พบ Billing สำหรับสร้างใบเสร็จรับเงิน");
        }

        if (details.Any(x =>
            string.IsNullOrWhiteSpace(
                x.BillingCode)))
        {
            return Failure(
                "พบรายการ Billing ที่ไม่มี BillingCode");
        }

        var duplicateBilling = details
            .GroupBy(x => x.BillingCode)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateBilling != null)
        {
            return Failure(
                $"Billing {duplicateBilling.Key} ถูกเลือกซ้ำ");
        }

        return null;
    }

    private static (
        DateOnly StartDate,
        DateOnly EndDate) GetMonthRange(
        string month,
        string year)
    {
        if (!int.TryParse(
                month,
                out var monthValue) ||
            !int.TryParse(
                year,
                out var yearValue) ||
            monthValue < 1 ||
            monthValue > 12 ||
            yearValue < 1 ||
            yearValue > 9999)
        {
            throw new ArgumentException(
                "เดือนหรือปีไม่ถูกต้อง");
        }

        var startDate =
            new DateOnly(
                yearValue,
                monthValue,
                1);

        return (
            startDate,
            startDate.AddMonths(1));
    }

    private static ResponseViewModel Success()
    {
        return new ResponseViewModel
        {
            result = true,
            resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว"
        };
    }

    private static ResponseViewModel Failure(
        string message)
    {
        return new ResponseViewModel
        {
            result = false,
            resultMessage = message
        };
    }

    private ResponseViewModel Error(
        Exception ex,
        string operation,
        string? entityId = null)
    {
        logger.LogError(
            ex,
            "Error in {Operation}. EntityId: {EntityId}",
            operation,
            entityId);

        return new ResponseViewModel
        {
            result = false,
            resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>"
        };
    }

    private async Task RollbackSafelyAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        try
        {
            await transaction.RollbackAsync(
                cancellationToken);
        }
        catch (Exception rollbackEx)
        {
            logger.LogError(
                rollbackEx,
                "Error while rolling back Receipt transaction.");
        }
    }
}
