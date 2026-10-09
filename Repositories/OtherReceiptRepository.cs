using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class OtherReceiptRepository
{
    private readonly DbavalogContext db;
    private readonly IAuditLogService auditLogService;
    private readonly ILogger<OtherReceiptRepository> logger;

    public OtherReceiptRepository(
        DbavalogContext context,
        IAuditLogService _auditLogService,
        ILogger<OtherReceiptRepository> _logger)
    {
        db = context;
        auditLogService = _auditLogService;
        logger = _logger;
    }

    public async Task<List<ContainerHandlingViewModel>>
        GetContainerHandlingDataList(
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
            return new List<ContainerHandlingViewModel>();
        }

        /*
         * คง Month/Year ไว้เพื่อ compatibility กับ Entity ปัจจุบัน
         * เพราะจาก Repository อย่างเดียวไม่สามารถยืนยันได้ว่า
         * ReceiptDate เป็น DateOnly หรือ DateTime
         */
        var rawData = await (
            from a in db.TblReceiptOthers.AsNoTracking()
            join b in db.TblReceiptOtherDetails.AsNoTracking()
                on a.ReceiptOtherCode equals b.ReceiptOtherCode
            where a.ReceiptOtherType == "01"
                  && a.IsEnabled == true
                  && a.ReceiptDate.Month == m
                  && a.ReceiptDate.Year == y
            select new
            {
                a.ReceiptOtherCode,
                b.ReceiptOtherName,
                a.CustomerName,
                a.CustomerTax,
                a.CustomerAddress,
                a.NetTotal,
                a.Vat,
                a.Total,
                a.ReceiptDate
            })
            .ToListAsync(cancellationToken);

        return rawData
            .Select(a => new ContainerHandlingViewModel
            {
                code = a.ReceiptOtherCode,
                container_no = a.ReceiptOtherName,
                customer_name = a.CustomerName,
                customer_tax = a.CustomerTax,
                customer_address = a.CustomerAddress,
                net_total = a.NetTotal,
                vat = a.Vat,
                total = a.Total,
                receipt_date =
                    a.ReceiptDate.ToString("yyyy-MM-dd")
            })
            .ToList();
    }

    public async Task<TblReceiptOther?> GetOtherReceiptDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblReceiptOthers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.ReceiptOtherCode == code,
                cancellationToken);
    }

    public async Task<List<TblReceiptOtherDetail>>
        GetOtherReceiptDetailDataByCode(
            string code,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new List<TblReceiptOtherDetail>();
        }

        return await db.TblReceiptOtherDetails
            .AsNoTracking()
            .Where(w => w.ReceiptOtherCode == code)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResponseViewModel> AddReceiptHandling(
        TblReceiptOther model,
        CancellationToken cancellationToken = default)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.ReceiptOtherCode))
        {
            return Failure(
                "ไม่พบข้อมูล Container Handling");
        }

        try
        {
            var exists = await db.TblReceiptOthers
                .AsNoTracking()
                .AnyAsync(
                    w => w.ReceiptOtherCode ==
                         model.ReceiptOtherCode,
                    cancellationToken);

            if (exists)
            {
                return Failure(
                    $"รหัส {model.ReceiptOtherCode} มีอยู่ในระบบแล้ว");
            }

            NormalizeDetails(model);

            db.TblReceiptOthers.Add(model);

            var newValues = CreateReceiptSnapshot(
                model,
                model.TblReceiptOtherDetails?
                    .ToList() ??
                new List<TblReceiptOtherDetail>());

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Container handling",
                    ActionName = AuditActions.Create,
                    EntityName = nameof(TblReceiptOther),
                    EntityId = model.ReceiptOtherCode,
                    Description =
                        $"เพิ่มข้อมูล Container handling {model.ReceiptOtherCode} โดย {model.CreateBy}",
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "AddReceiptHandling",
                model.ReceiptOtherCode);
        }
    }

    public async Task<ResponseViewModel> EditReceiptHandling(
        TblReceiptOther model,
        CancellationToken cancellationToken = default)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.ReceiptOtherCode))
        {
            return Failure(
                "ไม่พบข้อมูล Container Handling");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var receipt = await db.TblReceiptOthers
                .FirstOrDefaultAsync(
                    w => w.ReceiptOtherCode ==
                         model.ReceiptOtherCode,
                    cancellationToken);

            if (receipt == null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    $"ไม่พบข้อมูล Container handling {model.ReceiptOtherCode}");
            }

            if (receipt.IsEnabled != true)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    "รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldDetails = await db.TblReceiptOtherDetails
                .AsNoTracking()
                .Where(w =>
                    w.ReceiptOtherCode ==
                    model.ReceiptOtherCode)
                .ToListAsync(cancellationToken);

            var oldValues =
                CreateReceiptSnapshot(
                    receipt,
                    oldDetails);

            /*
             * ใช้ tracked master แทน db.Update(detached model)
             * เพื่อไม่ให้ EF ชน entity instance ที่ key เดียวกัน
             * และกัน field ระบบไม่ให้ form ทับ
             */
            CopyEditableScalarValues(
                receipt,
                model,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    nameof(TblReceiptOther.ReceiptOtherCode),
                    nameof(TblReceiptOther.IsEnabled),
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            receipt.UpdateDate = DateTime.Now;
            receipt.UpdateBy = model.UpdateBy;

            await db.TblReceiptOtherDetails
                .Where(w =>
                    w.ReceiptOtherCode ==
                    model.ReceiptOtherCode)
                .ExecuteDeleteAsync(cancellationToken);

            var newDetails =
                model.TblReceiptOtherDetails?
                    .ToList() ??
                new List<TblReceiptOtherDetail>();

            foreach (var detail in newDetails)
            {
                detail.ReceiptOtherCode =
                    receipt.ReceiptOtherCode;
            }

            if (newDetails.Count > 0)
            {
                db.TblReceiptOtherDetails.AddRange(
                    newDetails);
            }

            var newValues =
                CreateReceiptSnapshot(
                    receipt,
                    newDetails);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Update container handling.",

                    ActionName =
                        AuditActions.Update,

                    EntityName =
                        nameof(TblReceiptOther),

                    EntityId =
                        receipt.ReceiptOtherCode,

                    Description =
                        $"แก้ไขข้อมูล Container handling {receipt.ReceiptOtherCode} โดย {receipt.UpdateBy}",

                    OldValues =
                        oldValues,

                    NewValues =
                        newValues,

                    Result =
                        AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Success();
        }
        catch (OperationCanceledException)
        {
            await RollbackSafelyAsync(
                transaction,
                CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "EditReceiptHandling",
                model.ReceiptOtherCode);
        }
    }

    public async Task<ResponseViewModel> RemoveReceiptHandling(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure(
                "ไม่พบรหัส Container Handling");
        }

        try
        {
            var receipt = await db.TblReceiptOthers
                .FirstOrDefaultAsync(
                    w => w.ReceiptOtherCode == code,
                    cancellationToken);

            if (receipt == null)
            {
                return Failure(
                    "ไม่พบข้อมูลที่ต้องการลบ");
            }

            if (receipt.IsEnabled != true)
            {
                return Failure(
                    "รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues =
                GetScalarSnapshot(receipt);

            receipt.IsEnabled = false;
            receipt.UpdateDate = DateTime.Now;
            receipt.UpdateBy = username;

            var newValues =
                GetScalarSnapshot(receipt);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Remove receipt handling.",

                    ActionName =
                        AuditActions.Delete,

                    EntityName =
                        nameof(TblReceiptOther),

                    EntityId =
                        code,

                    Description =
                        $"ลบข้อมูล Container handling {code} โดย {username}",

                    OldValues =
                        oldValues,

                    NewValues =
                        newValues,

                    Result =
                        AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            return Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "RemoveReceiptHandling",
                code);
        }
    }

    private void NormalizeDetails(
        TblReceiptOther model)
    {
        if (model.TblReceiptOtherDetails == null)
        {
            return;
        }

        foreach (var detail in model.TblReceiptOtherDetails)
        {
            detail.ReceiptOtherCode =
                model.ReceiptOtherCode;
        }
    }

    private object CreateReceiptSnapshot(
        TblReceiptOther receipt,
        IEnumerable<TblReceiptOtherDetail> details)
    {
        return new
        {
            Receipt =
                GetScalarSnapshot(receipt),

            Details =
                details
                    .Select(GetScalarSnapshot)
                    .ToList()
        };
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
                "Error while rolling back OtherReceipt transaction.");
        }
    }
}
