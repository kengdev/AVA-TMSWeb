using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class CoverSheetRepository
{
    private readonly DbavalogContext db;
    private readonly IAuditLogService auditLogService;
    private readonly CodeRepository codeRepo;
    private readonly ILogger<CoverSheetRepository> logger;

    public CoverSheetRepository(
        DbavalogContext context,
        IAuditLogService _auditLogService,
        CodeRepository _codeRepo,
        ILogger<CoverSheetRepository> _logger)
    {
        db = context;
        auditLogService = _auditLogService;
        codeRepo = _codeRepo;
        logger = _logger;
    }

    public async Task<List<CoverSheetViewModel>> GetAllCoverSheetDataList(
        string month,
        string year,
        CancellationToken cancellationToken = default)
    {
        var (startDate, endDate) = GetMonthRange(month, year);

        var query = await (
            from a in db.TblCoversheets.AsNoTracking()
            join b in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals b.CustomerCode
            where a.IsEnabled == true
                  && a.CoversheetDate >= startDate
                  && a.CoversheetDate < endDate
            select new
            {
                a.CoversheetCode,
                b.CustomerAcronym,
                b.CustomerCode,
                a.CoversheetDate,
                a.CoversheetTotal,
                a.CoversheetAmount
            })
            .ToListAsync(cancellationToken);

        return query
            .Select(a => new CoverSheetViewModel
            {
                coversheet_code = a.CoversheetCode,
                customer_name = a.CustomerAcronym,
                customer_code = a.CustomerCode,
                coversheet_date = a.CoversheetDate.ToString("yyyy-MM-dd"),
                coversheet_total = a.CoversheetTotal,
                coversheet_amount = a.CoversheetAmount
            })
            .ToList();
    }

    public async Task<List<CoverSheetViewModel>> GetBillingDataList(
        CancellationToken cancellationToken = default)
    {
        var query = await (
            from a in db.TblBillingMasters.AsNoTracking()
            join b in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals b.CustomerCode
            where a.IsCoversheet == false
                  && a.IsClearing == false
                  && a.IsEnabled == true
            select new
            {
                a.BillingCode,
                b.CustomerAcronym,
                a.BillingDate,
                a.BillingNetTotal
            })
            .ToListAsync(cancellationToken);

        return query
            .Select(a => new CoverSheetViewModel
            {
                billing_code = a.BillingCode,
                customer_name = a.CustomerAcronym,
                billing_date = a.BillingDate.ToString("yyyy-MM-dd"),
                billing_total = a.BillingNetTotal
            })
            .ToList();
    }

    public Task<List<CoverSheetViewModel>> GetCoverSheetDataList(
        string month,
        string year,
        CancellationToken cancellationToken = default)
    {
        return GetAllCoverSheetDataList(
            month,
            year,
            cancellationToken);
    }

    public async Task<TblCoversheet?> GetCoverSheetData(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblCoversheets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.CoversheetCode == code,
                cancellationToken);
    }

    public async Task<List<TblCoversheetDetail>> GetCoverSheetDetailListData(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new List<TblCoversheetDetail>();
        }

        return await db.TblCoversheetDetails
            .AsNoTracking()
            .Where(w => w.CoversheetCode == code)
            .OrderBy(o => o.BillingCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<TblCoversheetDetail?> GetCoverSheetDetailData(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblCoversheetDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.BillingCode == code,
                cancellationToken);
    }

    public async Task<CoverSheetViewModel?> GetBillingBycode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var data = await (
            from a in db.TblBillingMasters.AsNoTracking()
            join b in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals b.CustomerCode
            where a.BillingCode == code
            select new
            {
                a.BillingCode,
                a.CreateDate,
                a.BillingDate,
                b.CustomerAcronym,
                a.BillingNetTotal
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data == null)
        {
            return null;
        }

        return new CoverSheetViewModel
        {
            billing_code = data.BillingCode,
            create_date = data.CreateDate,
            billing_date = data.BillingDate.ToString("dd-MM-yyyy"),
            customer_name = data.CustomerAcronym,
            billing_total = data.BillingNetTotal
        };
    }

    public async Task<ResponseViewModel> RemoveCoverSheet(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบเลขที่ Coversheet");
        }

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var cover = await db.TblCoversheets
                .FirstOrDefaultAsync(
                    w => w.CoversheetCode == code,
                    cancellationToken);

            if (cover == null)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure($"ไม่พบ Coversheet {code}");
            }

            if (cover.IsEnabled != true)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure("Coversheet รายการนี้ถูกยกเลิกไปแล้ว");
            }

            if (cover.IsClearing == true)
            {
                await tr.RollbackAsync(cancellationToken);

                return new ResponseViewModel
                {
                    result = false,
                    resultMessage =
                        "<div>ไม่สามารถลบรายการได้</div>" +
                        "<div style='margin-top:-30px'> " +
                        "เนื่องจากรายการดังกล่าวได้ออกใบเสร็จรับเงินเรียบร้อยแล้ว</div>"
                };
            }

            var coverDetails = await db.TblCoversheetDetails
                .Where(w => w.CoversheetCode == code)
                .ToListAsync(cancellationToken);

            var oldValues = new
            {
                CoverSheet = GetScalarSnapshot(cover),
                Details = coverDetails
                    .Select(GetScalarSnapshot)
                    .ToList()
            };

            var billingCodes = coverDetails
                .Select(w => w.BillingCode)
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Distinct()
                .ToList();

            var billings = billingCodes.Count == 0
                ? new Dictionary<string, TblBillingMaster>()
                : await db.TblBillingMasters
                    .Where(w => billingCodes.Contains(w.BillingCode))
                    .ToDictionaryAsync(
                        w => w.BillingCode,
                        cancellationToken);

            var now = DateTime.Now;

            foreach (var item in coverDetails)
            {
                if (billings.TryGetValue(item.BillingCode, out var billing))
                {
                    billing.IsCoversheet = false;
                    billing.UpdateDate = now;
                    billing.UpdateBy = username;
                }
            }

            if (coverDetails.Count > 0)
            {
                db.TblCoversheetDetails.RemoveRange(coverDetails);
            }

            cover.IsEnabled = false;
            cover.UpdateDate = now;
            cover.UpdateBy = username;

            var newValues = new
            {
                CoverSheet = GetScalarSnapshot(cover),
                ReleasedBillingCodes = billingCodes
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Remove coversheet.",
                    ActionName = AuditActions.Delete,
                    EntityName = nameof(TblCoversheet),
                    EntityId = code,
                    Description = $"ลบข้อมูลใบคุมการแจ้งหนี้ {code} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr, cancellationToken);
            return Error(ex, "RemoveCoverSheet", code);
        }
    }

    public async Task<ResponseViewModel> AddCoverSheet(
        TblCoversheet coverToAdd,
        CancellationToken cancellationToken = default)
    {
        if (coverToAdd == null)
        {
            return Failure("ไม่พบข้อมูล Coversheet");
        }

        var details = coverToAdd.TblCoversheetDetails?
            .ToList() ?? new List<TblCoversheetDetail>();

        var validation = ValidateCoverSheetDetails(details);
        if (validation != null)
        {
            return validation;
        }

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var billingCodes = details
                .Select(w => w.BillingCode)
                .Distinct()
                .ToList();

            var billings = await db.TblBillingMasters
                .Where(w => billingCodes.Contains(w.BillingCode))
                .ToListAsync(cancellationToken);

            if (billings.Count != billingCodes.Count)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure("พบ Billing บางรายการไม่อยู่ในฐานข้อมูล");
            }

            var invalidBilling = billings.FirstOrDefault(w =>
                w.IsEnabled != true ||
                w.IsClearing == true ||
                w.IsCoversheet == true);

            if (invalidBilling != null)
            {
                await tr.RollbackAsync(cancellationToken);

                return Failure(
                    $"Billing {invalidBilling.BillingCode} ไม่สามารถนำมาสร้าง Coversheet ได้ " +
                    "เนื่องจากถูกยกเลิก เคลียร์แล้ว หรืออยู่ใน Coversheet อื่นแล้ว");
            }

            var customerCodes = billings
                .Select(w => w.CustomerCode)
                .Distinct()
                .ToList();

            if (customerCodes.Count != 1)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure("Billing ที่เลือกต้องเป็นลูกค้ารายเดียวกันทั้งหมด");
            }

            await RecalculateBillingTotalsAsync(
                billings,
                cancellationToken);

            var billingMap = billings.ToDictionary(w => w.BillingCode);

            coverToAdd.CoversheetCode =
                await codeRepo.GetCoverSheetCode(cancellationToken);

            coverToAdd.CustomerCode = customerCodes[0];
            coverToAdd.CoversheetAmount = details.Count;
            coverToAdd.ClearingDate = null;
            coverToAdd.IsClearing = false;
            coverToAdd.IsEnabled = true;

            var now = DateTime.Now;

            foreach (var detail in details)
            {
                var billing = billingMap[detail.BillingCode];

                billing.IsCoversheet = true;
                billing.UpdateDate = now;
                billing.UpdateBy = coverToAdd.UpdateBy;

                detail.CoversheetCode = coverToAdd.CoversheetCode;
                detail.CoversheetValue = billing.BillingNetTotal;
            }

            coverToAdd.CoversheetTotal =
                details.Sum(w => w.CoversheetValue);

            db.TblCoversheets.Add(coverToAdd);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr, cancellationToken);
            return Error(
                ex,
                "AddCoverSheet",
                coverToAdd.CoversheetCode);
        }
    }

    public async Task<ResponseViewModel> EditCoverSheet(
        TblCoversheet coverToEdit,
        CancellationToken cancellationToken = default)
    {
        if (coverToEdit == null ||
            string.IsNullOrWhiteSpace(coverToEdit.CoversheetCode))
        {
            return Failure("ไม่พบข้อมูล Coversheet");
        }

        var newDetails = coverToEdit.TblCoversheetDetails?
            .ToList() ?? new List<TblCoversheetDetail>();

        var validation = ValidateCoverSheetDetails(newDetails);
        if (validation != null)
        {
            return validation;
        }

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var cover = await db.TblCoversheets
                .FirstOrDefaultAsync(
                    w => w.CoversheetCode == coverToEdit.CoversheetCode,
                    cancellationToken);

            if (cover == null)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure(
                    $"ไม่พบ Coversheet {coverToEdit.CoversheetCode}");
            }

            if (cover.IsEnabled != true)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure("Coversheet รายการนี้ถูกยกเลิกไปแล้ว");
            }

            if (cover.IsClearing == true)
            {
                await tr.RollbackAsync(cancellationToken);

                return Failure(
                    "ไม่สามารถแก้ไข Coversheet ที่ออกใบเสร็จรับเงินเรียบร้อยแล้ว");
            }

            var oldDetails = await db.TblCoversheetDetails
                .Where(w =>
                    w.CoversheetCode == coverToEdit.CoversheetCode)
                .ToListAsync(cancellationToken);

            var oldValues = new
            {
                CoverSheet = GetScalarSnapshot(cover),
                Details = oldDetails
                    .Select(GetScalarSnapshot)
                    .ToList()
            };

            var oldBillingCodes = oldDetails
                .Select(w => w.BillingCode)
                .Distinct()
                .ToHashSet();

            var newBillingCodes = newDetails
                .Select(w => w.BillingCode)
                .Distinct()
                .ToList();

            var allBillingCodes = oldBillingCodes
                .Union(newBillingCodes)
                .ToList();

            var billings = await db.TblBillingMasters
                .Where(w => allBillingCodes.Contains(w.BillingCode))
                .ToListAsync(cancellationToken);

            var billingMap = billings
                .ToDictionary(w => w.BillingCode);

            var selectedBillings = billings
                .Where(w => newBillingCodes.Contains(w.BillingCode))
                .ToList();

            if (selectedBillings.Count != newBillingCodes.Count)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure("พบ Billing บางรายการไม่อยู่ในฐานข้อมูล");
            }

            var invalidBilling = selectedBillings.FirstOrDefault(w =>
                w.IsEnabled != true ||
                w.IsClearing == true ||
                (w.IsCoversheet == true &&
                 !oldBillingCodes.Contains(w.BillingCode)));

            if (invalidBilling != null)
            {
                await tr.RollbackAsync(cancellationToken);

                return Failure(
                    $"Billing {invalidBilling.BillingCode} ไม่สามารถนำมาใช้กับ Coversheet นี้ได้ " +
                    "เนื่องจากถูกยกเลิก เคลียร์แล้ว หรืออยู่ใน Coversheet อื่นแล้ว");
            }

            var customerCodes = selectedBillings
                .Select(w => w.CustomerCode)
                .Distinct()
                .ToList();

            if (customerCodes.Count != 1)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure("Billing ที่เลือกต้องเป็นลูกค้ารายเดียวกันทั้งหมด");
            }

            var now = DateTime.Now;

            foreach (var item in oldDetails)
            {
                if (!billingMap.TryGetValue(
                        item.BillingCode,
                        out var oldBilling))
                {
                    continue;
                }

                if (!newBillingCodes.Contains(item.BillingCode))
                {
                    oldBilling.IsCoversheet = false;
                    oldBilling.UpdateDate = now;
                    oldBilling.UpdateBy = coverToEdit.UpdateBy;
                }
            }

            await RecalculateBillingTotalsAsync(
                selectedBillings,
                cancellationToken);

            foreach (var detail in newDetails)
            {
                var billing = billingMap[detail.BillingCode];

                billing.IsCoversheet = true;
                billing.UpdateDate = now;
                billing.UpdateBy = coverToEdit.UpdateBy;

                detail.CoversheetCode =
                    coverToEdit.CoversheetCode;

                detail.CoversheetValue =
                    billing.BillingNetTotal;
            }

            CopyEditableScalarValues(
                cover,
                coverToEdit,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    nameof(TblCoversheet.CoversheetCode),
                    nameof(TblCoversheet.CustomerCode),
                    nameof(TblCoversheet.CoversheetAmount),
                    nameof(TblCoversheet.CoversheetTotal),
                    nameof(TblCoversheet.IsEnabled),
                    nameof(TblCoversheet.IsClearing),
                    nameof(TblCoversheet.ClearingDate),
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            cover.CustomerCode = customerCodes[0];
            cover.CoversheetAmount = newDetails.Count;
            cover.CoversheetTotal =
                newDetails.Sum(w => w.CoversheetValue);

            if (oldDetails.Count > 0)
            {
                db.TblCoversheetDetails.RemoveRange(oldDetails);
            }

            db.TblCoversheetDetails.AddRange(newDetails);

            var newValues = new
            {
                CoverSheet = GetScalarSnapshot(cover),
                Details = newDetails
                    .Select(GetScalarSnapshot)
                    .ToList()
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Update coversheet.",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblCoversheet),
                    EntityId = cover.CoversheetCode,
                    Description =
                        $"แก้ไขข้อมูล CoverSheet {cover.CoversheetCode} โดย {cover.UpdateBy}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr, cancellationToken);

            return Error(
                ex,
                "EditCoverSheet",
                coverToEdit.CoversheetCode);
        }
    }

    public async Task<ResponseViewModel> ProveBilling(
        string billing_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(billing_code))
        {
            return Failure("ไม่พบเลขที่ Billing");
        }

        try
        {
            var billing = await db.TblBillingMasters
                .FirstOrDefaultAsync(
                    w => w.BillingCode == billing_code,
                    cancellationToken);

            if (billing == null)
            {
                return Failure($"ไม่พบ Billing {billing_code}");
            }

            if (billing.IsEnabled != true)
            {
                return Failure("Billing รายการนี้ถูกยกเลิกไปแล้ว");
            }

            await RecalculateBillingTotalsAsync(
                new List<TblBillingMaster>
                {
                    billing
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "ProveBilling",
                billing_code);
        }
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
            .Where(w => w.BillingType != "O")
            .Select(w => w.BillingCode)
            .Distinct()
            .ToList();

        var otherCodes = billings
            .Where(w => w.BillingType == "O")
            .Select(w => w.BillingCode)
            .Distinct()
            .ToList();

        var normalTotals =
            normalCodes.Count == 0
                ? new Dictionary<
                    string,
                    (decimal Advance, decimal Transport)>()
                : (await db.TblBillingDetails
                    .AsNoTracking()
                    .Where(w =>
                        normalCodes.Contains(
                            w.BillingCode))
                    .GroupBy(w => w.BillingCode)
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
                        w => w.BillingCode,
                        w => (
                            Advance: w.Advance,
                            Transport: w.Transport));

        var otherTotals =
            otherCodes.Count == 0
                ? new Dictionary<string, decimal>()
                : (await db.TblBillingOthers
                    .AsNoTracking()
                    .Where(w =>
                        otherCodes.Contains(
                            w.BillingCode))
                    .GroupBy(w => w.BillingCode)
                    .Select(g => new
                    {
                        BillingCode = g.Key,
                        Advance =
                            g.Sum(x => x.BillingValue)
                    })
                    .ToListAsync(cancellationToken))
                    .ToDictionary(
                        w => w.BillingCode,
                        w => w.Advance);

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
                    totalAdvance = total.Advance;
                    totalTransport = total.Transport;
                }
            }
            else
            {
                if (otherTotals.TryGetValue(
                    billing.BillingCode,
                    out var total))
                {
                    totalAdvance = total;
                }
            }

            var vat =
                totalTransport * 1m / 100m;

            billing.AdvanceTotal = totalAdvance;
            billing.TransportTotal = totalTransport;
            billing.BillingTotal =
                totalTransport + totalAdvance;

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

    private static ResponseViewModel? ValidateCoverSheetDetails(
        IReadOnlyCollection<TblCoversheetDetail> details)
    {
        if (details.Count == 0)
        {
            return Failure(
                "ไม่พบรายการ Billing สำหรับสร้าง Coversheet");
        }

        if (details.Any(w =>
            string.IsNullOrWhiteSpace(w.BillingCode)))
        {
            return Failure(
                "พบรายการ Billing ที่ไม่มี BillingCode");
        }

        var duplicateBilling = details
            .GroupBy(w => w.BillingCode)
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
        if (!int.TryParse(month, out var monthValue) ||
            !int.TryParse(year, out var yearValue) ||
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
                "Error while rolling back Coversheet transaction.");
        }
    }
}
