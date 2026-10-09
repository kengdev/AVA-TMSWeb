using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class FuelRateRepository
{
    private readonly DbavalogContext db;
    private readonly IAuditLogService auditLogService;
    private readonly ILogger<FuelRateRepository> logger;

    public FuelRateRepository(
        DbavalogContext context,
        IAuditLogService _auditLogService,
        ILogger<FuelRateRepository> _logger)
    {
        db = context;
        auditLogService = _auditLogService;
        logger = _logger;
    }

    public async Task<ResponseViewModel> EditFuelRate(
        string zone_code,
        string truck_typegroup_code,
        int value,
        string username,
        CancellationToken cancellationToken = default)
    {
        zone_code = zone_code?.Trim() ?? string.Empty;
        truck_typegroup_code =
            truck_typegroup_code?.Trim() ?? string.Empty;

        if (zone_code.Length == 0)
        {
            return Failure("ไม่พบรหัส Zone");
        }

        if (truck_typegroup_code.Length == 0)
        {
            return Failure("ไม่พบรหัสกลุ่มประเภทรถ");
        }

        if (value < 0)
        {
            return Failure("เรทน้ำมันต้องไม่ต่ำกว่า 0");
        }

        try
        {
            var rate = await db.TblFuelRates
                .FirstOrDefaultAsync(
                    w =>
                        w.ZoneCode == zone_code &&
                        w.TruckTypegroupCode ==
                        truck_typegroup_code,
                    cancellationToken);

            if (rate == null)
            {
                return Failure(
                    $"ไม่พบเรทน้ำมัน Zone {zone_code} " +
                    $"กลุ่มรถ {truck_typegroup_code}");
            }

            if (rate.FuelRateValue == value)
            {
                return Success();
            }

            var oldValues =
                GetScalarSnapshot(rate);

            var oldRateValue =
                rate.FuelRateValue;

            rate.FuelRateValue = value;
            rate.UpdateDate = DateTime.Now;
            rate.UpdateBy = username;

            var newValues =
                GetScalarSnapshot(rate);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "FuelRate",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblFuelRate),

                    EntityId =
                        $"{zone_code} | {truck_typegroup_code}",

                    Description =
                        $"แก้ไขเรทการจ่ายน้ำมัน Zone {zone_code} " +
                        $"กลุ่มรถ {truck_typegroup_code} " +
                        $"จาก {oldRateValue:#,##0} เป็น {value:#,##0} " +
                        $"โดย {username}",

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
                "EditFuelRate",
                $"{zone_code}/{truck_typegroup_code}");
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
