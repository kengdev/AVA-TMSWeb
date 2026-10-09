using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class QuotationRepository
{
    private readonly DbavalogContext db;
    private readonly ILogger<QuotationRepository> logger;

    public QuotationRepository(
        DbavalogContext context,
        ILogger<QuotationRepository> _logger)
    {
        db = context;
        logger = _logger;
    }

    public async Task<ResponseViewModel> Add(
        TblQuotation model,
        CancellationToken cancellationToken = default)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.QuotationCode))
        {
            return Failure("ไม่พบข้อมูลใบเสนอราคา");
        }

        try
        {
            var exists = await db.TblQuotations
                .AsNoTracking()
                .AnyAsync(
                    w => w.QuotationCode == model.QuotationCode,
                    cancellationToken);

            if (exists)
            {
                return Failure(
                    $"เลขที่ใบเสนอราคา {model.QuotationCode} มีอยู่ในระบบแล้ว");
            }

            NormalizeChildKeys(model);

            db.TblQuotations.Add(model);

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
                "Add",
                model.QuotationCode);
        }
    }

    public async Task<ResponseViewModel> Edit(
        TblQuotation model,
        CancellationToken cancellationToken = default)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.QuotationCode))
        {
            return Failure("ไม่พบข้อมูลใบเสนอราคา");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var quotation = await db.TblQuotations
                .FirstOrDefaultAsync(
                    w => w.QuotationCode == model.QuotationCode,
                    cancellationToken);

            if (quotation == null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    $"ไม่พบใบเสนอราคา {model.QuotationCode}");
            }

            if (quotation.IsEnabled != true)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    "ใบเสนอราคารายการนี้ถูกยกเลิกไปแล้ว");
            }

            var newDetails =
                model.TblQuotationDetails?
                    .ToList()
                ?? new List<TblQuotationDetail>();

            var newConditions =
                model.TblQuotationConditions?
                    .ToList()
                ?? new List<TblQuotationCondition>();

            /*
             * ใช้ tracked master แทน db.Update(detached model)
             * และกัน field ระบบไม่ให้ form ทับโดยไม่ตั้งใจ
             */
            CopyEditableScalarValues(
                quotation,
                model,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    nameof(TblQuotation.QuotationCode),
                    nameof(TblQuotation.IsEnabled),
                    nameof(TblQuotation.IsActived),
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            quotation.UpdateDate = DateTime.Now;
            quotation.UpdateBy = model.UpdateBy;

            /*
             * ลบ children เดิมทั้งหมด แล้วเพิ่มชุดใหม่อย่างชัดเจน
             * เพื่อไม่พึ่ง graph state จาก detached model
             */
            await db.TblQuotationDetails
                .Where(w =>
                    w.QuotationCode == quotation.QuotationCode)
                .ExecuteDeleteAsync(cancellationToken);

            await db.TblQuotationConditions
                .Where(w =>
                    w.QuotationCode == quotation.QuotationCode)
                .ExecuteDeleteAsync(cancellationToken);

            foreach (var detail in newDetails)
            {
                detail.QuotationCode =
                    quotation.QuotationCode;

                ResetGeneratedPrimaryKeys(detail);
            }

            foreach (var condition in newConditions)
            {
                condition.QuotationCode =
                    quotation.QuotationCode;

                ResetGeneratedPrimaryKeys(condition);
            }

            if (newDetails.Count > 0)
            {
                db.TblQuotationDetails.AddRange(
                    newDetails);
            }

            if (newConditions.Count > 0)
            {
                db.TblQuotationConditions.AddRange(
                    newConditions);
            }

            await db.SaveChangesAsync(cancellationToken);

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
                "Edit",
                model.QuotationCode);
        }
    }

    public async Task<ResponseViewModel> Remove(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบเลขที่ใบเสนอราคา");
        }

        try
        {
            var quotation = await db.TblQuotations
                .FirstOrDefaultAsync(
                    w => w.QuotationCode == code,
                    cancellationToken);

            if (quotation == null)
            {
                return Failure(
                    "ไม่พบข้อมูลใบเสนอราคา");
            }

            if (quotation.IsEnabled != true)
            {
                return Failure(
                    "ใบเสนอราคารายการนี้ถูกยกเลิกไปแล้ว");
            }

            quotation.IsActived = false;
            quotation.IsEnabled = false;
            quotation.UpdateDate = DateTime.Now;
            quotation.UpdateBy = username;

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
                "Remove",
                code);
        }
    }

    public async Task<ResponseViewModel> Active(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบเลขที่ใบเสนอราคา");
        }

        try
        {
            var quotation = await db.TblQuotations
                .FirstOrDefaultAsync(
                    w => w.QuotationCode == code,
                    cancellationToken);

            if (quotation == null)
            {
                return Failure(
                    "ไม่พบข้อมูลใบเสนอราคา");
            }

            if (quotation.IsEnabled != true)
            {
                return Failure(
                    "ไม่สามารถเปิดใช้งานใบเสนอราคาที่ถูกยกเลิกแล้ว");
            }

            if (quotation.IsActived == true)
            {
                return Success();
            }

            quotation.IsActived = true;
            quotation.UpdateDate = DateTime.Now;
            quotation.UpdateBy = username;

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
                "Active",
                code);
        }
    }

    public async Task<ResponseViewModel> Inactive(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบเลขที่ใบเสนอราคา");
        }

        try
        {
            var quotation = await db.TblQuotations
                .FirstOrDefaultAsync(
                    w => w.QuotationCode == code,
                    cancellationToken);

            if (quotation == null)
            {
                return Failure(
                    "ไม่พบข้อมูลใบเสนอราคา");
            }

            if (quotation.IsEnabled != true)
            {
                return Failure(
                    "ใบเสนอราคารายการนี้ถูกยกเลิกไปแล้ว");
            }

            if (quotation.IsActived == false)
            {
                return Success();
            }

            quotation.IsActived = false;
            quotation.UpdateDate = DateTime.Now;
            quotation.UpdateBy = username;

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
                "Inactive",
                code);
        }
    }

    public async Task<TblQuotation?> GetQuotationDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        /*
         * มี child collection 2 ชุด ใช้ AsSplitQuery()
         * เพื่อลด cartesian explosion และ AsNoTracking()
         * เพราะ method นี้ใช้สำหรับอ่านข้อมูล
         */
        return await db.TblQuotations
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.TblQuotationDetails
                .OrderBy(o => o.FuelRate))
            .Include(x => x.TblQuotationConditions
                .OrderBy(o => o.Id))
            .FirstOrDefaultAsync(
                w => w.QuotationCode == code,
                cancellationToken);
    }

    public async Task<List<QuotationViewModel>> GetQuotationList(
        CancellationToken cancellationToken = default)
    {
        var quotations = await db.TblQuotations
            .AsNoTracking()
            .Where(a => a.IsEnabled == true)
            .OrderByDescending(a => a.CreateDate)
            .Select(a => new
            {
                a.QuotationCode,
                a.CustomerName,
                a.Route,
                a.StartDate,
                a.ExpireDate,
                a.IsActived,
                a.ContainerSize1,
                a.ContainerSize2,
                a.ContainerSize3,
                a.CreateDate
            })
            .ToListAsync(cancellationToken);

        if (quotations.Count == 0)
        {
            return new List<QuotationViewModel>();
        }

        var quotationCodes = quotations
            .Select(x => x.QuotationCode)
            .Distinct()
            .ToList();

        /*
         * โหลด active detail ครั้งเดียวแล้ว lookup ใน memory
         * แทน correlated query ต่อ quotation
         */
        var activeDetails = await db.TblQuotationDetails
            .AsNoTracking()
            .Where(w =>
                quotationCodes.Contains(w.QuotationCode) &&
                w.IsActive == true)
            .ToListAsync(cancellationToken);

        var activeDetailLookup = activeDetails
            .GroupBy(x => x.QuotationCode)
            .ToDictionary(
                g => g.Key,
                g => g.First());

        return quotations
            .Select(a =>
                new QuotationViewModel
                {
                    quotation_code = a.QuotationCode,
                    customer = a.CustomerName,
                    route = a.Route,

                    start_date =
                        a.StartDate.ToString("dd-MM-yyyy"),

                    expire_date =
                        a.ExpireDate.ToString("dd-MM-yyyy"),

                    is_actived = a.IsActived,
                    container1 = a.ContainerSize1,
                    container2 = a.ContainerSize2,
                    container3 = a.ContainerSize3,

                    create_date =
                        a.CreateDate.ToString(
                            "dd-MM-yyyy HH:mm"),

                    active_detail =
                        activeDetailLookup.TryGetValue(
                            a.QuotationCode,
                            out var detail)
                            ? detail
                            : null
                })
            .ToList();
    }

    private void NormalizeChildKeys(
        TblQuotation quotation)
    {
        if (quotation.TblQuotationDetails != null)
        {
            foreach (var detail in quotation.TblQuotationDetails)
            {
                detail.QuotationCode =
                    quotation.QuotationCode;
            }
        }

        if (quotation.TblQuotationConditions != null)
        {
            foreach (var condition in quotation.TblQuotationConditions)
            {
                condition.QuotationCode =
                    quotation.QuotationCode;
            }
        }
    }

    private void ResetGeneratedPrimaryKeys<TEntity>(
        TEntity entity)
        where TEntity : class
    {
        var entry = db.Entry(entity);
        var primaryKey = entry.Metadata.FindPrimaryKey();

        if (primaryKey == null)
        {
            return;
        }

        foreach (var keyProperty in primaryKey.Properties)
        {
            if (keyProperty.ValueGenerated != ValueGenerated.OnAdd)
            {
                continue;
            }

            var property =
                entry.Property(keyProperty.Name);

            property.CurrentValue =
                GetDefaultValue(keyProperty.ClrType);
        }
    }

    private static object? GetDefaultValue(
        Type type)
    {
        var underlyingType =
            Nullable.GetUnderlyingType(type);

        if (underlyingType != null)
        {
            return null;
        }

        return type.IsValueType
            ? Activator.CreateInstance(type)
            : null;
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
                "Error while rolling back Quotation transaction.");
        }
    }
}
