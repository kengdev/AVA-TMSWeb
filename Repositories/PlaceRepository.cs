using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class PlaceRepository
{
    private readonly DbavalogContext db;
    private readonly CodeRepository codeRepo;
    private readonly IAuditLogService auditLogService;
    private readonly ILogger<PlaceRepository> logger;

    public PlaceRepository(
        DbavalogContext context,
        IAuditLogService _auditLogService,
        CodeRepository _codeRepo,
        ILogger<PlaceRepository> _logger)
    {
        db = context;
        auditLogService = _auditLogService;
        codeRepo = _codeRepo;
        logger = _logger;
    }

    public async Task<PlaceModel?> GetPlaceByCode(
        string place_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(place_code))
        {
            return null;
        }

        return await (
            from a in db.TblPlaces.AsNoTracking()
            join b in db.TblIndustries.AsNoTracking()
                on a.PlaceCode equals b.IndustryCode
            join c in db.TblZones.AsNoTracking()
                on b.ZoneCode equals c.ZoneCode
            where a.PlaceCode == place_code
            select new PlaceModel
            {
                place_code = a.PlaceCode,
                place_port_code = b.PortCode,
                place_tax = b.IndustryTax,
                place_fullname = b.IndustryFullname,
                place_name = a.PlaceName,
                place_name_th = b.IndustryNameTh,
                place_address = b.IndustryAddress,
                zone_code = b.ZoneCode,
                zone_name = c.ZoneName,
                province_code = b.ProvinceCode,
                place_lat = b.IndustryLat,
                place_lon = b.IndustryLon,
                place_remark = a.PlaceRemark,
                expressway_value = a.ExpresswayValue,
                is_actived = a.IsActived,
                is_enabled = a.IsEnabled
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<PlaceViewModel>> GetPlaceDataList(
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblZones.AsNoTracking()
            join b in db.TblIndustries.AsNoTracking()
                on a.ZoneCode equals b.ZoneCode
            join c in db.TblPlaces.AsNoTracking()
                on b.IndustryCode equals c.PlaceCode
            where c.IsEnabled == true
            orderby a.ZoneName, c.PlaceName
            select new PlaceViewModel
            {
                place_code = c.PlaceCode,
                place_port_code = b.PortCode,
                place_name = c.PlaceName,
                zone_name = a.ZoneName,
                is_actived = c.IsActived
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TblZone?> GetZoneDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblZones
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.ZoneCode == code,
                cancellationToken);
    }

    public async Task<PlaceViewModel?> GetPlaceDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await (
            from a in db.TblPlaces.AsNoTracking()
            join b in db.TblIndustries.AsNoTracking()
                on a.PlaceCode equals b.IndustryCode
            where a.PlaceCode == code
            select new PlaceViewModel
            {
                place_code = a.PlaceCode,
                place_name = b.IndustryFullname,
                place_address = b.IndustryAddress,
                place_tax = b.IndustryTax
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ResponseViewModel> AddPlace(
        PlaceModel model,
        CancellationToken cancellationToken = default)
    {
        if (model == null)
        {
            return Failure("ไม่พบข้อมูลสถานที่");
        }

        if (string.IsNullOrWhiteSpace(model.place_name))
        {
            return Failure("กรุณาระบุชื่อสถานที่");
        }

        if (string.IsNullOrWhiteSpace(model.province_code))
        {
            return Failure("กรุณาระบุจังหวัด");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var now = DateTime.Now;

            var placeModel = new TblPlace
            {
                PlaceCode =
                    await codeRepo.GetPlaceCode(
                        model.province_code),

                PlaceName =
                    model.place_name
                        .Trim()
                        .ToUpperInvariant(),

                PlaceType = "INDT",
                ExpresswayValue = model.expressway_value,
                PlaceRemark = model.place_remark,
                IsActived = true,
                IsEnabled = true,
                CreateDate = now,
                CreateBy = model.create_by,
                UpdateDate = now,
                UpdateBy = model.create_by
            };

            var codeExists = await db.TblPlaces
                .AsNoTracking()
                .AnyAsync(
                    w => w.PlaceCode ==
                         placeModel.PlaceCode,
                    cancellationToken);

            if (codeExists)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    $"รหัสสถานที่ {placeModel.PlaceCode} มีอยู่ในระบบแล้ว");
            }

            var industryModel = new TblIndustry
            {
                IndustryCode = placeModel.PlaceCode,
                PortCode = model.place_port_code,
                IndustryNameTh = model.place_name_th,
                IndustryFullname = model.place_fullname,
                IndustryAddress = model.place_address,
                IndustryTax = model.place_tax,
                ZoneCode = model.zone_code,
                IndustryLat = model.place_lat,
                IndustryLon = model.place_lon,
                ProvinceCode = model.province_code
            };

            placeModel.TblIndustry = industryModel;

            db.TblPlaces.Add(placeModel);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Place",
                    ActionName = AuditActions.Create,
                    EntityName = nameof(TblPlace),
                    EntityId = placeModel.PlaceCode,
                    Description =
                        $"เพิ่มข้อมูล Place {placeModel.PlaceName} โดย {model.create_by}",
                    NewValues = new
                    {
                        Place = GetScalarSnapshot(placeModel),
                        Industry = GetScalarSnapshot(industryModel)
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

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
                "AddPlace",
                model.place_name);
        }
    }

    public async Task<ResponseViewModel> EditPlace(
        PlaceModel placeToEdit,
        CancellationToken cancellationToken = default)
    {
        if (placeToEdit == null ||
            string.IsNullOrWhiteSpace(placeToEdit.place_code))
        {
            return Failure("ไม่พบข้อมูลสถานที่");
        }

        if (string.IsNullOrWhiteSpace(placeToEdit.place_name))
        {
            return Failure("กรุณาระบุชื่อสถานที่");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var placeModel = await db.TblPlaces
                .FirstOrDefaultAsync(
                    w => w.PlaceCode ==
                         placeToEdit.place_code,
                    cancellationToken);

            if (placeModel == null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    $"ไม่พบสถานที่ {placeToEdit.place_code}");
            }

            if (placeModel.IsEnabled != true)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    "สถานที่รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var industryModel = await db.TblIndustries
                .FirstOrDefaultAsync(
                    w => w.IndustryCode ==
                         placeToEdit.place_code,
                    cancellationToken);

            if (industryModel == null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    $"ไม่พบข้อมูล Industry ของสถานที่ {placeToEdit.place_code}");
            }

            var oldValues = new
            {
                Place = GetScalarSnapshot(placeModel),
                Industry = GetScalarSnapshot(industryModel)
            };

            placeModel.PlaceName =
                placeToEdit.place_name
                    .Trim()
                    .ToUpperInvariant();

            placeModel.ExpresswayValue =
                placeToEdit.expressway_value;

            placeModel.PlaceRemark =
                placeToEdit.place_remark;

            /*
             * ไม่ตั้ง IsEnabled = true ใน Edit
             * เพื่อป้องกันการแก้ไข endpoint นี้แล้วชุบสถานที่ที่ถูก Remove กลับมา
             */
            placeModel.UpdateDate = DateTime.Now;
            placeModel.UpdateBy = placeToEdit.update_by;

            industryModel.PortCode =
                placeToEdit.place_port_code;

            industryModel.IndustryNameTh =
                placeToEdit.place_name_th;

            industryModel.IndustryFullname =
                placeToEdit.place_fullname;

            industryModel.IndustryAddress =
                placeToEdit.place_address;

            industryModel.IndustryTax =
                placeToEdit.place_tax;

            industryModel.ZoneCode =
                placeToEdit.zone_code;

            industryModel.IndustryLat =
                placeToEdit.place_lat;

            industryModel.IndustryLon =
                placeToEdit.place_lon;

            industryModel.ProvinceCode =
                placeToEdit.province_code;

            SetUpdateAuditFieldsIfPresent(
                industryModel,
                placeToEdit.update_by);

            var newValues = new
            {
                Place = GetScalarSnapshot(placeModel),
                Industry = GetScalarSnapshot(industryModel)
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Place",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblPlace),
                    EntityId = placeModel.PlaceCode,
                    Description =
                        $"แก้ไขข้อมูล Place {placeModel.PlaceName} โดย {placeModel.UpdateBy}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

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
                "EditPlace",
                placeToEdit.place_code);
        }
    }

    public async Task<ResponseViewModel> Remove(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัสสถานที่");
        }

        try
        {
            var item = await db.TblPlaces
                .FirstOrDefaultAsync(
                    w => w.PlaceCode == code,
                    cancellationToken);

            if (item == null)
            {
                return Failure(
                    $"ไม่พบสถานที่ {code}");
            }

            if (item.IsEnabled != true)
            {
                return Failure(
                    "สถานที่รายการนี้ถูกยกเลิกไปแล้ว");
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
                    ModuleName = "Place",
                    ActionName = AuditActions.Delete,
                    EntityName = nameof(TblPlace),
                    EntityId = code,
                    Description =
                        $"ลบข้อมูล Place {item.PlaceName} โดย {username}",
                    OldValues = oldValues,
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
            return Failure("ไม่พบรหัสสถานที่");
        }

        try
        {
            var item = await db.TblPlaces
                .FirstOrDefaultAsync(
                    w => w.PlaceCode == code,
                    cancellationToken);

            if (item == null)
            {
                return Failure(
                    $"ไม่พบสถานที่ {code}");
            }

            if (item.IsEnabled != true)
            {
                return Failure(
                    "ไม่สามารถเปิดใช้งานสถานที่ที่ถูกยกเลิกแล้ว");
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
                    ModuleName = "Place",
                    ActionName = AuditActions.Active,
                    EntityName = nameof(TblPlace),
                    EntityId = code,
                    Description =
                        $"เปิดใช้งาน Place {item.PlaceName} โดย {username}",
                    OldValues = oldValues,
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
                "Active",
                code);
        }
    }

    public async Task<ResponseViewModel> InActive(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัสสถานที่");
        }

        try
        {
            var item = await db.TblPlaces
                .FirstOrDefaultAsync(
                    w => w.PlaceCode == code,
                    cancellationToken);

            if (item == null)
            {
                return Failure(
                    $"ไม่พบสถานที่ {code}");
            }

            if (item.IsEnabled != true)
            {
                return Failure(
                    "สถานที่รายการนี้ถูกยกเลิกไปแล้ว");
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
                    ModuleName = "Place",
                    ActionName = AuditActions.Inactive,
                    EntityName = nameof(TblPlace),
                    EntityId = code,
                    Description =
                        $"ปิดใช้งาน Place {item.PlaceName} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            /*
             * Method เดิมเปิด transaction แต่ไม่ Commit
             * ทำให้ transaction ถูก Dispose แล้ว rollback ภายหลัง
             * ตัวใหม่นี้ไม่สร้าง explicit transaction เพราะมี SaveChanges เดียว
             * ซึ่ง EF Core ครอบ transaction ให้อัตโนมัติ
             */
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
                "InActive",
                code);
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

    private void SetUpdateAuditFieldsIfPresent(
        object entity,
        string? username)
    {
        var entry = db.Entry(entity);

        if (entry.Metadata.FindProperty("UpdateDate") != null)
        {
            entry.Property("UpdateDate").CurrentValue =
                DateTime.Now;
        }

        if (entry.Metadata.FindProperty("UpdateBy") != null)
        {
            entry.Property("UpdateBy").CurrentValue =
                username;
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
                "Error while rolling back Place transaction.");
        }
    }
}
