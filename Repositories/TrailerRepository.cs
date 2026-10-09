using Microsoft.EntityFrameworkCore;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class TrailerRepository
{
    private readonly DbavalogContext db;
    private readonly AuditLogService auditLogService;
    private readonly CodeRepository codeRepo;

    public TrailerRepository(DbavalogContext context, AuditLogService _auditLogService, CodeRepository _codeRepo)
    {
        db = context;
        auditLogService = _auditLogService;
        codeRepo = _codeRepo;
    }

    // =====================================================
    // Get Trailer By Code
    // =====================================================
    public async Task<TblTrailer?> GetTrailerDataByCode(
        string trailer_code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblTrailers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.TrailerCode == trailer_code,
                cancellationToken);
    }

    // =====================================================
    // Get Trailer List
    // =====================================================
    public async Task<List<TrailerViewModel>> GetTruckDataList(
        CancellationToken cancellationToken = default)
    {
        return await db.ViewTrailerListActives
            .AsNoTracking()
            .Select(a => new TrailerViewModel
            {
                trailer_code = a.TrailerCode,
                trailer_license = a.TrailerLicense,
                trailer_type_name = a.TrailerTypeName,
                trailer_floor_name = a.TrailerFloorName,
                truck_license = a.TruckLicense,
                trailer_no = a.TrailerNo,
                is_actived = a.IsActived
            })
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Get Trailer By License
    // =====================================================
    public async Task<ViewTrailerListActive?> GetTrailerByLicense(
        string license,
        CancellationToken cancellationToken = default)
    {
        return await db.ViewTrailerListActives
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.TrailerLicense == license,
                cancellationToken);
    }

    // =====================================================
    // Get Insurance By License
    // =====================================================
    public async Task<List<ViewTrailerInsuranceList>> GetInsuranceByLicense(
        string license,
        CancellationToken cancellationToken = default)
    {
        return await db.ViewTrailerInsuranceLists
            .AsNoTracking()
            .Where(w =>
                w.TrailerLicense == license &&
                w.IsEnabled == true)
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Get Insurance Name
    // =====================================================
    public async Task<TblInsurance?> GetInsuranceName(
        string insurance_code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblInsurances
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.InsuranceCode == insurance_code,
                cancellationToken);
    }

    // =====================================================
    // Get Insurance Type
    // =====================================================
    public async Task<TblInsuranceType?> GetInsuranceTypeName(
        string insurance_type_code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblInsuranceTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.InsuranceTypeCode == insurance_type_code,
                cancellationToken);
    }

    // =====================================================
    // Get Truck By Trailer
    // =====================================================
    public async Task<TblTruckTrailer?> GetTruckByTrailer(
        string trailer_code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblTruckTrailers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.TrailerCode == trailer_code,
                cancellationToken);
    }

    // =====================================================
    // Add Trailer
    // =====================================================
    public async Task<ResponseViewModel> AddTrailer(
        TblTrailer trailerToAdd,
        string truck_code,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            trailerToAdd.TrailerCode = await codeRepo.GetTrailerCode();

            db.TblTrailers.Add(
                trailerToAdd);

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
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }

    // =====================================================
    // Edit Trailer Insurance
    // =====================================================
    public async Task<ResponseViewModel> EditTrailerInsurance(
        TblTrailer model,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            var now = DateTime.Now;

            // =================================================
            // Disable Old Insurance
            // =================================================
            var oldInsurances =
                await db.TblTrailerInsurances
                    .Where(w =>
                        w.TrailerCode ==
                        model.TrailerCode)
                    .ToListAsync(cancellationToken);

            foreach (var item in oldInsurances)
            {
                item.IsEnabled = false;
                item.UpdateDate = now;
                item.UpdateBy = username;
            }

            // =================================================
            // Add New Insurance
            // =================================================
            foreach (var item in model.TblTrailerInsurances)
            {
                var trailerInsurance =
                    new TblTrailerInsurance
                    {
                        TrailerCode =
                            model.TrailerCode,

                        InsuranceCode =
                            item.InsuranceTypeCode,

                        InsuranceTypeCode =
                            item.InsuranceTypeCode,

                        InsuranceBudget =
                            item.InsuranceBudget,

                        InsuranceIncrease =
                            item.InsuranceIncrease,

                        InsuranceDetail =
                            "-",

                        StartedDate =
                            item.StartedDate,

                        ExpiredDate =
                            item.ExpiredDate,

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

                db.TblTrailerInsurances.Add(
                    trailerInsurance);
            }

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Trailer",
                    ActionName = AuditActions.Create,
                    EntityName = nameof(TblTrailer),
                    EntityId = model.TrailerCode,
                    Description = $"เพิ่มข้อมูลหาง {model.TrailerLicense}",
                    NewValues = new
                    {
                        model
                    },
                    Result = AuditResults.Success
                },
                cancellationToken);

            /*
             * SaveChangesAsync ครั้งเดียว
             *
             * EF Core จะสร้าง Transaction ให้เอง
             * ทั้ง Update insurance เดิม
             * Add insurance ใหม่
             * และ Log
             */
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
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }

    // =====================================================
    // Edit Trailer
    // =====================================================
    public async Task<ResponseViewModel> EditTrailer(
        TblTrailer model,
        string truck_code,
        CancellationToken cancellationToken = default)
    {
        var response = new ResponseViewModel();

        try
        {
            // =================================================
            // Remove Old Truck-Trailer
            // =================================================
            var oldTruckTrailer =
                await db.TblTruckTrailers
                    .FirstOrDefaultAsync(
                        w =>
                            w.TruckCode == truck_code &&
                            w.TrailerCode == model.TrailerCode,
                        cancellationToken);
            var oldData = oldTruckTrailer;

            db.TblTruckTrailers.Remove(oldTruckTrailer!);

            // =================================================
            // Add Truck-Trailer
            // =================================================
            var truckTrailer =
                new TblTruckTrailer
                {
                    TruckCode =
                        truck_code,

                    TrailerCode = model.TrailerCode
                };

            db.TblTruckTrailers.Add(
                truckTrailer);

            // =================================================
            // Update Trailer
            // =================================================
            db.TblTrailers.Update(model);

            await auditLogService.AddAsync(
            new AuditLogEntry
            {
                ModuleName = "Trailer",
                ActionName = AuditActions.Update,
                EntityName = nameof(TblTrailer),
                EntityId = model.TrailerCode,
                Description = $"แก้ไขข้อมูลหาง {model.TrailerLicense}",
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
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }

    // =====================================================
    // Remove Trailer
    // =====================================================
    public async Task<ResponseViewModel> Remove(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response =
            new ResponseViewModel();

        try
        {
            var now =
                DateTime.Now;

            // =================================================
            // Truck-Trailer
            // =================================================
            var truckTrailer =
                await db.TblTruckTrailers
                    .FirstOrDefaultAsync(
                        w => w.TrailerCode == code,
                        cancellationToken);

            db.TblTruckTrailers.Remove(
                truckTrailer!);

            // =================================================
            // Trailer
            // =================================================
            var trailer =
                await db.TblTrailers.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            trailer!.IsActived = false;
            trailer.IsEnabled = false;
            trailer.UpdateDate = now;
            trailer.UpdateBy = username;

            await auditLogService.AddAsync(
            new AuditLogEntry
            {
                ModuleName = "Trailer",
                ActionName = AuditActions.Delete,
                EntityName = nameof(TblTrailer),
                EntityId = trailer.TrailerCode,
                Description = $"ลบข้อมูลหาง {trailer.TrailerLicense} โดย {username}",
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
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }

    // =====================================================
    // Active Trailer
    // =====================================================
    public async Task<ResponseViewModel> Active(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response =
            new ResponseViewModel();

        try
        {
            var trailer =
                await db.TblTrailers.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            trailer!.IsActived = true;
            trailer.UpdateDate = DateTime.Now;
            trailer.UpdateBy = username;

            await auditLogService.AddAsync(
            new AuditLogEntry
            {
                ModuleName = "Trailer",
                ActionName = AuditActions.Active,
                EntityName = nameof(TblAgent),
                EntityId = trailer.TrailerCode,
                Description = $"เปิดใช้งานหาง {trailer.TrailerLicense} โดย {username}",
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
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }

    // =====================================================
    // Inactive Trailer
    // =====================================================
    public async Task<ResponseViewModel> Inactive(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response =
            new ResponseViewModel();

        try
        {
            var trailer =
                await db.TblTrailers.FindAsync(
                    new object?[] { code },
                    cancellationToken);

            trailer!.IsActived = false;
            trailer.UpdateDate = DateTime.Now;
            trailer.UpdateBy = username;

            await auditLogService.AddAsync(
            new AuditLogEntry
            {
                ModuleName = "Trailer",
                ActionName = AuditActions.Inactive,
                EntityName = nameof(TblTrailer),
                EntityId = trailer.TrailerCode,
                Description = $"ปิดใช้งาน Agent {trailer.TrailerLicense} โดย {username}",
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
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";

            response.errorException = ex;
        }

        return response;
    }
}