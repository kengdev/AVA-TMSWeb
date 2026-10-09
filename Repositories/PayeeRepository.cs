using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class PayeeRepository
{
    private readonly DbavalogContext db;
    private readonly IAuditLogService auditLogService;
    private readonly CodeRepository codeRepo;
    private readonly ILogger<PayeeRepository> logger;

    public PayeeRepository(
        DbavalogContext context,
        IAuditLogService _auditLogService,
        CodeRepository _codeRepo,
        ILogger<PayeeRepository> _logger)
    {
        db = context;
        auditLogService = _auditLogService;
        codeRepo = _codeRepo;
        logger = _logger;
    }

    public async Task<TblReceivePayment?> GetReceivePaymentData(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblReceivePayments
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.SupplierCodeNavigation)
            .Include(x => x.TblReceivePaymentDetails)
            .FirstOrDefaultAsync(
                w => w.ReceivePaymentCode == code,
                cancellationToken);
    }

    public async Task<ResponseViewModel> EditReceivePayment(
        TblReceivePayment model,
        CancellationToken cancellationToken = default)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.ReceivePaymentCode))
        {
            return Failure("ไม่พบข้อมูลการรับยอดรายเดือน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var payment = await db.TblReceivePayments
                .FirstOrDefaultAsync(
                    w => w.ReceivePaymentCode ==
                         model.ReceivePaymentCode,
                    cancellationToken);

            if (payment == null)
            {
                await tr.RollbackAsync(cancellationToken);

                return Failure(
                    $"ไม่พบข้อมูลการรับยอดรายเดือน {model.ReceivePaymentCode}");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure("รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldDetails = await db.TblReceivePaymentDetails
                .AsNoTracking()
                .Where(w =>
                    w.ReceivePaymentCode ==
                    model.ReceivePaymentCode)
                .ToListAsync(cancellationToken);

            var oldReceipts = await db.TblReceivePaymentReceipts
                .AsNoTracking()
                .Where(w =>
                    w.ReceivePaymentCode ==
                    model.ReceivePaymentCode)
                .ToListAsync(cancellationToken);

            var oldValues = new
            {
                ReceivePayment =
                    GetScalarSnapshot(payment),

                Details =
                    oldDetails
                        .Select(GetScalarSnapshot)
                        .ToList(),

                Receipts =
                    oldReceipts
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            CopyEditableScalarValues(
                payment,
                model,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    nameof(TblReceivePayment.ReceivePaymentCode),
                    nameof(TblReceivePayment.IsEnabled),
                    nameof(TblReceivePayment.IsPaid),
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            var now = DateTime.Now;
            payment.UpdateDate = now;
            payment.UpdateBy = model.UpdateBy;

            await db.TblReceivePaymentDetails
                .Where(w =>
                    w.ReceivePaymentCode ==
                    model.ReceivePaymentCode)
                .ExecuteDeleteAsync(cancellationToken);

            var newDetails =
                model.TblReceivePaymentDetails?
                    .ToList() ??
                new List<TblReceivePaymentDetail>();

            foreach (var item in newDetails)
            {
                item.ReceivePaymentCode =
                    model.ReceivePaymentCode;
            }

            if (newDetails.Count > 0)
            {
                db.TblReceivePaymentDetails.AddRange(
                    newDetails);
            }

            await SynchronizeWithholdingTaxAsync(
                payment,
                model,
                cancellationToken);

            var newValues = new
            {
                ReceivePayment =
                    GetScalarSnapshot(payment),

                Details =
                    newDetails
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Update Receive Payment.",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblReceivePayment),
                    EntityId = model.ReceivePaymentCode,
                    Description =
                        $"ข้อมูลการรับยอดรายเดือน {model.ReceivePaymentCode} โดย {model.UpdateBy}",
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
            await RollbackSafelyAsync(
                tr,
                cancellationToken);

            return Error(
                ex,
                "EditReceivePayment",
                model.ReceivePaymentCode);
        }
    }

    public async Task<ResponseViewModel> AddReceivePayment(
        TblReceivePayment model,
        CancellationToken cancellationToken = default)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.ReceivePaymentCode))
        {
            return Failure("ไม่พบข้อมูลการรับยอดรายเดือน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var exists = await db.TblReceivePayments
                .AsNoTracking()
                .AnyAsync(
                    w => w.ReceivePaymentCode ==
                         model.ReceivePaymentCode,
                    cancellationToken);

            if (exists)
            {
                await tr.RollbackAsync(cancellationToken);

                return Failure(
                    $"รหัส {model.ReceivePaymentCode} มีอยู่ในระบบแล้ว");
            }

            var newDetails =
                model.TblReceivePaymentDetails?
                    .ToList() ??
                new List<TblReceivePaymentDetail>();

            foreach (var item in newDetails)
            {
                item.ReceivePaymentCode =
                    model.ReceivePaymentCode;
            }

            model.IsEnabled = true;

            if (model.IsTax == true)
            {
                var now = DateTime.Now;

                var pnd = new TblWithholdingTax
                {
                    WithholdingTaxCode =
                        await codeRepo.GetTaxPaymentCode(),

                    WithholdingTaxDate =
                        model.PaymentDate,

                    PaymentTypeCode =
                        model.PaymentTypeCode,

                    SupplierCode =
                        model.SupplierCode,

                    PaymentTotal =
                        model.Total,

                    TaxTotal =
                        model.Tax,

                    PaymentNetTotal =
                        model.Total - model.Tax,

                    IsEnabled = true,
                    CreateDate = now,
                    CreateBy = model.CreateBy,
                    UpdateDate = now,
                    UpdateBy = model.CreateBy
                };

                db.TblWithholdingTaxes.Add(pnd);

                db.TblWithholdingTaxPayments.Add(
                    new TblWithholdingTaxPayment
                    {
                        WithholdingTaxCode =
                            pnd.WithholdingTaxCode,

                        // Business rule เดิมของระบบ:
                        // field MonthlyPaymentCode ใช้เก็บ ReceivePaymentCode
                        MonthlyPaymentCode =
                            model.ReceivePaymentCode,

                        CreateDate = now,
                        CreateBy = model.CreateBy
                    });
            }

            db.TblReceivePayments.Add(model);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                tr,
                cancellationToken);

            return Error(
                ex,
                "AddReceivePayment",
                model.ReceivePaymentCode);
        }
    }

    public async Task<List<PaymentModel>> GetReceivePaymentDataList(
        string month,
        string year,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(month, out var m) ||
            !int.TryParse(year, out var y) ||
            m < 1 ||
            m > 12 ||
            y < 1 ||
            y > 9999)
        {
            throw new ArgumentException(
                "เดือนหรือปีไม่ถูกต้อง");
        }

        /*
         * PaymentDate อาจ map เป็น DateOnly หรือ DateTime
         * ตาม Entity ของโปรเจกต์ จึงคง Month/Year ไว้เพื่อรักษา
         * compatibility ของ repository นี้กับ model ปัจจุบัน
         */
        var rawData = await (
            from a in db.TblReceivePayments.AsNoTracking()
            join b in db.TblSuppliers.AsNoTracking()
                on a.SupplierCode equals b.SupplierCode
            where a.PaymentDate.Month == m
                  && a.PaymentDate.Year == y
                  && a.IsEnabled == true
            select new
            {
                a.PaymentDate,
                a.ReceivePaymentCode,
                a.PaymentTopic,
                a.NetTotal,
                a.IsPaid
            })
            .ToListAsync(cancellationToken);

        return rawData
            .Select(a => new PaymentModel
            {
                payment_date =
                    a.PaymentDate.ToString("yyyy-MM-dd"),

                payment_id =
                    a.ReceivePaymentCode,

                payment_name =
                    a.PaymentTopic,

                total =
                    a.NetTotal,

                payment_status =
                    a.IsPaid
            })
            .ToList();
    }

    public async Task<ResponseViewModel> RemoveReceivePayment(
        string code,
        string update_by,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure(
                "ไม่พบรหัสรายการที่ต้องการลบ");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var payment = await db.TblReceivePayments
                .FirstOrDefaultAsync(
                    w => w.ReceivePaymentCode == code,
                    cancellationToken);

            if (payment == null)
            {
                await tr.RollbackAsync(cancellationToken);

                return Failure(
                    "ไม่พบข้อมูลที่ต้องการลบ");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure("รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            var taxPayments = await db.TblWithholdingTaxPayments
                .AsNoTracking()
                .Where(w =>
                    w.MonthlyPaymentCode == code)
                .ToListAsync(cancellationToken);

            var withholdingTaxCodes = taxPayments
                .Select(w => w.WithholdingTaxCode)
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Distinct()
                .ToList();

            var withholdingTaxes =
                withholdingTaxCodes.Count == 0
                    ? new List<TblWithholdingTax>()
                    : await db.TblWithholdingTaxes
                        .Where(w =>
                            withholdingTaxCodes.Contains(
                                w.WithholdingTaxCode))
                        .ToListAsync(cancellationToken);

            var now = DateTime.Now;

            payment.IsEnabled = false;
            payment.UpdateDate = now;
            payment.UpdateBy = update_by;

            foreach (var withholdingTax in withholdingTaxes)
            {
                withholdingTax.IsEnabled = false;
                withholdingTax.UpdateDate = now;
                withholdingTax.UpdateBy = update_by;
            }

            var newValues = new
            {
                ReceivePayment =
                    GetScalarSnapshot(payment),

                DisabledWithholdingTaxCodes =
                    withholdingTaxCodes
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Remove Receive Payment.",

                    ActionName =
                        AuditActions.Delete,

                    EntityName =
                        nameof(TblReceivePayment),

                    EntityId =
                        code,

                    Description =
                        $"ลบข้อมูลการรับยอดรายเดือน {code} โดย {update_by}",

                    OldValues =
                        oldValues,

                    NewValues =
                        newValues,

                    Result =
                        AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                tr,
                cancellationToken);

            return Error(
                ex,
                "RemoveReceivePayment",
                code);
        }
    }

    public async Task<ResponseViewModel> AddReceipt(
        TblReceivePaymentReceipt model,
        CancellationToken cancellationToken = default)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(
                model.ReceivePaymentCode))
        {
            return Failure(
                "ไม่พบข้อมูล Receive Payment");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var payment = await db.TblReceivePayments
                .FirstOrDefaultAsync(
                    w => w.ReceivePaymentCode ==
                         model.ReceivePaymentCode,
                    cancellationToken);

            if (payment == null)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure(
                    "ไม่พบข้อมูล Receive Payment");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure(
                    "Receive Payment รายการนี้ถูกยกเลิกไปแล้ว");
            }

            if (payment.IsPaid == true)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure(
                    "Receive Payment รายการนี้บันทึกการรับเงินแล้ว");
            }

            var hasReceipt =
                await db.TblReceivePaymentReceipts
                    .AsNoTracking()
                    .AnyAsync(
                        w => w.ReceivePaymentCode ==
                             model.ReceivePaymentCode,
                        cancellationToken);

            if (hasReceipt)
            {
                await tr.RollbackAsync(cancellationToken);
                return Failure(
                    "Receive Payment รายการนี้มีหลักฐานการรับเงินอยู่แล้ว");
            }

            payment.IsPaid = true;

            db.TblReceivePaymentReceipts.Add(model);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                tr,
                cancellationToken);

            return Error(
                ex,
                "AddReceipt",
                model.ReceivePaymentCode);
        }
    }

    private async Task SynchronizeWithholdingTaxAsync(
        TblReceivePayment payment,
        TblReceivePayment input,
        CancellationToken cancellationToken)
    {
        var taxPayment =
            await db.TblWithholdingTaxPayments
                .FirstOrDefaultAsync(
                    w =>
                        w.MonthlyPaymentCode ==
                        payment.ReceivePaymentCode,
                    cancellationToken);

        TblWithholdingTax? withholdingTax = null;

        if (taxPayment != null &&
            !string.IsNullOrWhiteSpace(
                taxPayment.WithholdingTaxCode))
        {
            withholdingTax =
                await db.TblWithholdingTaxes
                    .FirstOrDefaultAsync(
                        w =>
                            w.WithholdingTaxCode ==
                            taxPayment.WithholdingTaxCode,
                        cancellationToken);
        }

        var now = DateTime.Now;
        var username =
            !string.IsNullOrWhiteSpace(input.UpdateBy)
                ? input.UpdateBy
                : input.CreateBy;

        if (input.IsTax == true)
        {
            if (withholdingTax == null)
            {
                withholdingTax =
                    new TblWithholdingTax
                    {
                        WithholdingTaxCode =
                            await codeRepo.GetTaxPaymentCode(),

                        WithholdingTaxDate =
                            input.PaymentDate,

                        PaymentTypeCode =
                            input.PaymentTypeCode,

                        SupplierCode =
                            input.SupplierCode,

                        PaymentTotal =
                            input.Total,

                        TaxTotal =
                            input.Tax,

                        PaymentNetTotal =
                            input.Total - input.Tax,

                        IsEnabled =
                            true,

                        CreateDate =
                            now,

                        CreateBy =
                            username,

                        UpdateDate =
                            now,

                        UpdateBy =
                            username
                    };

                db.TblWithholdingTaxes.Add(
                    withholdingTax);

                if (taxPayment == null)
                {
                    db.TblWithholdingTaxPayments.Add(
                        new TblWithholdingTaxPayment
                        {
                            WithholdingTaxCode =
                                withholdingTax.WithholdingTaxCode,

                            MonthlyPaymentCode =
                                payment.ReceivePaymentCode,

                            CreateDate =
                                now,

                            CreateBy =
                                username
                        });
                }
                else
                {
                    taxPayment.WithholdingTaxCode =
                        withholdingTax.WithholdingTaxCode;
                }
            }
            else
            {
                withholdingTax.WithholdingTaxDate =
                    input.PaymentDate;

                withholdingTax.PaymentTypeCode =
                    input.PaymentTypeCode;

                withholdingTax.SupplierCode =
                    input.SupplierCode;

                withholdingTax.PaymentTotal =
                    input.Total;

                withholdingTax.TaxTotal =
                    input.Tax;

                withholdingTax.PaymentNetTotal =
                    input.Total - input.Tax;

                withholdingTax.IsEnabled =
                    true;

                withholdingTax.UpdateDate =
                    now;

                withholdingTax.UpdateBy =
                    username;
            }
        }
        else if (withholdingTax != null)
        {
            withholdingTax.IsEnabled =
                false;

            withholdingTax.UpdateDate =
                now;

            withholdingTax.UpdateBy =
                username;
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
                "Error while rolling back Payee transaction.");
        }
    }
}
