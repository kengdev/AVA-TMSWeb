using System.Data;
using Microsoft.Extensions.Logging;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using TMSWeb.Repositories;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class ApproveRepository
{
    private readonly DbavalogContext db;
    private readonly CodeRepository codeRepo;
    private readonly IAuditLogService auditLogService;
    private readonly ILogger<ApproveRepository> logger;

    public ApproveRepository(
        DbavalogContext context,
        IAuditLogService _auditLogService,
        CodeRepository _codeRepo,
        ILogger<ApproveRepository> _logger)
    {
        db = context;
        auditLogService = _auditLogService;
        codeRepo = _codeRepo;
        logger = _logger;
    }

    public async Task<List<JobViewModel>> GetJobApproveList(
        CancellationToken cancellationToken = default)
    {
        var data = await (
            from jm in db.TblJobMasters.AsNoTracking()
            join jd in db.TblJobDetails.AsNoTracking()
                on jm.JobCode equals jd.JobCode
            join mp in db.TblPlaces.AsNoTracking()
                on jd.MissionPlaceCode equals mp.PlaceCode
            join sp in db.TblPlaces.AsNoTracking()
                on jd.StartPlaceCode equals sp.PlaceCode
            join cs in db.TblContainerSizes.AsNoTracking()
                on jm.ContainerSizeCode equals cs.ContainerSizeCode
            join t in db.TblTrucks.AsNoTracking()
                on jd.TruckCode equals t.TruckCode
            join s in db.TblSuppliers.AsNoTracking()
                on jd.SupplierCode equals s.SupplierCode
            where jm.IsEnabled == true
                  && jd.IsEnabled == true
                  && jd.IsDocument == true
                  && jd.IsCanceled == false
                  && jd.IsBilling == false
                  && jd.JobDetailCode != null
            select new
            {
                jd.JobCode,
                jd.JobDetailCode,
                jm.JobDate,
                MissionName = mp.PlaceName,
                StartName = sp.PlaceName,
                jm.BookingNo,
                jd.ContainerNo,
                cs.ContainerSizeName,
                t.TruckLicense,
                s.SupplierName,
                jd.TrackingStatus,
                jd.IsDocument,
                jd.IsApprove,
                jd.IsBilling
            })
            .ToListAsync(cancellationToken);

        return data.Select(a => new JobViewModel
        {
            job_code = a.JobCode,
            job_detail_code = a.JobDetailCode,
            job_date = a.JobDate.ToString("yyyy-MM-dd"),
            job_date_value = a.JobDate,
            mission_name = a.MissionName,
            start_name = a.StartName,
            booking_no = a.BookingNo,
            container_no = a.ContainerNo,
            container_size = a.ContainerSizeName,
            truck_license = a.TruckLicense,
            supplier_name = a.SupplierName,
            tracking_status = a.TrackingStatus,
            is_document = a.IsDocument,
            is_approve = a.IsApprove,
            is_billing = a.IsBilling
        }).ToList();
    }

    public async Task<List<JobViewModel>> GetJobChangeDriver(
        CancellationToken cancellationToken = default)
    {
        var data = await (
            from a in db.TblJobDetails.AsNoTracking()
            join b in db.TblDriverCosts.AsNoTracking()
                on a.JobDetailCode equals b.JobDetailCode
            join c in db.TblJobMasters.AsNoTracking()
                on a.JobCode equals c.JobCode
            join d in db.ViewDriverListActives.AsNoTracking()
                on a.DriverCode equals d.DriverCode
            where b.CostType == "B"
            orderby c.JobDate
            select new
            {
                a.JobDetailCode,
                c.JobDate,
                c.BookingNo,
                a.ContainerNo,
                c.ContainerSizeName,
                d.TruckLicense,
                d.DriverFullname,
                a.IsBilling,
                b.TripValue
            })
            .ToListAsync(cancellationToken);

        return data.Select(a => new JobViewModel
        {
            job_detail_code = a.JobDetailCode,
            job_date = a.JobDate.ToString("yyyy-MM-dd"),
            booking_no = a.BookingNo,
            container_no = a.ContainerNo,
            container_size = a.ContainerSizeName,
            truck_license = a.TruckLicense,
            driver_fullname = a.DriverFullname,
            is_billing = a.IsBilling,
            cost_value = a.TripValue
        }).ToList();
    }

    public async Task<List<JobViewModel>> GetJobBillingList(
        CancellationToken cancellationToken = default)
    {
        var data = await db.ViewJobListActives
            .AsNoTracking()
            .Where(a =>
                a.IsDocument == true &&
                a.IsCanceled == false &&
                a.IsApprove == true &&
                a.JobDetailCode != null &&
                a.IsBilling == false)
            .Select(a => new
            {
                a.JobCode,
                a.JobType,
                a.CustomerNameTh,
                a.SupplierName,
                a.BookingNo,
                a.ContainerType,
                a.ContainerSizeName,
                a.JobStatus,
                a.JobDate,
                a.CashingTotal,
                a.IsBilling,
                a.StartName,
                a.MissionName,
                a.JobDetailCode,
                a.TruckLicense,
                a.DriverFullname,
                a.ContainerNo,
                a.IsDocument
            })
            .ToListAsync(cancellationToken);

        return data.Select(a => new JobViewModel
        {
            job_code = a.JobCode,
            job_type = a.JobType,
            customer_name = a.CustomerNameTh,
            supplier_name = a.SupplierName,
            booking_no = a.BookingNo,
            container_type = a.ContainerType,
            container_size = a.ContainerSizeName,
            job_status = a.JobStatus,
            job_date = a.JobDate.ToString("yyyy-MM-dd"),
            job_date_value = a.JobDate,
            cashing_total = a.CashingTotal,
            is_billing = a.IsBilling,
            start_name = a.StartName,
            mission_name = a.MissionName,
            job_detail_code = a.JobDetailCode,
            truck_license = a.TruckLicense,
            trailer_license = a.TruckLicense,
            driver_fullname = a.DriverFullname,
            container_no = a.ContainerNo,
            is_document = a.IsDocument
        }).ToList();
    }

    public async Task<List<DriverCostViewModel>> GetDriverCost(
        string driver_code,
        string job_detail_code,
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in db.TblDriverCosts.AsNoTracking()
            join b in db.ViewDriverListActives.AsNoTracking()
                on a.DriverCode equals b.DriverCode
            where a.JobDetailCode == job_detail_code &&
                  a.DriverCode == driver_code
            orderby a.CostType
            select new DriverCostViewModel
            {
                driver_code = a.DriverCode,
                driver_name = b.DriverFullname,
                cost_value = a.TripValue,
                cost_type = a.CostType
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TblSupplierAllowance?> GetSupplyerCost(
        string supplyer_code,
        string job_detail_code,
        CancellationToken cancellationToken = default)
    {
        var data = await db.TblSupplierAllowances
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.JobDetailCode == job_detail_code &&
                     w.SupplierCode == supplyer_code,
                cancellationToken);

        if (data != null)
        {
            data.CostValue = data.CostReserve > 0
                ? data.CostReserve
                : data.CostValue;
        }

        return data;
    }

    public async Task<ResponseViewModel> SendBack(
        string job_detail_code,
        string username,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var item = await db.TblJobDetails.FindAsync(
                new object?[] { job_detail_code },
                cancellationToken);

            if (item == null)
            {
                return NotFound("ไม่พบข้อมูลงาน");
            }

            var oldValues = GetScalarValues(item);

            item.IsApprove = false;
            item.IsDocument = false;
            item.UpdateDate = DateTime.Now;
            item.UpdateBy = username;

            var newValues = GetScalarValues(item);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Reject job from approve.",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblJobDetail),
                    EntityId = job_detail_code,
                    Description = $"ส่งงานกลับจากการรออนุมัติ {job_detail_code} โดย {username}",
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
            return Error(ex, nameof(SendBack));
        }
    }

    public async Task<ResponseViewModel> ConfirmData(
        FinishJobModel jobToFinish,
        TblJobadvance jobAdvance,
        TblJobcashing jobCashing,
        CancellationToken cancellationToken = default)
    {
        await using var tr = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == jobToFinish.job_detail_code,
                    cancellationToken);

            if (cashing == null)
            {
                await tr.RollbackAsync(cancellationToken);
                return NotFound("ไม่พบข้อมูลค่าใช้จ่าย");
            }

            var advance = await db.TblJobadvances
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == jobToFinish.job_detail_code,
                    cancellationToken);

            if (advance == null)
            {
                await tr.RollbackAsync(cancellationToken);
                return NotFound("ไม่พบข้อมูลเงินสำรอง");
            }

            var itemDetail = await db.TblJobDetails.FindAsync(
                new object?[] { jobToFinish.job_detail_code },
                cancellationToken);

            if (itemDetail == null)
            {
                await tr.RollbackAsync(cancellationToken);
                return NotFound("ไม่พบข้อมูลงาน");
            }

            var supplierAllowance = await db.TblSupplierAllowances.FindAsync(
                new object?[]
                {
                    itemDetail.SupplierCode,
                    itemDetail.JobDetailCode
                },
                cancellationToken);

            if (supplierAllowance == null)
            {
                await tr.RollbackAsync(cancellationToken);
                return NotFound("ไม่พบข้อมูลค่าขนส่ง Supplier");
            }

            ValidateCashingInput(jobToFinish);
            ValidateAdvanceInput(jobToFinish);
            ValidateDriverInput(jobToFinish);

            var oldValues = GetScalarValues(itemDetail);

            // =====================================================
            // Cashing
            // =====================================================
            cashing.CashingFuel = jobCashing.CashingFuel;
            cashing.CashingFuelup = jobCashing.CashingFuelup;
            cashing.CashingExpressway = jobCashing.CashingExpressway;
            cashing.CashingExpresswayup = jobCashing.CashingExpresswayup;

            // เดิมใช้เฉพาะ CashingExpresswayup ทำให้ยอด Fuelup หายจาก Total
            cashing.CashingTotal =
                jobCashing.CashingFuelup +
                jobCashing.CashingExpresswayup;

            if (jobToFinish.valCashing != null)
            {
                await db.TblJobcashingOthers
                    .Where(w => w.CashingCode == cashing.CashingCode)
                    .ExecuteDeleteAsync(cancellationToken);

                var cashingOthers = new List<TblJobcashingOther>();

                for (int i = 0; i < jobToFinish.valCashing.Count(); i++)
                {
                    var name = jobToFinish.nameCashing[i];

                    if (!string.IsNullOrWhiteSpace(name) &&
                        jobToFinish.valCashing[i] != 0)
                    {
                        cashingOthers.Add(new TblJobcashingOther
                        {
                            CashingOther = name.Trim(),
                            CashingOtherValue = jobToFinish.valWithdrawCashing[i],
                            CashingOtherValueup = jobToFinish.valCashing[i],
                            CashingCode = cashing.CashingCode
                        });
                    }
                }

                if (cashingOthers.Count > 0)
                {
                    db.TblJobcashingOthers.AddRange(cashingOthers);
                }
            }

            // =====================================================
            // Advance
            // =====================================================
            advance.AdvanceLiftoffup = jobAdvance.AdvanceLiftoffup;
            advance.AdvanceLiftonup = jobAdvance.AdvanceLiftonup;
            advance.AdvancePortup = jobAdvance.AdvancePortup;
            advance.AdvanceYardup = jobAdvance.AdvanceYardup;
            advance.AdvanceTotalup =
                jobAdvance.AdvanceLiftoffup +
                jobAdvance.AdvanceLiftonup +
                jobAdvance.AdvancePortup +
                jobAdvance.AdvanceYardup;

            if (jobToFinish.valAdvance != null)
            {
                // ถ้ามีการแก้รายการ Other เดิม Billing ที่อ้างถึงรายการเดิม
                // ต้องถูกล้างก่อน เพื่อไม่ให้ค้าง reference ไปยัง record ที่กำลังแทนที่
                await db.TblBillingOthers
                    .Where(w => w.AdvanceCode == advance.AdvanceCode)
                    .ExecuteDeleteAsync(cancellationToken);

                await db.TblJobadvanceOthers
                    .Where(w => w.AdvanceCode == advance.AdvanceCode)
                    .ExecuteDeleteAsync(cancellationToken);

                var advanceOthers = new List<TblJobadvanceOther>();

                for (int i = 0; i < jobToFinish.valAdvance.Count(); i++)
                {
                    var name = jobToFinish.nameAdvance[i];

                    if (!string.IsNullOrWhiteSpace(name) &&
                        name != "-")
                    {
                        advanceOthers.Add(new TblJobadvanceOther
                        {
                            AdvanceOther = name.Trim(),
                            AdvanceOtherValue = jobToFinish.valWithdrawAdvance[i],
                            AdvanceOtherValueup = jobToFinish.valAdvance[i],
                            SupplierOtherValue = 0,
                            AdvanceCode = advance.AdvanceCode
                        });
                    }
                }

                if (advanceOthers.Count > 0)
                {
                    db.TblJobadvanceOthers.AddRange(advanceOthers);
                }
            }

            // =====================================================
            // Job detail
            // =====================================================
            itemDetail.CostValue = jobToFinish.cost_value;
            itemDetail.ContainerNo = NormalizeUpper(jobToFinish.container_no);
            itemDetail.SealNo = NormalizeUpper(jobToFinish.seal_no);
            itemDetail.TareWeight = jobToFinish.tare_weight;
            itemDetail.IsDocument = true;
            itemDetail.IsApprove = true;
            itemDetail.UpdateDate = DateTime.Now;
            itemDetail.UpdateBy = jobToFinish.username;

            supplierAllowance.CostReserve = jobToFinish.supplier_value;

            // =====================================================
            // Supplier rate
            // =====================================================
            if (jobToFinish.is_save == true)
            {
                var supplierRate = await db.TblSupplierRates
                    .FirstOrDefaultAsync(
                        w => w.SupplierCode == itemDetail.SupplierCode &&
                             w.PlaceCode == itemDetail.MissionPlaceCode,
                        cancellationToken);

                if (supplierRate != null)
                {
                    supplierRate.CostRate = jobToFinish.supplier_value;
                }
                else
                {
                    db.TblSupplierRates.Add(new TblSupplierRate
                    {
                        SupplierCode = itemDetail.SupplierCode,
                        PlaceCode = itemDetail.MissionPlaceCode,
                        CostRate = jobToFinish.supplier_value
                    });
                }
            }

            // =====================================================
            // Driver cost
            // =====================================================
            var fleetTypeCode = await db.TblSuppliers
                .AsNoTracking()
                .Where(w => w.SupplierCode == itemDetail.SupplierCode)
                .Select(w => w.FleetTypeCode)
                .FirstOrDefaultAsync(cancellationToken);

            if (fleetTypeCode == "F1" &&
                jobToFinish.driver_code != null)
            {
                var driverCosts = await db.TblDriverCosts
                    .Where(w => w.JobDetailCode == jobToFinish.job_detail_code)
                    .ToListAsync(cancellationToken);

                var driverCostMap = driverCosts
                    .GroupBy(w => (w.DriverCode, w.CostType))
                    .ToDictionary(g => g.Key, g => g.First());

                for (int i = 0; i < jobToFinish.driver_code.Count(); i++)
                {
                    var key = (
                        jobToFinish.driver_code[i],
                        jobToFinish.cost_type[i]);

                    if (!driverCostMap.TryGetValue(key, out var driver))
                    {
                        continue;
                    }

                    driver.TripValue = jobToFinish.driver_value[i];

                    driver.FuelValue = cashing.CashingFuel > 0
                        ? cashing.CashingFuel - cashing.CashingFuelup
                        : 0;

                    driver.ExpresswayValue =
                        cashing.CashingExpressway -
                        cashing.CashingExpresswayup;
                }
            }

            var newValues = GetScalarValues(itemDetail);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Confirm job data.",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblJobDetail),
                    EntityId = jobToFinish.job_detail_code,
                    Description = $"ยืนยันข้อมูล {jobToFinish.job_detail_code} โดย {jobToFinish.username}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                },
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            return Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await tr.RollbackAsync(CancellationToken.None);
            return Error(ex, nameof(ConfirmData));
        }
    }

    public async Task<List<ShippingPaymentViewModel>> GetClearingList(
        string month,
        string year,
        CancellationToken cancellationToken = default)
    {
        int m = Convert.ToInt32(month);
        int y = Convert.ToInt32(year);

        var startDate = new DateOnly(y, m, 1);
        var endDate = startDate.AddMonths(1);

        var data = await (
            from a in db.TblShippingpayments.AsNoTracking()
            join b in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals b.CustomerCode
            where a.ReceiveDate >= startDate &&
                  a.ReceiveDate < endDate &&
                  a.IsEnabled == true
            select new
            {
                a.PaymentCode,
                b.CustomerAcronym,
                a.ReceiveDate,
                a.Total,
                a.Amount
            })
            .ToListAsync(cancellationToken);

        return data.Select(a => new ShippingPaymentViewModel
        {
            payment_code = a.PaymentCode,
            customer_name = a.CustomerAcronym,
            payment_date = a.ReceiveDate.ToString("dd-MM-yyyy"),
            payment_total = a.Total,
            payment_amount = a.Amount
        }).ToList();
    }

    public async Task<List<JobViewModel>> GetAdvanceJobList(
        CancellationToken cancellationToken = default)
    {
        var data = await (
            from a in db.TblJobMasters.AsNoTracking()
            join b in db.TblJobDetails.AsNoTracking()
                on a.JobCode equals b.JobCode
            join c in db.TblShippingadvances.AsNoTracking()
                on b.JobDetailCode equals c.JobDetailCode
            join d in db.TblPlaces.AsNoTracking()
                on b.MissionPlaceCode equals d.PlaceCode
            join e in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals e.CustomerCode
            where b.IsEnabled == true && c.IsPaid == false
            select new
            {
                a.JobCode,
                a.JobDate,
                a.JobType,
                a.BookingNo,
                e.CustomerAcronym,
                a.ContainerType,
                a.ContainerSizeName,
                MissionName = d.PlaceName,
                b.JobDetailCode,
                b.ContainerNo,
                c.AdvanceValue
            })
            .ToListAsync(cancellationToken);

        return data.Select(a => new JobViewModel
        {
            job_code = a.JobCode,
            job_date = a.JobDate.ToString("yyyy-MM-dd"),
            job_date_value = a.JobDate,
            job_type = a.JobType,
            booking_no = a.BookingNo,
            customer_name = a.CustomerAcronym,
            container_type = a.ContainerType,
            container_size = a.ContainerSizeName,
            mission_name = a.MissionName,
            job_detail_code = a.JobDetailCode,
            container_no = a.ContainerNo,
            cost_value = a.AdvanceValue
        }).ToList();
    }

    public async Task<TblShippingpayment?> GetClearingDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await db.TblShippingpayments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.PaymentCode == code,
                cancellationToken);
    }

    public async Task<List<JobViewModel>> GetClearingDataListByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var data = await (
            from a in db.TblJobMasters.AsNoTracking()
            join b in db.TblJobDetails.AsNoTracking()
                on a.JobCode equals b.JobCode
            join c in db.TblShippingadvances.AsNoTracking()
                on b.JobDetailCode equals c.JobDetailCode
            join d in db.TblPlaces.AsNoTracking()
                on b.MissionPlaceCode equals d.PlaceCode
            join e in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals e.CustomerCode
            join f in db.TblShippingpaymentDetails.AsNoTracking()
                on b.JobDetailCode equals f.JobDetailCode
            where f.PaymentCode == code
            select new
            {
                a.JobCode,
                a.JobDate,
                a.JobType,
                a.BookingNo,
                e.CustomerAcronym,
                a.ContainerType,
                a.ContainerSizeName,
                MissionName = d.PlaceName,
                b.JobDetailCode,
                b.ContainerNo,
                c.AdvanceValue
            })
            .ToListAsync(cancellationToken);

        return data.Select(a => new JobViewModel
        {
            job_code = a.JobCode,
            job_date = a.JobDate.ToString("yyyy-MM-dd"),
            job_date_value = a.JobDate,
            job_type = a.JobType,
            booking_no = a.BookingNo,
            customer_name = a.CustomerAcronym,
            container_type = a.ContainerType,
            container_size = a.ContainerSizeName,
            mission_name = a.MissionName,
            job_detail_code = a.JobDetailCode,
            container_no = a.ContainerNo,
            cost_value = a.AdvanceValue
        }).ToList();
    }

    public async Task<ResponseViewModel> Clearing(
        TblShippingpayment model,
        string[] code,
        CancellationToken cancellationToken = default)
    {
        var inputCount = code?.Length ?? 0;
        var codes = NormalizeCodes(code);

        if (codes.Length == 0)
        {
            return Invalid("กรุณาเลือกรายการที่ต้องการเคลียร์");
        }

        if (codes.Length != inputCount)
        {
            return Invalid("พบรายการงานซ้ำ กรุณาตรวจสอบข้อมูลอีกครั้ง");
        }

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var jobs = await db.ViewJobListActives
                .AsNoTracking()
                .Where(w => codes.Contains(w.JobDetailCode))
                .Select(w => new
                {
                    w.JobDetailCode,
                    w.CustomerCode
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            if (jobs.Count != codes.Length)
            {
                await tr.RollbackAsync(cancellationToken);
                return NotFound("พบงานบางรายการที่ไม่มีอยู่ในระบบ");
            }

            var customerCodes = jobs
                .Select(w => w.CustomerCode)
                .Distinct()
                .ToList();

            if (customerCodes.Count != 1 ||
                string.IsNullOrWhiteSpace(customerCodes[0]))
            {
                await tr.RollbackAsync(cancellationToken);
                return Invalid("รายการที่เลือกต้องเป็นลูกค้ารายเดียวกัน");
            }

            var shippingAdvances = await db.TblShippingadvances
                .Where(w => codes.Contains(w.JobDetailCode))
                .ToListAsync(cancellationToken);

            var advanceMap = shippingAdvances
                .GroupBy(w => w.JobDetailCode)
                .ToDictionary(g => g.Key, g => g.First());

            if (advanceMap.Count != codes.Length)
            {
                await tr.RollbackAsync(cancellationToken);
                return NotFound("พบงานบางรายการที่ไม่มีข้อมูลเงินสำรอง");
            }

            foreach (var jobDetailCode in codes)
            {
                var advance = advanceMap[jobDetailCode];

                if (advance.IsPaid)
                {
                    await tr.RollbackAsync(cancellationToken);
                    return Invalid($"งาน {jobDetailCode} ถูกเคลียร์เงินแล้ว");
                }
            }

            model.PaymentCode = await codeRepo.GetShippingPaymentCode(cancellationToken);
            model.ReceiveDate = DateOnly.FromDateTime(DateTime.Now);
            model.CustomerCode = customerCodes[0];
            model.IsEnabled = true;

            var details = new List<TblShippingpaymentDetail>(codes.Length);
            decimal total = 0;

            foreach (var jobDetailCode in codes)
            {
                var advance = advanceMap[jobDetailCode];

                advance.IsPaid = true;
                total += advance.AdvanceValue;

                details.Add(new TblShippingpaymentDetail
                {
                    JobDetailCode = jobDetailCode,
                    PaymentCode = model.PaymentCode
                });
            }

            model.Amount = details.Count;
            model.Total = total;
            model.TblShippingpaymentDetails = details;

            db.TblShippingpayments.Add(model);

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            return Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await tr.RollbackAsync(CancellationToken.None);
            return Error(ex, nameof(Clearing));
        }
    }

    public async Task<ResponseViewModel> RemoveClearing(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await db.TblShippingpayments
                .FirstOrDefaultAsync(
                    w => w.PaymentCode == code,
                    cancellationToken);

            if (payment == null)
            {
                return NotFound("ไม่พบข้อมูลการเคลียร์เงิน");
            }

            var details = await db.TblShippingpaymentDetails
                .Where(w => w.PaymentCode == code)
                .ToListAsync(cancellationToken);

            if (details.Count > 0)
            {
                var jobDetailCodes = details
                    .Select(w => w.JobDetailCode)
                    .Distinct()
                    .ToList();

                var shippingAdvances = await db.TblShippingadvances
                    .Where(w => jobDetailCodes.Contains(w.JobDetailCode))
                    .ToListAsync(cancellationToken);

                foreach (var advance in shippingAdvances)
                {
                    advance.IsPaid = false;
                }

                db.TblShippingpaymentDetails.RemoveRange(details);
            }

            payment.IsEnabled = false;
            payment.UpdateDate = DateTime.Now;
            payment.UpdateBy = username;

            await db.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Error(ex, nameof(RemoveClearing));
        }
    }

    public async Task<ResponseViewModel> Grant(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var agent = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code,
                    cancellationToken);

            if (agent == null)
            {
                return NotFound("ไม่พบข้อมูลงาน");
            }

            var oldValues = GetScalarValues(agent);

            agent.IsAdvanceProve = false;
            agent.UpdateDate = DateTime.Now;
            agent.UpdateBy = username;

            var newValues = GetScalarValues(agent);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Grant to update advance.",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblJobDetail),
                    EntityId = code,
                    Description = $"ให้สิทธิ์การแก้ไขข้อมูล {code} โดย {username}",
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
            return Error(ex, nameof(Grant));
        }
    }

    public async Task<ResponseViewModel> EditClearing(
        TblShippingpayment model,
        string[] code,
        CancellationToken cancellationToken = default)
    {
        var inputCount = code?.Length ?? 0;
        var codes = NormalizeCodes(code);

        if (codes.Length == 0)
        {
            return Invalid("กรุณาเลือกรายการที่ต้องการเคลียร์");
        }

        if (codes.Length != inputCount)
        {
            return Invalid("พบรายการงานซ้ำ กรุณาตรวจสอบข้อมูลอีกครั้ง");
        }

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var currentPayment = await db.TblShippingpayments
                .FirstOrDefaultAsync(
                    w => w.PaymentCode == model.PaymentCode,
                    cancellationToken);

            if (currentPayment == null)
            {
                await tr.RollbackAsync(cancellationToken);
                return NotFound("ไม่พบข้อมูลการเคลียร์เงิน");
            }

            var oldDetails = await db.TblShippingpaymentDetails
                .Where(w => w.PaymentCode == model.PaymentCode)
                .ToListAsync(cancellationToken);

            var oldCodes = oldDetails
                .Select(w => w.JobDetailCode)
                .Distinct()
                .ToArray();

            var jobs = await db.ViewJobListActives
                .AsNoTracking()
                .Where(w => codes.Contains(w.JobDetailCode))
                .Select(w => new
                {
                    w.JobDetailCode,
                    w.CustomerCode
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            if (jobs.Count != codes.Length)
            {
                await tr.RollbackAsync(cancellationToken);
                return NotFound("พบงานบางรายการที่ไม่มีอยู่ในระบบ");
            }

            var customerCodes = jobs
                .Select(w => w.CustomerCode)
                .Distinct()
                .ToList();

            if (customerCodes.Count != 1 ||
                string.IsNullOrWhiteSpace(customerCodes[0]))
            {
                await tr.RollbackAsync(cancellationToken);
                return Invalid("รายการที่เลือกต้องเป็นลูกค้ารายเดียวกัน");
            }

            var allCodes = oldCodes
                .Concat(codes)
                .Distinct()
                .ToArray();

            var shippingAdvances = await db.TblShippingadvances
                .Where(w => allCodes.Contains(w.JobDetailCode))
                .ToListAsync(cancellationToken);

            var advanceMap = shippingAdvances
                .GroupBy(w => w.JobDetailCode)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var oldCode in oldCodes)
            {
                if (advanceMap.TryGetValue(oldCode, out var oldAdvance))
                {
                    oldAdvance.IsPaid = false;
                }
            }

            foreach (var jobDetailCode in codes)
            {
                if (!advanceMap.TryGetValue(jobDetailCode, out var advance))
                {
                    await tr.RollbackAsync(cancellationToken);
                    return NotFound($"ไม่พบข้อมูลเงินสำรองของงาน {jobDetailCode}");
                }

                // ถ้ารายการนี้ไม่ใช่ของใบเดิม แต่ถูกจ่ายอยู่ แสดงว่าถูกใช้กับใบอื่นแล้ว
                if (advance.IsPaid &&
                    !oldCodes.Contains(jobDetailCode))
                {
                    await tr.RollbackAsync(cancellationToken);
                    return Invalid($"งาน {jobDetailCode} ถูกเคลียร์เงินแล้ว");
                }

                advance.IsPaid = true;
            }

            db.TblShippingpaymentDetails.RemoveRange(oldDetails);

            var newDetails = codes
                .Select(jobDetailCode => new TblShippingpaymentDetail
                {
                    JobDetailCode = jobDetailCode,
                    PaymentCode = currentPayment.PaymentCode
                })
                .ToList();

            db.TblShippingpaymentDetails.AddRange(newDetails);

            decimal total = codes
                .Sum(jobDetailCode => advanceMap[jobDetailCode].AdvanceValue);

            var paymentCode = currentPayment.PaymentCode;
            var isEnabled = currentPayment.IsEnabled;

            db.Entry(currentPayment)
                .CurrentValues
                .SetValues(model);

            // ป้องกันค่าจาก form ทับ key / soft-delete state
            currentPayment.PaymentCode = paymentCode;
            currentPayment.IsEnabled = isEnabled;
            currentPayment.CustomerCode = customerCodes[0];
            currentPayment.Amount = newDetails.Count;
            currentPayment.Total = total;

            await db.SaveChangesAsync(cancellationToken);
            await tr.CommitAsync(cancellationToken);

            return Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await tr.RollbackAsync(CancellationToken.None);
            return Error(ex, nameof(EditClearing));
        }
    }


    private Dictionary<string, object?> GetScalarValues(object entity)
    {
        return db.Entry(entity)
            .Properties
            .ToDictionary(
                p => p.Metadata.Name,
                p => p.CurrentValue);
    }

    private static string NormalizeUpper(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "-"
            : value.Trim().ToUpperInvariant();
    }

    private static string[] NormalizeCodes(string[]? codes)
    {
        if (codes == null)
        {
            return Array.Empty<string>();
        }

        return codes
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => w.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void ValidateCashingInput(FinishJobModel model)
    {
        if (model.valCashing == null)
        {
            return;
        }

        int count = model.valCashing.Count();

        if (model.nameCashing == null ||
            model.valWithdrawCashing == null ||
            model.nameCashing.Count() != count ||
            model.valWithdrawCashing.Count() != count)
        {
            throw new InvalidOperationException(
                "ข้อมูลค่าใช้จ่าย Cashing ไม่ครบหรือจำนวนรายการไม่ตรงกัน");
        }
    }

    private static void ValidateAdvanceInput(FinishJobModel model)
    {
        if (model.valAdvance == null)
        {
            return;
        }

        int count = model.valAdvance.Count();

        if (model.nameAdvance == null ||
            model.valWithdrawAdvance == null ||
            model.nameAdvance.Count() != count ||
            model.valWithdrawAdvance.Count() != count)
        {
            throw new InvalidOperationException(
                "ข้อมูลเงินสำรอง Advance ไม่ครบหรือจำนวนรายการไม่ตรงกัน");
        }
    }

    private static void ValidateDriverInput(FinishJobModel model)
    {
        if (model.driver_code == null)
        {
            return;
        }

        int count = model.driver_code.Count();

        if (model.cost_type == null ||
            model.driver_value == null ||
            model.cost_type.Count() != count ||
            model.driver_value.Count() != count)
        {
            throw new InvalidOperationException(
                "ข้อมูลค่าเที่ยวคนขับไม่ครบหรือจำนวนรายการไม่ตรงกัน");
        }
    }

    private static ResponseViewModel Success()
    {
        return new ResponseViewModel
        {
            result = true,
            resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว"
        };
    }

    private static ResponseViewModel NotFound(string message)
    {
        return new ResponseViewModel
        {
            result = false,
            resultMessage = message
        };
    }

    private static ResponseViewModel Invalid(string message)
    {
        return new ResponseViewModel
        {
            result = false,
            resultMessage = message
        };
    }

    private ResponseViewModel Error(Exception ex, string operation)
    {
        logger.LogError(
            ex,
            "ApproveRepository error in {Operation}",
            operation);

        return new ResponseViewModel
        {
            result = false,
            resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>"
        };
    }

}
