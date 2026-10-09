using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class SupplierRepository
{
    private const string HiddenSupplierCode = "SP18001";
    private const string InternalFleetType = "F1";
    private const string ExternalFleetType = "F2";
    private const string InternalFleetCode = "FL001";
    private const string InternalFleetDetailType = "A1";

    private readonly DbavalogContext db;
    private readonly IAuditLogService auditLogService;
    private readonly CodeRepository codeRepo;
    private readonly ILogger<SupplierRepository> logger;

    public SupplierRepository(
        DbavalogContext context,
        CodeRepository _codeRepo,
        IAuditLogService _auditLogService,
        ILogger<SupplierRepository> _logger)
    {
        db = context;
        codeRepo = _codeRepo;
        auditLogService = _auditLogService;
        logger = _logger;
    }

    public async Task<TblSupplier?> GetSupplierData(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblSuppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.SupplierCode == code,
                cancellationToken);
    }

    public async Task<List<SupplierViewModel>> GetSupplierDataList(
        CancellationToken cancellationToken = default)
    {
        return await db.TblSuppliers
            .AsNoTracking()
            .Where(w =>
                w.IsEnabled == true &&
                w.SupplierCode != HiddenSupplierCode)
            .OrderBy(o => o.SupplierName)
            .Select(a => new SupplierViewModel
            {
                supplier_code = a.SupplierCode,
                supplier_name = a.SupplierName,
                supplier_phone = a.SupplierPhone,
                supplier_address = a.SupplierAddress,
                is_actived = a.IsActived
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TblFleetExternal?> GetExternalFleetByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblFleetExternals
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.SupplierCode == code,
                cancellationToken);
    }

    public async Task<TblFleetInternal?> GetInternalFleetByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblFleetInternals
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.SupplierCode == code,
                cancellationToken);
    }

    public async Task<ResponseViewModel> Add(
        TblSupplier model,
        CancellationToken cancellationToken = default)
    {
        if (model == null)
        {
            return Failure("ไม่พบข้อมูล Supplier");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            model.SupplierCode =
                await codeRepo.GetSupplierCode(
                    cancellationToken);

            if (string.IsNullOrWhiteSpace(model.SupplierCode))
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    "ไม่สามารถสร้างรหัส Supplier ได้");
            }

            var codeExists = await db.TblSuppliers
                .AsNoTracking()
                .AnyAsync(
                    w => w.SupplierCode == model.SupplierCode,
                    cancellationToken);

            if (codeExists)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    $"Supplier Code {model.SupplierCode} มีอยู่ในระบบแล้ว");
            }

            /*
             * ใช้ Entry.State = Added เฉพาะ root Supplier
             * แทน Add(model) ที่อาจ traverse navigation graph
             * ที่ถูกส่งมาจาก client โดยไม่ตั้งใจ
             */
            db.Entry(model).State =
                EntityState.Added;

            TblFleetInternal? internalFleet = null;
            TblFleetExternal? externalFleet = null;
            var newRates = new List<TblSupplierRate>();

            if (model.FleetTypeCode == InternalFleetType)
            {
                internalFleet = new TblFleetInternal
                {
                    SupplierCode = model.SupplierCode,
                    FleetCode = InternalFleetCode,
                    FleetTypeCode = InternalFleetDetailType
                };

                db.TblFleetInternals.Add(internalFleet);
            }
            else if (model.FleetTypeCode == ExternalFleetType)
            {
                externalFleet = new TblFleetExternal
                {
                    SupplierCode = model.SupplierCode,
                    FleetTypeCode = ExternalFleetType
                };

                db.TblFleetExternals.Add(externalFleet);

                var placeCodes =
                    await GetActiveSupplierRatePlaceCodesAsync(
                        cancellationToken);

                newRates = placeCodes
                    .Select(placeCode =>
                        new TblSupplierRate
                        {
                            SupplierCode = model.SupplierCode,
                            PlaceCode = placeCode,
                            CostRate = 0
                        })
                    .ToList();

                if (newRates.Count > 0)
                {
                    db.TblSupplierRates.AddRange(
                        newRates);
                }
            }

            var newValues = new
            {
                Supplier = GetScalarSnapshot(model),

                FleetInternal =
                    internalFleet == null
                        ? null
                        : GetScalarSnapshot(internalFleet),

                FleetExternal =
                    externalFleet == null
                        ? null
                        : GetScalarSnapshot(externalFleet),

                SupplierRates =
                    newRates
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Supplier",
                    ActionName = AuditActions.Create,
                    EntityName = nameof(TblSupplier),
                    EntityId = model.SupplierCode,
                    Description =
                        $"เพิ่มข้อมูล Supplier {model.SupplierName}",
                    NewValues = newValues,
                    Result = AuditResults.Success
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
                "Add",
                model.SupplierCode);
        }
    }

    public async Task<ResponseViewModel> Edit(
        TblSupplier model,
        CancellationToken cancellationToken = default)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.SupplierCode))
        {
            return Failure("ไม่พบข้อมูล Supplier");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var supplier = await db.TblSuppliers
                .FirstOrDefaultAsync(
                    w => w.SupplierCode == model.SupplierCode,
                    cancellationToken);

            if (supplier == null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    $"ไม่พบ Supplier {model.SupplierCode}");
            }

            if (supplier.IsEnabled != true)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    "Supplier รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldInternalFleets =
                await db.TblFleetInternals
                    .AsNoTracking()
                    .Where(w =>
                        w.SupplierCode ==
                        supplier.SupplierCode)
                    .ToListAsync(cancellationToken);

            var oldExternalFleets =
                await db.TblFleetExternals
                    .AsNoTracking()
                    .Where(w =>
                        w.SupplierCode ==
                        supplier.SupplierCode)
                    .ToListAsync(cancellationToken);

            var oldRates =
                await db.TblSupplierRates
                    .AsNoTracking()
                    .Where(w =>
                        w.SupplierCode ==
                        supplier.SupplierCode)
                    .ToListAsync(cancellationToken);

            var oldValues = new
            {
                Supplier =
                    GetScalarSnapshot(supplier),

                FleetInternals =
                    oldInternalFleets
                        .Select(GetScalarSnapshot)
                        .ToList(),

                FleetExternals =
                    oldExternalFleets
                        .Select(GetScalarSnapshot)
                        .ToList(),

                SupplierRates =
                    oldRates
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            CopyEditableScalarValues(
                supplier,
                model,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    nameof(TblSupplier.SupplierCode),
                    nameof(TblSupplier.IsEnabled),
                    nameof(TblSupplier.IsActived),
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            supplier.UpdateDate = DateTime.Now;
            supplier.UpdateBy = model.UpdateBy;

            /*
             * Fleet relation เดิมควรเหลือได้เพียงชนิดเดียว
             * ลบ relation เก่าแล้วสร้างใหม่ตาม FleetTypeCode ปัจจุบัน
             */
            await db.TblFleetInternals
                .Where(w =>
                    w.SupplierCode ==
                    supplier.SupplierCode)
                .ExecuteDeleteAsync(cancellationToken);

            await db.TblFleetExternals
                .Where(w =>
                    w.SupplierCode ==
                    supplier.SupplierCode)
                .ExecuteDeleteAsync(cancellationToken);

            TblFleetInternal? newInternalFleet = null;
            TblFleetExternal? newExternalFleet = null;

            if (supplier.FleetTypeCode == InternalFleetType)
            {
                newInternalFleet =
                    new TblFleetInternal
                    {
                        SupplierCode = supplier.SupplierCode,
                        FleetCode = InternalFleetCode,
                        FleetTypeCode = InternalFleetDetailType
                    };

                db.TblFleetInternals.Add(
                    newInternalFleet);

                await db.TblSupplierRates
                    .Where(w =>
                        w.SupplierCode ==
                        supplier.SupplierCode)
                    .ExecuteDeleteAsync(
                        cancellationToken);
            }
            else if (supplier.FleetTypeCode == ExternalFleetType)
            {
                newExternalFleet =
                    new TblFleetExternal
                    {
                        SupplierCode = supplier.SupplierCode,
                        FleetTypeCode = ExternalFleetType
                    };

                db.TblFleetExternals.Add(
                    newExternalFleet);

                /*
                 * รักษาเรทเดิมไว้ และเติมเฉพาะ Place ที่ขาด
                 * เพื่อไม่ทำให้เรทที่ผู้ใช้ตั้งไว้ถูก reset เป็น 0
                 */
                var activePlaceCodes =
                    await GetActiveSupplierRatePlaceCodesAsync(
                        cancellationToken);

                var existingRatePlaceCodes =
                    oldRates
                        .Select(x => x.PlaceCode)
                        .ToHashSet();

                var missingPlaceCodes =
                    activePlaceCodes
                        .Where(code =>
                            !existingRatePlaceCodes.Contains(code))
                        .ToList();

                if (missingPlaceCodes.Count > 0)
                {
                    var ratesToAdd =
                        missingPlaceCodes
                            .Select(placeCode =>
                                new TblSupplierRate
                                {
                                    SupplierCode =
                                        supplier.SupplierCode,

                                    PlaceCode =
                                        placeCode,

                                    CostRate = 0
                                })
                            .ToList();

                    db.TblSupplierRates.AddRange(
                        ratesToAdd);
                }
            }
            else
            {
                /*
                 * คงพฤติกรรมเดิม:
                 * Supplier ที่ไม่ใช่ F2 ไม่ใช้ TblSupplierRate
                 */
                await db.TblSupplierRates
                    .Where(w =>
                        w.SupplierCode ==
                        supplier.SupplierCode)
                    .ExecuteDeleteAsync(
                        cancellationToken);
            }

            /*
             * Snapshot หลังแก้ใช้ค่าที่ตั้งใจจะบันทึก
             * โดยไม่ serialize navigation graph ของ Supplier
             */
            var activeRateSnapshots =
                supplier.FleetTypeCode == ExternalFleetType
                    ? await BuildSupplierRateSnapshotsAsync(
                        supplier.SupplierCode,
                        cancellationToken)
                    : new List<Dictionary<string, object?>>();

            /*
             * BuildSupplierRateSnapshotsAsync อ่านเฉพาะข้อมูลที่อยู่ใน DB
             * จึงยังไม่เห็น rows ใหม่ที่ Added แต่ยังไม่ Save
             * เติม rows Added จาก ChangeTracker เข้า snapshot ด้วย
             */
            var addedRateSnapshots =
                db.ChangeTracker
                    .Entries<TblSupplierRate>()
                    .Where(e =>
                        e.State == EntityState.Added &&
                        e.Entity.SupplierCode ==
                        supplier.SupplierCode)
                    .Select(e =>
                        GetScalarSnapshot(e.Entity))
                    .ToList();

            if (addedRateSnapshots.Count > 0)
            {
                activeRateSnapshots.AddRange(
                    addedRateSnapshots);
            }

            var newValues = new
            {
                Supplier =
                    GetScalarSnapshot(supplier),

                FleetInternal =
                    newInternalFleet == null
                        ? null
                        : GetScalarSnapshot(
                            newInternalFleet),

                FleetExternal =
                    newExternalFleet == null
                        ? null
                        : GetScalarSnapshot(
                            newExternalFleet),

                SupplierRates =
                    activeRateSnapshots
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Supplier",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblSupplier),
                    EntityId = supplier.SupplierCode,
                    Description =
                        $"แก้ไขข้อมูล Supplier {supplier.SupplierName} โดย {supplier.UpdateBy}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
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
                "Edit",
                model.SupplierCode);
        }
    }

    public async Task<ResponseViewModel> Remove(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัส Supplier");
        }

        try
        {
            var item = await db.TblSuppliers
                .FirstOrDefaultAsync(
                    w => w.SupplierCode == code,
                    cancellationToken);

            if (item == null)
            {
                return Failure(
                    $"ไม่พบ Supplier {code}");
            }

            if (item.IsEnabled != true)
            {
                return Failure(
                    "Supplier รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues =
                GetScalarSnapshot(item);

            item.IsActived = false;
            item.IsEnabled = false;
            item.UpdateDate = DateTime.Now;
            item.UpdateBy = username;

            var newValues =
                GetScalarSnapshot(item);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Supplier",
                    ActionName = AuditActions.Delete,
                    EntityName = nameof(TblSupplier),
                    EntityId = code,
                    Description =
                        $"ลบข้อมูล Supplier {item.SupplierName} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
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
            return Failure("ไม่พบรหัส Supplier");
        }

        try
        {
            var item = await db.TblSuppliers
                .FirstOrDefaultAsync(
                    w => w.SupplierCode == code,
                    cancellationToken);

            if (item == null)
            {
                return Failure(
                    $"ไม่พบ Supplier {code}");
            }

            if (item.IsEnabled != true)
            {
                return Failure(
                    "ไม่สามารถเปิดใช้งาน Supplier ที่ถูกยกเลิกแล้ว");
            }

            if (item.IsActived == true)
            {
                return Success();
            }

            var oldValues =
                GetScalarSnapshot(item);

            item.IsActived = true;
            item.UpdateDate = DateTime.Now;
            item.UpdateBy = username;

            var newValues =
                GetScalarSnapshot(item);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Supplier",
                    ActionName = AuditActions.Active,
                    EntityName = nameof(TblSupplier),
                    EntityId = code,
                    Description =
                        $"เปิดใช้งาน Supplier {item.SupplierName} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
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
            return Failure("ไม่พบรหัส Supplier");
        }

        try
        {
            var item = await db.TblSuppliers
                .FirstOrDefaultAsync(
                    w => w.SupplierCode == code,
                    cancellationToken);

            if (item == null)
            {
                return Failure(
                    $"ไม่พบ Supplier {code}");
            }

            if (item.IsEnabled != true)
            {
                return Failure(
                    "Supplier รายการนี้ถูกยกเลิกไปแล้ว");
            }

            if (item.IsActived == false)
            {
                return Success();
            }

            var oldValues =
                GetScalarSnapshot(item);

            item.IsActived = false;
            item.UpdateDate = DateTime.Now;
            item.UpdateBy = username;

            var newValues =
                GetScalarSnapshot(item);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Supplier",
                    ActionName = AuditActions.Inactive,
                    EntityName = nameof(TblSupplier),
                    EntityId = code,
                    Description =
                        $"ปิดใช้งาน Supplier {item.SupplierName} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
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
                "Inactive",
                code);
        }
    }

    private async Task<List<string>>
        GetActiveSupplierRatePlaceCodesAsync(
            CancellationToken cancellationToken)
    {
        return await db.TblCustomerCosts
            .AsNoTracking()
            .Where(w =>
                w.IsActived == true)
            .Select(w =>
                w.MissionCode)
            .Distinct()
            .ToListAsync(
                cancellationToken);
    }

    private async Task<List<Dictionary<string, object?>>>
        BuildSupplierRateSnapshotsAsync(
            string supplierCode,
            CancellationToken cancellationToken)
    {
        var rates = await db.TblSupplierRates
            .AsNoTracking()
            .Where(w =>
                w.SupplierCode == supplierCode)
            .ToListAsync(cancellationToken);

        return rates
            .Select(GetScalarSnapshot)
            .ToList();
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

        var sourceProperties =
            sourceEntry.Properties
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
                "Error while rolling back Supplier transaction.");
        }
    }
}
