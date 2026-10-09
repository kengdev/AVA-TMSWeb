using Microsoft.EntityFrameworkCore;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class ZoneRepository
{
    private readonly DbavalogContext db;
    private readonly IAuditLogService auditLogService;
    private readonly CodeRepository codeRepo;

    public ZoneRepository(
        DbavalogContext context,
        IAuditLogService auditLogService,
        CodeRepository _codeRepo)
    {
        db = context;
        this.auditLogService = auditLogService;
        this.codeRepo = _codeRepo;
    }

    public async Task<List<ZoneViewModel>> GetZoneDataList()
    {
        var query = await
            (from zone in db.TblZones
             join tr4 in db.TblFuelRates.Where(x => x.TruckTypegroupCode == "01")
                 on zone.ZoneCode equals tr4.ZoneCode
             join tr6 in db.TblFuelRates.Where(x => x.TruckTypegroupCode == "02")
                 on zone.ZoneCode equals tr6.ZoneCode
             join tr10 in db.TblFuelRates.Where(x => x.TruckTypegroupCode == "03")
                 on zone.ZoneCode equals tr10.ZoneCode
             where zone.IsActived == true
             select new ZoneViewModel
             {
                 zone_code = zone.ZoneCode,
                 zone_name = zone.ZoneName,
                 tr4 = tr4.FuelRateValue,
                 tr6 = tr6.FuelRateValue,
                 tr10 = tr10.FuelRateValue,
                 is_actived = zone.IsActived
             }).ToListAsync();

        return query;
    }

    public async Task<List<TblZone>> GetZone(
        CancellationToken cancellationToken = default)
    {
        return await db.TblZones
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<TblZone?> GetZoneByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblZones
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.ZoneCode == code,
                cancellationToken);
    }

    public async Task<List<TblFuelRate>> GetFuelRate(
        CancellationToken cancellationToken = default)
    {
        return await db.TblFuelRates
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FuelRateViewModel>> GetFuelRateValue(
        CancellationToken cancellationToken = default)
    {
        return await db.TblZones
            .AsNoTracking()
            .Where(w => w.ZoneCode != "00")
            .Select(w => new FuelRateViewModel
            {
                zone_code = w.ZoneCode,
                zone_name = w.ZoneName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ResponseViewModel> EditRate(
        string[] zone,
        int[] tr4,
        int[] tr6,
        int[] tr10,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();
        try
        {
            await using var transaction =
                await db.Database.BeginTransactionAsync(cancellationToken);

            var rates = await db.TblFuelRates
                .Where(w =>
                    zone.Contains(w.ZoneCode) &&
                    (w.TruckTypegroupCode == "01" ||
                     w.TruckTypegroupCode == "02" ||
                     w.TruckTypegroupCode == "03"))
                .ToListAsync(cancellationToken);

            var rateLookup = rates.ToDictionary(
                x => (x.ZoneCode, x.TruckTypegroupCode));

            for (int i = 0; i < zone.Length; i++)
            {
                rateLookup[(zone[i], "01")].FuelRateValue = tr4[i];
                rateLookup[(zone[i], "02")].FuelRateValue = tr6[i];
                rateLookup[(zone[i], "03")].FuelRateValue = tr10[i];
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> EditZoneData(
        TblZone zoneToEdit,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            await using var transaction =
                await db.Database.BeginTransactionAsync(cancellationToken);

            db.TblZones.Update(zoneToEdit);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Zone",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblZone),
                    EntityId = zoneToEdit.ZoneCode,
                    Description = $"แก้ไขข้อมูล Zone {zoneToEdit.ZoneName}",
                    NewValues = new
                    {
                        zoneToEdit
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> AddZoneData(
        TblZone zoneToAdd,
        int tr4,
        int tr6,
        int tr10,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            await using var transaction =
                await db.Database.BeginTransactionAsync(cancellationToken);

            zoneToAdd.ZoneCode = await codeRepo.GetZoneCode();

            db.TblZones.Add(zoneToAdd);

            db.TblFuelRates.AddRange(
                new TblFuelRate
                {
                    ZoneCode = zoneToAdd.ZoneCode,
                    TruckTypegroupCode = "01",
                    FuelRateValue = tr4,
                    UpdateDate = DateTime.Now,
                    UpdateBy = zoneToAdd.UpdateBy
                },
                new TblFuelRate
                {
                    ZoneCode = zoneToAdd.ZoneCode,
                    TruckTypegroupCode = "02",
                    FuelRateValue = tr6,
                    UpdateDate = DateTime.Now,
                    UpdateBy = zoneToAdd.UpdateBy
                },
                new TblFuelRate
                {
                    ZoneCode = zoneToAdd.ZoneCode,
                    TruckTypegroupCode = "03",
                    FuelRateValue = tr10,
                    UpdateDate = DateTime.Now,
                    UpdateBy = zoneToAdd.UpdateBy
                });

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> Remove(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var item = await db.TblZones.FindAsync(
                new object?[] { code },
                cancellationToken);

            var rates = await db.TblFuelRates
                .Where(w => w.ZoneCode == code)
                .ToListAsync(cancellationToken);

            db.TblFuelRates.RemoveRange(rates);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Zone",
                    ActionName = AuditActions.Delete,
                    EntityName = nameof(TblZone),
                    EntityId = code,
                    Description = $"ลบข้อมูล Zone {item.ZoneName}",
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> Active(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var item = await db.TblZones.FindAsync(
                new object?[] { code },
                cancellationToken);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Zone",
                    ActionName = AuditActions.Active,
                    EntityName = nameof(TblZone),
                    EntityId = code,
                    Description = $"เปิดใช้งาน Zone {item.ZoneName}",
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> InActive(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();
        try
        {

            var item = await db.TblZones.FindAsync(
                new object?[] { code },
                cancellationToken);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Zone",
                    ActionName = AuditActions.Inactive,
                    EntityName = nameof(TblZone),
                    EntityId = code,
                    Description = $"ปิดใช้งาน Zone {item.ZoneName}",
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }
}