using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class DriverJobRepository
{
    private readonly DbavalogContext db;
    private readonly ILogger<DriverJobRepository> logger;

    public DriverJobRepository(
        DbavalogContext context,
        ILogger<DriverJobRepository> _logger)
    {
        db = context;
        logger = _logger;
    }

    // =====================================================
    // Get Order By Hash Code
    // =====================================================
    public async Task<ViewJobListActive?> GetOrderByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.ViewJobListActives
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.HashCode == code,
                cancellationToken);
    }

    // =====================================================
    // Get Job Order By Hash Code
    // =====================================================
    public async Task<ViewOrderListActive?> GetJobOrderByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.ViewOrderListActives
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.HashCode == code,
                cancellationToken);
    }

    // =====================================================
    // Update ST
    // =====================================================
    public async Task<ResponseViewModel> UpdateST(
        string code,
        string driver_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัสงาน");
        }

        try
        {
            var data = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code,
                    cancellationToken);

            if (data == null)
            {
                return Failure(
                    $"ไม่พบรายการงาน {code}");
            }

            data.TrackingStatus = "ST";

            AddTracking(
                code,
                "ST",
                "-",
                "-");

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
                "UpdateST",
                code);
        }
    }

    // =====================================================
    // Update WP
    // =====================================================
    public async Task<ResponseViewModel> UpdateWP(
        UpdateDriverJobModel model,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateModel(
            model,
            out var validationResponse))
        {
            return validationResponse;
        }

        try
        {
            var data = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (data == null)
            {
                return Failure(
                    $"ไม่พบรายการงาน {model.job_detail_code}");
            }

            data.TrackingStatus = "WP";
            data.ContainerNo =
                NormalizeUpper(model.container_no);
            data.SealNo =
                NormalizeUpper(model.seal_no);
            data.TrailerLicense =
                NormalizeText(model.trailer_license);
            data.TareWeight =
                model.tare_weight;

            AddTracking(
                data.JobDetailCode,
                "WP",
                model.Lat,
                model.Lon);

            var images =
                new List<TblJobImg>();

            if (model.container_pic != null)
            {
                images.Add(
                    new TblJobImg
                    {
                        JobDetailCode =
                            model.job_detail_code,
                        JobType = "container",
                        JobDetailImg =
                            model.container_pic
                    });
            }

            if (model.seal_pic != null)
            {
                images.Add(
                    new TblJobImg
                    {
                        JobDetailCode =
                            model.job_detail_code,
                        JobType = "seal",
                        JobDetailImg =
                            model.seal_pic
                    });
            }

            if (images.Count > 0)
            {
                db.TblJobImgs.AddRange(images);
            }

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
                "UpdateWP",
                model.job_detail_code);
        }
    }

    // =====================================================
    // Import - AR
    // =====================================================
    public async Task<ResponseViewModel> UpdateImportAR(
        UpdateDriverJobModel model,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateModel(
            model,
            out var validationResponse))
        {
            return validationResponse;
        }

        try
        {
            var data = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (data == null)
            {
                return Failure(
                    $"ไม่พบรายการงาน {model.job_detail_code}");
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (cashing == null)
            {
                return Failure(
                    $"ไม่พบข้อมูล Cashing ของงาน {model.job_detail_code}");
            }

            var advance = await db.TblJobadvances
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (advance == null)
            {
                return Failure(
                    $"ไม่พบข้อมูล Advance ของงาน {model.job_detail_code}");
            }

            data.TrackingStatus = "AR";
            cashing.CashingFuelup = model.fuel;

            // คง business logic เดิม:
            // Import AR ใช้ค่า yard ไปเก็บ AdvancePortup
            advance.AdvancePortup = model.yard;

            AddTracking(
                model.job_detail_code,
                "AR",
                model.Lat,
                model.Lon);

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
                "UpdateImportAR",
                model.job_detail_code);
        }
    }

    // =====================================================
    // Import - FN
    // =====================================================
    public async Task<ResponseViewModel> UpdateImportFN(
        UpdateDriverJobModel model,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateModel(
            model,
            out var validationResponse))
        {
            return validationResponse;
        }

        try
        {
            var data = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (data == null)
            {
                return Failure(
                    $"ไม่พบรายการงาน {model.job_detail_code}");
            }

            data.TrackingStatus = "FN";

            AddTracking(
                model.job_detail_code,
                "FN",
                model.Lat,
                model.Lon);

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
                "UpdateImportFN",
                model.job_detail_code);
        }
    }

    // =====================================================
    // Import - DO
    // =====================================================
    public async Task<ResponseViewModel> UpdateImportDO(
        UpdateDriverJobModel model,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateModel(
            model,
            out var validationResponse))
        {
            return validationResponse;
        }

        try
        {
            var data = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (data == null)
            {
                return Failure(
                    $"ไม่พบรายการงาน {model.job_detail_code}");
            }

            var advance = await db.TblJobadvances
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (advance == null)
            {
                return Failure(
                    $"ไม่พบข้อมูล Advance ของงาน {model.job_detail_code}");
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (cashing == null)
            {
                return Failure(
                    $"ไม่พบข้อมูล Cashing ของงาน {model.job_detail_code}");
            }

            data.TrackingStatus = "DO";

            advance.AdvanceLiftonup =
                model.lift1 +
                model.lift2 +
                model.lift3;

            cashing.CashingExpresswayup =
                model.express;

            AddTracking(
                model.job_detail_code,
                "DO",
                model.Lat,
                model.Lon);

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
                "UpdateImportDO",
                model.job_detail_code);
        }
    }

    // =====================================================
    // Export - AR
    // =====================================================
    public async Task<ResponseViewModel> UpdateExportAR(
        UpdateDriverJobModel model,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateModel(
            model,
            out var validationResponse))
        {
            return validationResponse;
        }

        try
        {
            var data = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (data == null)
            {
                return Failure(
                    $"ไม่พบรายการงาน {model.job_detail_code}");
            }

            var advance = await db.TblJobadvances
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (advance == null)
            {
                return Failure(
                    $"ไม่พบข้อมูล Advance ของงาน {model.job_detail_code}");
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (cashing == null)
            {
                return Failure(
                    $"ไม่พบข้อมูล Cashing ของงาน {model.job_detail_code}");
            }

            data.TrackingStatus = "AR";

            advance.AdvanceLiftonup =
                model.lift1 +
                model.lift2 +
                model.lift3;

            cashing.CashingFuelup =
                model.fuel;

            AddTracking(
                model.job_detail_code,
                "AR",
                model.Lat,
                model.Lon);

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
                "UpdateExportAR",
                model.job_detail_code);
        }
    }

    // =====================================================
    // Export - DO
    // =====================================================
    public async Task<ResponseViewModel> UpdateExportDO(
        UpdateDriverJobModel model,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateModel(
            model,
            out var validationResponse))
        {
            return validationResponse;
        }

        try
        {
            var data = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (data == null)
            {
                return Failure(
                    $"ไม่พบรายการงาน {model.job_detail_code}");
            }

            var advance = await db.TblJobadvances
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (advance == null)
            {
                return Failure(
                    $"ไม่พบข้อมูล Advance ของงาน {model.job_detail_code}");
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (cashing == null)
            {
                return Failure(
                    $"ไม่พบข้อมูล Cashing ของงาน {model.job_detail_code}");
            }

            data.TrackingStatus = "DO";

            // คง business logic เดิม:
            // Export DO ใช้ค่า yard ไปเก็บ AdvancePortup
            advance.AdvancePortup =
                model.yard;

            cashing.CashingExpresswayup =
                model.express;

            AddTracking(
                model.job_detail_code,
                "DO",
                model.Lat,
                model.Lon);

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
                "UpdateExportDO",
                model.job_detail_code);
        }
    }

    // =====================================================
    // Floor - AR
    // =====================================================
    public async Task<ResponseViewModel> UpdateFloorAR(
        UpdateDriverJobModel model,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateModel(
            model,
            out var validationResponse))
        {
            return validationResponse;
        }

        try
        {
            var data = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (data == null)
            {
                return Failure(
                    $"ไม่พบรายการงาน {model.job_detail_code}");
            }

            data.TrackingStatus = "AR";

            AddTracking(
                model.job_detail_code,
                "AR",
                model.Lat,
                model.Lon);

            if (model.floor_images != null)
            {
                var images = model.floor_images
                    .Where(image => image != null)
                    .Select(image =>
                        new TblJobImg
                        {
                            JobDetailCode =
                                model.job_detail_code,
                            JobType = "floor",
                            JobDetailImg = image
                        })
                    .ToList();

                if (images.Count > 0)
                {
                    db.TblJobImgs.AddRange(images);
                }
            }

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
                "UpdateFloorAR",
                model.job_detail_code);
        }
    }

    // =====================================================
    // Floor - DO
    // =====================================================
    public async Task<ResponseViewModel> UpdateFloorDO(
        UpdateDriverJobModel model,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateModel(
            model,
            out var validationResponse))
        {
            return validationResponse;
        }

        try
        {
            var data = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (data == null)
            {
                return Failure(
                    $"ไม่พบรายการงาน {model.job_detail_code}");
            }

            var advance = await db.TblJobadvances
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (advance == null)
            {
                return Failure(
                    $"ไม่พบข้อมูล Advance ของงาน {model.job_detail_code}");
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode ==
                        model.job_detail_code,
                    cancellationToken);

            if (cashing == null)
            {
                return Failure(
                    $"ไม่พบข้อมูล Cashing ของงาน {model.job_detail_code}");
            }

            data.TrackingStatus = "DO";

            advance.AdvancePortup =
                model.yard;

            cashing.CashingFuelup =
                model.fuel;

            AddTracking(
                model.job_detail_code,
                "DO",
                model.Lat,
                model.Lon);

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
                "UpdateFloorDO",
                model.job_detail_code);
        }
    }

    // =====================================================
    // Tracking Helper
    // =====================================================
    private void AddTracking(
        string jobDetailCode,
        string trackingStatus,
        string? latitude,
        string? longitude)
    {
        db.TblJobTrackings.Add(
            new TblJobTracking
            {
                JobDetailCode =
                    jobDetailCode,

                TrackingStatus =
                    trackingStatus,

                TrackingDate =
                    DateTime.Now,

                Lattitude =
                    string.IsNullOrWhiteSpace(latitude)
                        ? "-"
                        : latitude.Trim(),

                Longitude =
                    string.IsNullOrWhiteSpace(longitude)
                        ? "-"
                        : longitude.Trim(),

                Speed = "0"
            });
    }

    private static bool ValidateModel(
        UpdateDriverJobModel? model,
        out ResponseViewModel response)
    {
        if (model == null)
        {
            response =
                Failure("ไม่พบข้อมูลรายการงาน");

            return false;
        }

        if (string.IsNullOrWhiteSpace(
            model.job_detail_code))
        {
            response =
                Failure("ไม่พบรหัสรายการงาน");

            return false;
        }

        response = Success();
        return true;
    }

    private static string NormalizeUpper(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "-"
            : value.Trim().ToUpperInvariant();
    }

    private static string NormalizeText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "-"
            : value.Trim();
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
