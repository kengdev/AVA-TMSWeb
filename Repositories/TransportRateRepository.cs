using Microsoft.EntityFrameworkCore;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class TransportRateRepository
{
    private readonly DbavalogContext db;
    private readonly IAuditLogService auditLogService;
    private readonly CodeRepository codeRepo;

    public TransportRateRepository(DbavalogContext context, IAuditLogService _auditLogService, CodeRepository _codeRepo)
    {
        db = context;
        auditLogService = _auditLogService;
        codeRepo = _codeRepo;
    }

    public async Task<ResponseViewModel> AddCost(
        TblCustomerCost model,
        decimal fleet_cost,
        decimal driver_cost,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            model.CostCode =
                await codeRepo.GetCostCode(cancellationToken);

            db.TblCustomerCosts.Add(model);

            /*-------------------เพิ่มเรทค่าเที่ยว พขร----------------*/

            var driverRateExists =
                await db.TblDriverRates
                    .AsNoTracking()
                    .AnyAsync(
                        w => w.PlaceCode == model.MissionCode,
                        cancellationToken);

            if (!driverRateExists)
            {
                var driverRate = new TblDriverRate
                {
                    PlaceCode = model.MissionCode,
                    RateValue = driver_cost
                };

                db.TblDriverRates.Add(driverRate);
            }

            /*-------------------เพิ่มเรทค่าเที่ยว ซับนอก----------------*/

            var supplierCodes =
                await db.TblSuppliers
                    .AsNoTracking()
                    .Where(w => w.FleetTypeCode == "F1")
                    .Select(w => w.SupplierCode)
                    .ToListAsync(cancellationToken);

            if (supplierCodes.Count > 0)
            {
                var existingSupplierCodes =
                    await db.TblSupplierRates
                        .AsNoTracking()
                        .Where(w =>
                            w.PlaceCode == model.MissionCode &&
                            supplierCodes.Contains(
                                w.SupplierCode))
                        .Select(w => w.SupplierCode)
                        .ToListAsync(cancellationToken);

                var existingSupplierSet =
                    existingSupplierCodes.ToHashSet();

                var supplierRates =
                    supplierCodes
                        .Where(supplierCode =>
                            !existingSupplierSet.Contains(
                                supplierCode))
                        .Select(supplierCode =>
                            new TblSupplierRate
                            {
                                SupplierCode = supplierCode,
                                PlaceCode = model.MissionCode,
                                CostRate = 0
                            })
                        .ToList();

                if (supplierRates.Count > 0)
                {
                    db.TblSupplierRates.AddRange(
                        supplierRates);
                }
            }

            /*-------------------เพิ่มเรทค่าเที่ยว ซับAVA----------------*/

            var fleetCodes =
                await db.TblFleets
                    .AsNoTracking()
                    .Where(w => w.IsEnabled == true)
                    .Select(w => w.FleetCode)
                    .ToListAsync(cancellationToken);

            if (fleetCodes.Count > 0)
            {
                var existingFleetCodes =
                    await db.TblFleetRates
                        .AsNoTracking()
                        .Where(w =>
                            w.PlaceCode == model.MissionCode &&
                            fleetCodes.Contains(
                                w.FleetCode))
                        .Select(w => w.FleetCode)
                        .ToListAsync(cancellationToken);

                var existingFleetSet =
                    existingFleetCodes.ToHashSet();

                var fleetRates =
                    fleetCodes
                        .Where(fleetCode =>
                            !existingFleetSet.Contains(
                                fleetCode))
                        .Select(fleetCode =>
                            new TblFleetRate
                            {
                                FleetCode = fleetCode,
                                PlaceCode = model.MissionCode,
                                Truck4w = fleet_cost,
                                Truck6w = fleet_cost,
                                Truck10w = fleet_cost
                            })
                        .ToList();

                if (fleetRates.Count > 0)
                {
                    db.TblFleetRates.AddRange(
                        fleetRates);
                }
            }

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "TransportRate",
                    ActionName = AuditActions.Create,
                    EntityName = nameof(TblCustomerCost),
                    EntityId = model.CostCode,
                    Description = $"เพิ่มข้อมูลเรทการคิดเงิน",
                    NewValues = new
                    {
                        model
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

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

    public async Task<List<CostViewModel>> GetCostDataList(
        CancellationToken cancellationToken = default)
    {
        return await db.ViewCostListActives
            .AsNoTracking()
            .Where(w => w.CostCode != null)
            .Select(a => new CostViewModel
            {
                customer_code = a.CustomerCode,
                customer_name_th = a.CustomerNameTh,
                cost_code = a.CostCode,
                cost_value = a.CostValue.Value,
                mission_name = a.MissionName,
                is_actived = a.IsActived.Value
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TblCustomerCost?> GetCustomerCostDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblCustomerCosts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.CostCode == code,
                cancellationToken);
    }

    public async Task<CostViewModel?> GetCustomerDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var query =
            from a in db.TblCustomers
            join b in db.TblCustomerCosts
                on a.CustomerCode equals b.CustomerCode
            join c in db.TblPlaces
                on b.MissionCode equals c.PlaceCode
            join d in db.TblIndustries
                on c.PlaceCode equals d.IndustryCode
            where b.CostCode == code
            select new CostViewModel
            {
                mission_name = c.PlaceName,
                customer_name_th = a.CustomerNameTh,
                customer_address = a.CustomerAddress,
                customer_tax = a.CustomerVatNo,
                mission_address = d.IndustryAddress,
                mission_tax = d.IndustryTax
            };

        return await query
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public async Task<CostViewModel?> GetCustomerCostByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await db.ViewCostListActives
            .AsNoTracking()
            .Where(w => w.CostCode == code)
            .Select(a => new CostViewModel
            {
                customer_code = a.CustomerCode,
                customer_name_th = a.CustomerNameTh,
                cost_code = a.CostCode,
                cost_value = a.CostValue.Value
            })
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public async Task<ResponseViewModel> EditCost(
        TblCustomerCost costToEdit,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var oldRate = await db.TblCustomerCosts.Where(w => w.CostCode == costToEdit.CostCode).AsNoTracking().FirstOrDefaultAsync();
            db.TblCustomerCosts.Update(costToEdit);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "TransportRate",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblCustomerCost),
                    EntityId = costToEdit.CostCode,
                    Description = $"แก้ไขข้อมูลเรทการคิดเงิน",
                    OldValues = oldRate,
                    NewValues = new
                    {
                        costToEdit
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            response.result = true;
            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
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

    public async Task<bool> IsDuplicate(
        string customer_code,
        string place_code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblCustomerCosts
            .AsNoTracking()
            .AnyAsync(
                w =>
                    w.CustomerCode == customer_code &&
                    w.MissionCode == place_code &&
                    w.IsEnabled == true,
                cancellationToken);
    }

    public async Task<ResponseViewModel> Remove(
        string code,
        string update_by,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var cost =
                await db.TblCustomerCosts.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            cost!.IsActived = false;
            cost.IsEnabled = false;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "TransportRate",
                    ActionName = AuditActions.Delete,
                    EntityName = nameof(TblCustomerCost),
                    EntityId = code,
                    Description = $"ลบข้อมูลเรทการคิดเงิน โดย {update_by}",
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            response.result = true;
            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
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
        string update_by,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var cost =
                await db.TblCustomerCosts.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            cost!.IsActived = true;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "TransportRate",
                    ActionName = AuditActions.Active,
                    EntityName = nameof(TblCustomerCost),
                    EntityId = code,
                    Description = $"เปิดใช้งานเรทการคิดเงิน โดย {update_by}",
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            response.result = true;
            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
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

    public async Task<ResponseViewModel> Inactive(
        string code,
        string update_by,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var cost =
                await db.TblCustomerCosts.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            cost!.IsActived = false;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "TransportRate",
                    ActionName = AuditActions.Inactive,
                    EntityName = nameof(TblCustomerCost),
                    EntityId = code,
                    Description = $"ปิดใช้งานเรทการคิดเงิน โดย {update_by}",
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(
                cancellationToken);

            response.result = true;
            response.resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว";
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