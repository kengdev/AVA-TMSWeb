using Microsoft.EntityFrameworkCore;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class TruckRepository
{
    private readonly DbavalogContext db;
    private readonly IAuditLogService auditLogService;

    public TruckRepository(DbavalogContext context, IAuditLogService _auditLogService)
    {
        db = context;
        auditLogService = _auditLogService;
    }

    public async Task<List<TruckViewModel>> GetDriverListByTruck(
        string code,
        CancellationToken cancellationToken = default)
    {
        var query = await (
            from a in db.TblTruckDrivers.AsNoTracking()
            join b in db.TblDrivers.AsNoTracking()
                on a.DriverCode equals b.DriverCode
            where a.TruckCode == code
            select new TruckViewModel
            {
                truck_code = a.TruckCode,
                driver_code = a.DriverCode,
                driver_name = b.DriverFullname,
                driver_phone = b.DriverPhone
            })
            .ToListAsync(cancellationToken);

        return query;
    }

    public async Task<ViewTruckListActive?> GetTruckByLicense(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await db.ViewTruckListActives
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.TruckCode == code,
                cancellationToken);
    }

    public async Task<TruckModel?> GetTruckDataByCode(
        string truck_code,
        CancellationToken cancellationToken = default)
    {
        var query = await (
            from a in db.TblTrucks.AsNoTracking()
            join b in db.TblTruckDetails.AsNoTracking()
                on a.TruckCode equals b.TruckCode
            where a.TruckCode == truck_code
            select new TruckModel
            {
                truck_code = a.TruckCode,
                truck_license = a.TruckLicense,
                truck_no = b.TruckNo,
                truck_chassis_number = b.TruckChassisNumber,
                truck_year = b.TruckYear,
                brand_code = b.BrandCode,
                truck_type_code = b.TruckTypeCode,
                fuel_type_code = b.FuelTypeCode,
                truck_img = b.TruckImage
            })
            .FirstOrDefaultAsync(cancellationToken);

        return query;
    }

    public async Task<ResponseViewModel> EditTruckDriver(
        TblTruck model,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var dataDriverTruck =
                await db.TblTruckDrivers
                    .Where(w =>
                        w.TruckCode == model.TruckCode)
                    .ToListAsync(cancellationToken);

            var oldData = dataDriverTruck;

            db.TblTruckDrivers.RemoveRange(
                dataDriverTruck);

            var truckDrivers =
                model.TblTruckDrivers
                    .Select(item =>
                        new TblTruckDriver
                        {
                            DriverCode =
                                item.DriverCode,

                            TruckCode =
                                model.TruckCode,

                            CreateDate =
                                DateTime.Now,

                            CreateBy =
                                username
                        })
                    .ToList();

            if (truckDrivers.Count > 0)
            {
                db.TblTruckDrivers.AddRange(
                    truckDrivers);
            }

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "TruckDriver",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblCustomerCost),
                    EntityId = model.TruckCode,
                    Description = $"แก้ไขข้อมูลพนักงานประจำรถ",
                    OldValues = oldData,
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

    public async Task<ResponseViewModel> AddTruck(
        TruckModel truckToAdd,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var group =
                await db.TblTruckTypes
                    .AsNoTracking()
                    .Where(w =>
                        w.TruckTypeCode ==
                        truckToAdd.truck_type_code)
                    .Select(w => new
                    {
                        w.TruckTypegroupCode
                    })
                    .FirstOrDefaultAsync(
                        cancellationToken);

            var model = new TblTruck
            {
                TruckCode =
                    truckToAdd.truck_code,

                TruckLicense =
                    truckToAdd.truck_license,

                SupplierCode =
                    truckToAdd.supplier_code,

                FleetType = "F",
                IsActived = true,
                IsEnabled = true,
                CreateDate = DateTime.Now,
                CreateBy = username,
                UpdateDate = DateTime.Now,
                UpdateBy = username
            };

            var detail = new TblTruckDetail
            {
                TruckCode =
                    model.TruckCode,

                TruckNo =
                    truckToAdd.truck_no.ToUpper(),

                TruckChassisNumber =
                    truckToAdd
                        .truck_chassis_number
                        .ToUpper(),

                TruckYear =
                    truckToAdd.truck_year,

                BrandCode =
                    truckToAdd.brand_code,

                TruckTypeCode =
                    truckToAdd.truck_type_code,

                FuelTypeCode =
                    truckToAdd.fuel_type_code,

                TruckTypegroupCode =
                    group!.TruckTypegroupCode,

                TruckImage =
                    truckToAdd.truck_img
            };

            model.TblTruckDetail = detail;

            db.TblTrucks.Add(model);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Truck",
                    ActionName = AuditActions.Create,
                    EntityName = nameof(TblTruck),
                    EntityId = model.TruckCode,
                    Description = $"เพิ่มข้อมูล รถหัวลาก {model.TruckLicense}",
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

    public async Task<List<TruckViewModel>> GetTruckDataList(
        CancellationToken cancellationToken = default)
    {
        var data =
            await db.ViewTruckListActives
                .AsNoTracking()
                .Where(w =>
                    w.TruckCode != "T000000")
                .OrderBy(w =>
                    w.TruckNo)
                .Select(a =>
                    new TruckViewModel
                    {
                        truck_code =
                            a.TruckCode,

                        truck_license =
                            a.TruckLicense,

                        truck_chassis_number =
                            a.TruckChassisNumber,

                        truck_no =
                            a.TruckNo,

                        truck_type_name =
                            a.TruckTypeName,

                        brand_name =
                            a.BrandName,

                        fuel_type_name =
                            a.FuelTypeName,

                        supplier_code =
                            a.SupplierCode,

                        supplier_name =
                            a.SupplierName,

                        driver_code =
                            a.DriverCode,

                        driver_name =
                            a.DriverFullname,

                        driver_phone =
                            a.DriverPhone,

                        is_actived =
                            a.IsActived
                    })
                .ToListAsync(
                    cancellationToken);

        return data;
    }

    public async Task<ViewDriverListActive?> GetDriverByDriverCode(
        string driver_code,
        CancellationToken cancellationToken = default)
    {
        return await db.ViewDriverListActives
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.DriverCode == driver_code,
                cancellationToken);
    }

    public async Task<TblInsurance?> GetInsuranceName(
        string insurance_code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblInsurances
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w =>
                    w.InsuranceCode ==
                    insurance_code,
                cancellationToken);
    }

    public async Task<TblInsuranceType?> GetInsuranceTypeName(
        string insurance_type_code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblInsuranceTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w =>
                    w.InsuranceTypeCode ==
                    insurance_type_code,
                cancellationToken);
    }

    public async Task<ResponseViewModel> EditTruck(
        TruckModel truckToEdit,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var group =
                await db.TblTruckTypes
                    .AsNoTracking()
                    .Where(w =>
                        w.TruckTypeCode ==
                        truckToEdit.truck_type_code)
                    .Select(w => new
                    {
                        w.TruckTypegroupCode
                    })
                    .FirstOrDefaultAsync(
                        cancellationToken);

            var item =
                await db.TblTrucks.FindAsync(
                    new object?[]
                    {
                        truckToEdit.truck_code
                    },
                    cancellationToken);

            var oldData = item;

            item!.TruckLicense =
                truckToEdit.truck_license;

            item.UpdateDate = DateTime.Now;
            item.UpdateBy = username;

            var removeDetail =
                await db.TblTruckDetails.FindAsync(
                    new object?[]
                    {
                        truckToEdit.truck_code
                    },
                    cancellationToken);

            if (removeDetail != null)
            {
                db.TblTruckDetails.Remove(
                    removeDetail);
            }

            var detail = new TblTruckDetail
            {
                TruckCode =
                    truckToEdit.truck_code,

                TruckNo =
                    truckToEdit.truck_no.ToUpper(),

                TruckChassisNumber =
                    truckToEdit
                        .truck_chassis_number
                        .ToUpper(),

                TruckYear =
                    truckToEdit.truck_year,

                BrandCode =
                    truckToEdit.brand_code,

                TruckTypeCode =
                    truckToEdit.truck_type_code,

                TruckTypegroupCode =
                    group!.TruckTypegroupCode,

                FuelTypeCode =
                    truckToEdit.fuel_type_code,

                TruckImage =
                    truckToEdit.truck_img
            };

            db.TblTruckDetails.Add(detail);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Truck",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblTruck),
                    EntityId = truckToEdit.truck_code,
                    Description = $"แก้ไขข้อมูลรถหัวลาก {truckToEdit.truck_license}",
                    OldValues = oldData,
                    NewValues = new
                    {
                        truckToEdit
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

    public async Task<ResponseViewModel> Remove(
        string code,
        string update_by,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var dataTruck =
                await db.TblTrucks.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            var driverTruck =
                await db.TblTruckDrivers
                    .FirstOrDefaultAsync(
                        w => w.TruckCode == code,
                        cancellationToken);

            var trailerTruck =
                await db.TblTruckTrailers
                    .FirstOrDefaultAsync(
                        w => w.TruckCode == code,
                        cancellationToken);

            if (driverTruck != null)
            {
                db.TblTruckDrivers.Remove(
                    driverTruck);
            }

            if (trailerTruck != null)
            {
                db.TblTruckTrailers.Remove(
                    trailerTruck);
            }

            dataTruck!.IsActived = false;
            dataTruck.IsEnabled = false;
            dataTruck.UpdateDate = DateTime.Now;
            dataTruck.UpdateBy = update_by;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Truck",
                    ActionName = AuditActions.Delete,
                    EntityName = nameof(TblTruck),
                    EntityId = code,
                    Description = $"ลบข้อมูลรถหัวลาก โดย {update_by}",
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
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var dataTruck =
                await db.TblTrucks.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            dataTruck!.IsActived = true;
            dataTruck.UpdateDate = DateTime.Now;
            dataTruck.UpdateBy = username;

            await auditLogService.AddAsync(
            new AuditLogEntry
            {
                ModuleName = "Truck",
                ActionName = AuditActions.Active,
                EntityName = nameof(TblTruck),
                EntityId = dataTruck.TruckCode,
                Description = $"เปิดใช้งาน Truck {dataTruck.TruckLicense} โดย {username}",
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
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();
        try
        {
            var dataTruck =
                await db.TblTrucks.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            dataTruck!.IsActived = false;
            dataTruck.UpdateDate = DateTime.Now;
            dataTruck.UpdateBy = username;

            await auditLogService.AddAsync(
            new AuditLogEntry
            {
                ModuleName = "Truck",
                ActionName = AuditActions.Inactive,
                EntityName = nameof(TblTruck),
                EntityId = dataTruck.TruckLicense,
                Description = $"ปิดใช้งาน Truck {dataTruck.TruckLicense} โดย {username}",
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