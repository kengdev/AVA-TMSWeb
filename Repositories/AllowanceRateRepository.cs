using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class AllowanceRateRepository
{
    private readonly DbavalogContext db;
    private readonly IAuditLogService auditLogService;
    private readonly ILogger<AllowanceRateRepository> logger;

    public AllowanceRateRepository(
        DbavalogContext context,
        IAuditLogService _auditLogService,
        ILogger<AllowanceRateRepository> _logger)
    {
        db = context;
        auditLogService = _auditLogService;
        logger = _logger;
    }

    public async Task<List<DriverRateViewModel>> GetDriverRate(
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblDriverRates.AsNoTracking()
            join b in db.TblPlaces.AsNoTracking()
                on a.PlaceCode equals b.PlaceCode
            join c in db.TblIndustries.AsNoTracking()
                on a.PlaceCode equals c.IndustryCode
            join d in db.TblZones.AsNoTracking()
                on c.ZoneCode equals d.ZoneCode
            orderby d.ZoneName, b.PlaceName
            select new DriverRateViewModel
            {
                name = b.PlaceName,
                zone_name = d.ZoneName,
                rate_value = a.RateValue,
                rate_code = a.PlaceCode
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SupplyerRateViewModel>> GetSupplyerRate(
        string supplier_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(supplier_code))
        {
            return new List<SupplyerRateViewModel>();
        }

        return await (
            from a in db.TblSuppliers.AsNoTracking()
            join b in db.TblSupplierRates.AsNoTracking()
                on a.SupplierCode equals b.SupplierCode
            join c in db.TblPlaces.AsNoTracking()
                on b.PlaceCode equals c.PlaceCode
            join e in db.TblIndustries.AsNoTracking()
                on c.PlaceCode equals e.IndustryCode
            join f in db.TblZones.AsNoTracking()
                on e.ZoneCode equals f.ZoneCode
            where a.SupplierCode == supplier_code
            orderby f.ZoneName, c.PlaceName
            select new SupplyerRateViewModel
            {
                place_name = c.PlaceName,
                supplyer_name = a.SupplierName,
                zone_name = f.ZoneName,
                supplyer_code = a.SupplierCode,
                place_code = b.PlaceCode,
                rate_sup_value = b.CostRate
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SupplyerRateViewModel>> GetFleetRate(
        string supplier_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(supplier_code))
        {
            return new List<SupplyerRateViewModel>();
        }

        return await (
            from a in db.TblFleets.AsNoTracking()
            join b in db.TblFleetRates.AsNoTracking()
                on a.FleetCode equals b.FleetCode
            join c in db.TblPlaces.AsNoTracking()
                on b.PlaceCode equals c.PlaceCode
            join e in db.TblIndustries.AsNoTracking()
                on c.PlaceCode equals e.IndustryCode
            join f in db.TblZones.AsNoTracking()
                on e.ZoneCode equals f.ZoneCode
            where a.FleetCode == supplier_code
                  && c.IsActived == true
                  && f.IsActived == true
            orderby f.ZoneName, c.PlaceName
            select new SupplyerRateViewModel
            {
                place_name = c.PlaceName,
                supplyer_name = a.FleetName,
                zone_name = f.ZoneName,
                supplyer_code = a.FleetCode,
                place_code = b.PlaceCode,
                rate_4W_value = b.Truck4w,
                rate_6W_value = b.Truck6w,
                rate_10W_value = b.Truck10w
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ResponseViewModel> EditDriverRate(
        decimal value,
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
            var rate = await db.TblDriverRates
                .FindAsync(
                    new object?[] { code },
                    cancellationToken);

            if (rate == null)
            {
                return Failure(
                    $"ไม่พบเรทพนักงานขับรถสำหรับสถานที่ {code}");
            }

            var oldValues = GetScalarSnapshot(rate);
            var oldRateValue = rate.RateValue;

            rate.RateValue = value;

            SetAuditFieldsIfPresent(
                rate,
                username);

            var newValues = GetScalarSnapshot(rate);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "DriverRate",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblDriverRate),
                    EntityId = code,
                    Description =
                        $"แก้ไขเรทจ่ายเงินพนักงานขับรถ {code} " +
                        $"จาก {oldRateValue:#,##0.00} เป็น {value:#,##0.00} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "EditDriverRate",
                code);
        }
    }

    public async Task<ResponseViewModel> EditSupplyerRate(
        decimal value,
        string supplier_code,
        string place_code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(supplier_code) ||
            string.IsNullOrWhiteSpace(place_code))
        {
            return Failure(
                "ไม่พบรหัส Supplier หรือรหัสสถานที่");
        }

        try
        {
            var rate = await db.TblSupplierRates
                .FindAsync(
                    new object?[]
                    {
                        supplier_code,
                        place_code
                    },
                    cancellationToken);

            if (rate == null)
            {
                return Failure(
                    $"ไม่พบเรท Supplier {supplier_code} สำหรับสถานที่ {place_code}");
            }

            var oldValues = GetScalarSnapshot(rate);
            var oldRateValue = rate.CostRate;

            rate.CostRate = value;

            SetAuditFieldsIfPresent(
                rate,
                username);

            var newValues = GetScalarSnapshot(rate);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "SupplierRate",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblSupplierRate),
                    EntityId =
                        $"{supplier_code} | {place_code}",
                    Description =
                        $"แก้ไขเรทจ่ายเงิน Supplier {supplier_code} " +
                        $"สถานที่ {place_code} จาก {oldRateValue:#,##0.00} " +
                        $"เป็น {value:#,##0.00} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "EditSupplyerRate",
                $"{supplier_code}/{place_code}");
        }
    }

    public async Task<ResponseViewModel> EditFleetRate(
        decimal value,
        string fleet_code,
        string place_code,
        string type,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fleet_code) ||
            string.IsNullOrWhiteSpace(place_code))
        {
            return Failure(
                "ไม่พบรหัส Fleet หรือรหัสสถานที่");
        }

        type = type?.Trim().ToUpperInvariant() ?? string.Empty;

        if (type is not ("4W" or "6W" or "10W"))
        {
            return Failure(
                "ประเภทรถไม่ถูกต้อง ต้องเป็น 4W, 6W หรือ 10W");
        }

        try
        {
            var rate = await db.TblFleetRates
                .FindAsync(
                    new object?[]
                    {
                        fleet_code,
                        place_code
                    },
                    cancellationToken);

            if (rate == null)
            {
                return Failure(
                    $"ไม่พบเรท Fleet {fleet_code} สำหรับสถานที่ {place_code}");
            }

            var oldValues = GetScalarSnapshot(rate);

            decimal oldRateValue;

            switch (type)
            {
                case "4W":
                    oldRateValue = rate.Truck4w;
                    rate.Truck4w = value;
                    break;

                case "6W":
                    oldRateValue = rate.Truck6w;
                    rate.Truck6w = value;
                    break;

                case "10W":
                    oldRateValue = rate.Truck10w;
                    rate.Truck10w = value;
                    break;

                default:
                    return Failure(
                        "ประเภทรถไม่ถูกต้อง");
            }

            SetAuditFieldsIfPresent(
                rate,
                username);

            var newValues = GetScalarSnapshot(rate);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "FleetRate",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblFleetRate),
                    EntityId =
                        $"{fleet_code} | {place_code}",
                    Description =
                        $"แก้ไขเรท Fleet {fleet_code} รถ {type} " +
                        $"สถานที่ {place_code} จาก {oldRateValue:#,##0.00} " +
                        $"เป็น {value:#,##0.00} โดย {username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "EditFleetRate",
                $"{fleet_code}/{place_code}/{type}");
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

    /*
     * ไม่ผูก compile-time กับ UpdateDate / UpdateBy
     * เพราะ entity rate แต่ละตัวอาจมี schema ต่างกัน
     */
    private void SetAuditFieldsIfPresent(
        object entity,
        string username)
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
}
