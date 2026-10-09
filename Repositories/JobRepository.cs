using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class JobRepository
{
    private const string InternalSupplierCode = "SP18001";
    private const string DefaultDriverCode = "DV000000";
    private const string DefaultTruckCode = "T000000";
    private const string EmptyPlaceCode = "000000000";
    private const string TransportBudgetCode = "01";

    private readonly DbavalogContext db;
    private readonly CodeRepository codeRepo;
    private readonly IAuditLogService auditLogService;
    private readonly ILogger<JobRepository> logger;

    public JobRepository(
        DbavalogContext context,
        CodeRepository _codeRepo,
        IAuditLogService _auditLogService,
        ILogger<JobRepository> _logger)
    {
        db = context;
        codeRepo = _codeRepo;
        auditLogService = _auditLogService;
        logger = _logger;
    }

    // =====================================================
    // Remove Job
    // =====================================================
    public async Task<ResponseViewModel> RemoveJob(
        string code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัสงาน");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var now = DateTime.Now;

            var detail = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code,
                    cancellationToken);

            if (detail == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูลงาน");
            }

            if (detail.IsEnabled == false)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("งานนี้ถูกลบไปแล้ว");
            }

            var oldValues = GetScalarSnapshot(detail);

            detail.IsEnabled = false;
            detail.UpdateDate = now;
            detail.UpdateBy = username;

            var job = await db.TblJobMasters
                .FirstOrDefaultAsync(
                    w => w.JobCode == detail.JobCode,
                    cancellationToken);

            if (job != null)
            {
                job.LiftedAmount = job.LiftedAmount - 1;
            }

            var advance = await db.TblJobadvances
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code,
                    cancellationToken);

            if (advance != null)
            {
                await db.TblJobadvanceOthers
                    .Where(w => w.AdvanceCode == advance.AdvanceCode)
                    .ExecuteDeleteAsync(cancellationToken);

                await db.TblJobadvances
                    .Where(w => w.AdvanceCode == advance.AdvanceCode)
                    .ExecuteDeleteAsync(cancellationToken);
            }

            var cashing = await db.TblJobcashings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code,
                    cancellationToken);

            if (cashing != null)
            {
                await db.TblJobcashingOthers
                    .Where(w => w.CashingCode == cashing.CashingCode)
                    .ExecuteDeleteAsync(cancellationToken);

                await db.TblJobcashings
                    .Where(w => w.CashingCode == cashing.CashingCode)
                    .ExecuteDeleteAsync(cancellationToken);
            }

            await db.TblJobImgs
                .Where(w => w.JobDetailCode == code)
                .ExecuteDeleteAsync(cancellationToken);

            await db.TblSupplierAllowances
                .Where(w => w.JobDetailCode == code)
                .ExecuteDeleteAsync(cancellationToken);

            /*
             * LogBudgetTransaction เป็น business ledger
             * ไม่ใช่ AuditLog เก่า จึงยังคงไว้เพื่อใช้ย้อนคืน Budget
             */
            var transactionCodes = await db.LogBudgetTransactions
                .AsNoTracking()
                .Where(w => w.JobDetailCode == code)
                .Select(w => w.TransactionCode)
                .Distinct()
                .ToListAsync(cancellationToken);

            decimal amount = 0;

            if (transactionCodes.Count > 0)
            {
                var budgetTransactions = await db.TblBudgetTransactions
                    .Where(w => transactionCodes.Contains(w.TransactionCode))
                    .OrderBy(w => w.TransactionDate)
                    .ToListAsync(cancellationToken);

                /*
                 * UpdateAdvanceData เก็บ Amount เป็น "ยอดจ่ายล่าสุด"
                 * ทุกครั้ง ดังนั้นตอนลบงานต้องคืนยอดจาก transaction ล่าสุด
                 * ไม่ใช่คำนวณจาก Amount ซ้ำทุก transaction
                 *
                 * ตัวอย่าง: จ่าย 100 แล้วแก้เป็น 120
                 * Budget ถูกหักรวม 120 และต้องคืน 120
                 */
                var latestBudgetTransaction =
                    budgetTransactions
                        .OrderByDescending(w => w.TransactionDate)
                        .ThenByDescending(w => w.CreateDate)
                        .FirstOrDefault();

                if (latestBudgetTransaction != null)
                {
                    amount =
                        latestBudgetTransaction.Amount;
                }

                foreach (var budgetTransaction in budgetTransactions)
                {
                    budgetTransaction.IsEnabled = false;
                    budgetTransaction.UpdateDate = now;
                    budgetTransaction.UpdateBy = username;
                }
            }

            var budget = await db.TblBudgets
                .FirstOrDefaultAsync(
                    w => w.BudgetType == "A",
                    cancellationToken);

            if (budget != null)
            {
                budget.Balance += amount;
            }

            var binding = await db.TblBindingGenerates
                .FindAsync(
                    new object?[]
                    {
                        detail.JobCode,
                        code
                    },
                    cancellationToken);

            if (binding != null)
            {
                binding.IsEnabled = false;
            }

            await db.TblDriverCosts
                .Where(w => w.JobDetailCode == code)
                .ExecuteDeleteAsync(cancellationToken);

            await db.TblJobTrackings
                .Where(w => w.JobDetailCode == code)
                .ExecuteDeleteAsync(cancellationToken);

            var newValues = GetScalarSnapshot(detail);

            await AddAuditAsync(
                AuditActions.Delete,
                nameof(TblJobDetail),
                code,
                $"ลบงาน {code} โดย {username}",
                oldValues,
                newValues,
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

            return Error(ex, nameof(RemoveJob), code);
        }
    }

    // =====================================================
    // Edit Job
    // =====================================================
    public async Task<ResponseViewModel> EditJob(
        TblJobDetail jobToEdit,
        CancellationToken cancellationToken = default)
    {
        if (jobToEdit == null ||
            string.IsNullOrWhiteSpace(jobToEdit.JobDetailCode))
        {
            return Failure("ไม่พบข้อมูลงาน");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var now = DateTime.Now;

            var jobDetail = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == jobToEdit.JobDetailCode,
                    cancellationToken);

            if (jobDetail == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูลงาน");
            }

            var oldValues = GetScalarSnapshot(jobDetail);

            var change = new ChangeDriverModel
            {
                job_detail_code = jobDetail.JobDetailCode,
                mission_code = jobDetail.MissionPlaceCode,
                supplier_code = jobDetail.SupplierCode,
                driver_code = jobDetail.DriverCode,
                truck_code = jobDetail.TruckCode,
                new_supplier_code = jobToEdit.SupplierCode,
                new_driver_code = jobToEdit.DriverCode,
                new_truck_code = jobToEdit.TruckCode
            };

            var truck = await db.TblTruckDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.TruckCode == jobToEdit.TruckCode,
                    cancellationToken);

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == jobToEdit.JobDetailCode,
                    cancellationToken);

            var truckType =
                truck?.TruckTypegroupCode;

            if (truck != null &&
                jobDetail.IsAdvancePaid == false)
            {
                if (cashing == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Failure("ไม่พบข้อมูลค่าใช้จ่ายของงาน");
                }

                var fuel = await GetFuelRateAsync(
                    jobToEdit.MissionPlaceCode,
                    truck.TruckTypegroupCode,
                    cancellationToken);

                cashing.CashingFuel = fuel;

                /*
                 * รักษาสูตรเดิมจากต้นฉบับ
                 */
                cashing.CashingTotal =
                    cashing.CashingExpressway +
                    cashing.CashingWaste +
                    cashing.CashingOther;

                change.fuel = fuel;
            }

            await EditDriverDataAsync(
                change,
                cancellationToken);

            await db.TblSupplierAllowances
                .Where(w =>
                    w.SupplierCode == change.supplier_code &&
                    w.JobDetailCode == jobToEdit.JobDetailCode)
                .ExecuteDeleteAsync(cancellationToken);

            var supplierTrip =
                await GetSupplierCostAsync(
                    jobToEdit.SupplierCode,
                    truckType,
                    jobToEdit.MissionPlaceCode,
                    cancellationToken);

            db.TblSupplierAllowances.Add(
                new TblSupplierAllowance
                {
                    SupplierCode = jobToEdit.SupplierCode,
                    JobDetailCode = jobToEdit.JobDetailCode,
                    CostValue = supplierTrip
                });

            jobToEdit.DriverCode =
                string.IsNullOrWhiteSpace(jobToEdit.DriverCode)
                    ? DefaultDriverCode
                    : jobToEdit.DriverCode;

            jobToEdit.TruckCode =
                string.IsNullOrWhiteSpace(jobToEdit.TruckCode)
                    ? DefaultTruckCode
                    : jobToEdit.TruckCode;

            CopyEditableJobValues(
                jobDetail,
                jobToEdit);

            jobDetail.UpdateDate = now;
            jobDetail.UpdateBy = jobToEdit.UpdateBy;

            var newValues = GetScalarSnapshot(jobDetail);

            await AddAuditAsync(
                AuditActions.Update,
                nameof(TblJobDetail),
                jobDetail.JobDetailCode,
                $"แก้ไขงาน {jobDetail.JobDetailCode} โดย {jobDetail.UpdateBy}",
                oldValues,
                newValues,
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
                nameof(EditJob),
                jobToEdit.JobDetailCode);
        }
    }

    // =====================================================
    // Read Job Detail
    // =====================================================
    public async Task<TblJobDetail?> GetDetailDataByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblJobDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.JobDetailCode == code,
                cancellationToken);
    }

    // =====================================================
    // Job List
    // =====================================================
    public async Task<List<JobViewModel>> GetJobDataList(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        if (end < start)
        {
            return new List<JobViewModel>();
        }

        var rawData = await (
            from a in db.TblJobMasters.AsNoTracking()
            join b in db.TblJobDetails.AsNoTracking()
                on a.JobCode equals b.JobCode
            join c in db.TblTrucks.AsNoTracking()
                on b.TruckCode equals c.TruckCode
            join d in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals d.CustomerCode
            join e in db.TblPlaces.AsNoTracking()
                on b.MissionPlaceCode equals e.PlaceCode
            join f in db.TblSuppliers.AsNoTracking()
                on b.SupplierCode equals f.SupplierCode
            join i in db.TblDrivers.AsNoTracking()
                on b.DriverCode equals i.DriverCode
            join g in db.TblDriverCosts.AsNoTracking()
                on b.JobDetailCode equals g.JobDetailCode into gj
            from g in gj.DefaultIfEmpty()
            where b.IsDocument == false
                  && b.IsCanceled == false
                  && b.JobDetailCode != null
                  && a.JobDate >= start
                  && a.JobDate <= end
                  && a.IsEnabled == true
                  && b.IsEnabled == true
                  && (g.CostType == null || g.CostType == "A")
            select new
            {
                a.JobCode,
                b.JobDetailCode,
                a.JobDate,
                a.JobType,
                d.CustomerAcronym,
                e.PlaceName,
                a.BookingNo,
                b.ContainerNo,
                a.ContainerSizeName,
                c.TruckLicense,
                f.SupplierName,
                b.TareWeight,
                b.IsDocument,
                b.IsBilling,
                b.TrackingStatus,
                b.IsAdvancePaid,
                b.IsAdvanceProve,
                IsWeeklypayment =
                    g != null
                        ? g.IsWeeklypayment
                        : false,
                i.DriverFullname,
                i.DriverPhone,
                a.JobRemark
            })
            .ToListAsync(cancellationToken);

        return rawData
            .Select(a => new JobViewModel
            {
                job_code = a.JobCode,
                job_detail_code = a.JobDetailCode,
                job_date =
                    a.JobDate.ToString("yyyy-MM-dd HH:mm"),
                job_date_value = a.JobDate,
                job_type = a.JobType,
                customer_name = a.CustomerAcronym,
                mission_name = a.PlaceName,
                booking_no = a.BookingNo,
                container_no = a.ContainerNo,
                container_size = a.ContainerSizeName,
                truck_license = a.TruckLicense,
                supplier_name = a.SupplierName,
                tare_weight = a.TareWeight,
                is_document = a.IsDocument,
                is_billing = a.IsBilling,
                tracking_status = a.TrackingStatus,
                is_advance_paid = a.IsAdvancePaid,
                is_advance_prove = a.IsAdvanceProve,
                is_weeklypayment = a.IsWeeklypayment,
                driver_fullname = a.DriverFullname,
                driver_phone = a.DriverPhone,
                job_remark = a.JobRemark
            })
            .ToList();
    }

    public async Task<TblJobadvance?> GetAdvanceByJob(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblJobadvances
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w =>
                    w.JobDetailCode == code &&
                    w.IsEnabled == true,
                cancellationToken);
    }

    public async Task<bool> IsPayment(
        string job_detail_code,
        string driver_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(job_detail_code) ||
            string.IsNullOrWhiteSpace(driver_code))
        {
            return false;
        }

        var data = await db.TblDriverCosts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w =>
                    w.JobDetailCode == job_detail_code &&
                    w.DriverCode == driver_code,
                cancellationToken);

        return data != null &&
               data.IsWeeklypayment != true;
    }

    public async Task<List<OtherJobViewModel>> GetAdvanceOtherByJob(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new List<OtherJobViewModel>();
        }

        return await db.TblJobadvanceOthers
            .AsNoTracking()
            .Where(a => a.AdvanceCode == code)
            .Select(a => new OtherJobViewModel
            {
                name = a.AdvanceOther,
                value = a.AdvanceOtherValue,
                supplier_value = a.SupplierOtherValue,
                value_up = a.AdvanceOtherValueup
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TblJobcashing?> GetCashingByJob(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await db.TblJobcashings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w =>
                    w.JobDetailCode == code &&
                    w.IsEnabled == true,
                cancellationToken);
    }

    public async Task<List<OtherJobViewModel>> GetCashingOtherByJob(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new List<OtherJobViewModel>();
        }

        return await db.TblJobcashingOthers
            .AsNoTracking()
            .Where(a => a.CashingCode == code)
            .Select(a => new OtherJobViewModel
            {
                name = a.CashingOther,
                value = a.CashingOtherValue,
                value_up = a.CashingOtherValueup
            })
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Finish Job
    // =====================================================
    public async Task<ResponseViewModel> FinishJob(
        FinishJobModel jobToFinish,
        TblJobadvance jobAdvance,
        TblJobcashing jobCashing,
        CancellationToken cancellationToken = default)
    {
        if (jobToFinish == null ||
            jobAdvance == null ||
            jobCashing == null ||
            string.IsNullOrWhiteSpace(jobToFinish.job_detail_code))
        {
            return Failure("ข้อมูลปิดงานไม่ครบถ้วน");
        }

        var arrayValidation =
            ValidateFinishArrays(jobToFinish);

        if (arrayValidation != null)
        {
            return arrayValidation;
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var now = DateTime.Now;

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         jobToFinish.job_detail_code,
                    cancellationToken);

            var advance = await db.TblJobadvances
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         jobToFinish.job_detail_code,
                    cancellationToken);

            var itemDetail = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         jobToFinish.job_detail_code,
                    cancellationToken);

            if (cashing == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูล Cashing ของงาน");
            }

            if (advance == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูล Advance ของงาน");
            }

            if (itemDetail == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูลงาน");
            }

            var oldValues = GetScalarSnapshot(itemDetail);

            cashing.CashingFuel =
                jobCashing.CashingFuel;

            cashing.CashingFuelup =
                jobCashing.CashingFuelup;

            cashing.CashingExpressway =
                jobCashing.CashingExpressway;

            cashing.CashingExpresswayup =
                jobCashing.CashingExpresswayup;

            /*
             * รักษาสูตรเดิมจากต้นฉบับ
             */
            cashing.CashingTotal =
                jobCashing.CashingExpresswayup +
                jobCashing.CashingWaste;

            await db.TblJobcashingOthers
                .Where(w => w.CashingCode == cashing.CashingCode)
                .ExecuteDeleteAsync(cancellationToken);

            if (jobToFinish.valCashing != null)
            {
                for (var i = 0;
                     i < jobToFinish.valCashing.Count();
                     i++)
                {
                    if (string.IsNullOrWhiteSpace(
                            jobToFinish.nameCashing[i]) ||
                        jobToFinish.valWithdrawCashing[i] == 0)
                    {
                        continue;
                    }

                    db.TblJobcashingOthers.Add(
                        new TblJobcashingOther
                        {
                            CashingOther =
                                jobToFinish.nameCashing[i],

                            CashingOtherValue =
                                jobToFinish.valWithdrawCashing[i],

                            CashingOtherValueup =
                                jobToFinish.valCashing[i],

                            CashingCode =
                                cashing.CashingCode
                        });
                }
            }

            advance.AdvanceLiftoffup =
                jobAdvance.AdvanceLiftoffup;

            advance.AdvanceLiftonup =
                jobAdvance.AdvanceLiftonup;

            advance.AdvancePortup =
                jobAdvance.AdvancePortup;

            advance.AdvanceYardup =
                jobAdvance.AdvanceYardup;

            advance.AdvanceTotalup =
                jobAdvance.AdvanceLiftoffup +
                jobAdvance.AdvanceLiftonup +
                jobAdvance.AdvancePortup +
                jobAdvance.AdvanceYardup;

            await db.TblJobadvanceOthers
                .Where(w => w.AdvanceCode == advance.AdvanceCode)
                .ExecuteDeleteAsync(cancellationToken);

            if (jobToFinish.valAdvance != null)
            {
                for (var i = 0;
                     i < jobToFinish.valAdvance.Count();
                     i++)
                {
                    if (string.IsNullOrWhiteSpace(
                            jobToFinish.nameAdvance[i]) ||
                        jobToFinish.valWithdrawAdvance[i] == 0)
                    {
                        continue;
                    }

                    db.TblJobadvanceOthers.Add(
                        new TblJobadvanceOther
                        {
                            AdvanceOther =
                                jobToFinish.nameAdvance[i],

                            AdvanceOtherValue =
                                jobToFinish.valWithdrawAdvance[i],

                            AdvanceOtherValueup =
                                jobToFinish.valAdvance[i],

                            SupplierOtherValue = 0,
                            AdvanceCode = advance.AdvanceCode
                        });
                }
            }

            itemDetail.TrackingStatus = "BO";

            itemDetail.ContainerNo =
                NormalizeUpper(jobToFinish.container_no);

            itemDetail.SealNo =
                NormalizeUpper(jobToFinish.seal_no);

            itemDetail.TareWeight =
                jobToFinish.tare_weight;

            itemDetail.SupplierInvoice =
                itemDetail.SupplierCode != InternalSupplierCode
                    ? NormalizeText(jobToFinish.supplier_invoice)
                    : "-";

            itemDetail.IsDocument = true;
            itemDetail.UpdateDate = now;
            itemDetail.UpdateBy = jobToFinish.username;

            if (itemDetail.SupplierCode ==
                InternalSupplierCode)
            {
                var truck = await db.TblTruckDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        w => w.TruckCode ==
                             itemDetail.TruckCode,
                        cancellationToken);

                if (truck == null)
                {
                    await transaction.RollbackAsync(
                        cancellationToken);

                    return Failure(
                        "ไม่พบข้อมูลประเภทรถสำหรับคำนวณค่าใช้จ่ายคนขับ");
                }

                var driver = await db.TblDriverCosts
                    .FirstOrDefaultAsync(
                        w => w.JobDetailCode ==
                             jobToFinish.job_detail_code,
                        cancellationToken);

                if (driver != null)
                {
                    driver.FuelValue =
                        truck.TruckTypegroupCode == "02"
                            ? (cashing.CashingFuel -
                               cashing.CashingFuelup) * 20
                            : 0;

                    driver.ExpresswayValue =
                        cashing.CashingExpressway -
                        cashing.CashingExpresswayup;
                }
                else
                {
                    var master = await db.TblJobMasters
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            w => w.JobCode ==
                                 itemDetail.JobCode,
                            cancellationToken);

                    if (master == null)
                    {
                        await transaction.RollbackAsync(
                            cancellationToken);

                        return Failure(
                            "ไม่พบข้อมูล Job Master");
                    }

                    var fuel = await GetFuelRateAsync(
                        itemDetail.MissionPlaceCode,
                        master.TruckTypegroupCode,
                        cancellationToken);

                    var jobPlace = await db.TblJobPlaces
                        .FirstOrDefaultAsync(
                            w => w.JobCode ==
                                 master.JobCode,
                            cancellationToken);

                    if (jobPlace != null)
                    {
                        jobPlace.FuelValue = fuel;
                    }

                    var wage =
                        await GetDriverCostAsync(
                            itemDetail.MissionPlaceCode,
                            cancellationToken);

                    db.TblDriverCosts.Add(
                        new TblDriverCost
                        {
                            JobDetailCode =
                                itemDetail.JobDetailCode,

                            DriverCode =
                                itemDetail.DriverCode,

                            TripValue = wage,

                            FuelValue =
                                truck.TruckTypegroupCode == "02"
                                    ? (cashing.CashingFuel -
                                       cashing.CashingFuelup) * 20
                                    : 0,

                            ExpresswayValue =
                                cashing.CashingExpressway -
                                cashing.CashingExpresswayup,

                            CostType = "A"
                        });
                }
            }

            if (jobToFinish.shipping_advance > 0)
            {
                var shippingAdvance =
                    await db.TblShippingadvances
                        .FirstOrDefaultAsync(
                            w => w.JobDetailCode ==
                                 jobToFinish.job_detail_code,
                            cancellationToken);

                if (shippingAdvance != null)
                {
                    shippingAdvance.AdvanceValue =
                        jobToFinish.shipping_advance;
                }
                else
                {
                    db.TblShippingadvances.Add(
                        new TblShippingadvance
                        {
                            JobDetailCode =
                                jobToFinish.job_detail_code,

                            AdvanceValue =
                                jobToFinish.shipping_advance,

                            IsPaid = false
                        });
                }
            }

            db.TblJobTrackings.Add(
                new TblJobTracking
                {
                    JobDetailCode =
                        jobToFinish.job_detail_code,

                    TrackingStatus = "BO",
                    TrackingDate = now,
                    Lattitude = "-",
                    Longitude = "-",
                    Speed = "0"
                });

            var supplierCost =
                await db.TblSupplierAllowances
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        w => w.JobDetailCode ==
                             itemDetail.JobDetailCode,
                        cancellationToken);

            db.LogCosts.Add(
                new LogCost
                {
                    JobDetailCode =
                        itemDetail.JobDetailCode,

                    CustomerCost =
                        itemDetail.CostValue,

                    SupplierCost =
                        supplierCost?.CostValue ?? 0,

                    CreateDate = now,
                    CreateBy = jobToFinish.username
                });

            var newValues = GetScalarSnapshot(itemDetail);

            await AddAuditAsync(
                AuditActions.Update,
                nameof(TblJobDetail),
                jobToFinish.job_detail_code,
                $"ปิดงาน {jobToFinish.job_detail_code} โดย {jobToFinish.username}",
                oldValues,
                newValues,
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
                nameof(FinishJob),
                jobToFinish.job_detail_code);
        }
    }

    // =====================================================
    // Remove Image
    // =====================================================
    public async Task<ResponseViewModel> RemoveImage(
        int id,
        string username,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await db.TblJobImgs
                .FirstOrDefaultAsync(
                    w => w.Id == id,
                    cancellationToken);

            if (data == null)
            {
                return Failure("ไม่พบรูปภาพ");
            }

            var oldValues = GetScalarSnapshot(data);
            var jobDetailCode = data.JobDetailCode;

            db.TblJobImgs.Remove(data);

            await AddAuditAsync(
                AuditActions.Delete,
                nameof(TblJobImg),
                id.ToString(),
                $"ลบรูปภาพงาน {jobDetailCode} โดย {username}",
                oldValues,
                null,
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return Success("จัดการข้อมูลเรียบร้อยแล้ว");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                nameof(RemoveImage),
                id.ToString());
        }
    }

    // =====================================================
    // Reverse Tracking Status
    // =====================================================
    public async Task<ResponseViewModel> ReverseStatus(
        string code,
        string status,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(status))
        {
            return Failure("ข้อมูลสถานะไม่ครบถ้วน");
        }

        try
        {
            var job = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code,
                    cancellationToken);

            if (job == null)
            {
                return Failure("ไม่พบข้อมูลงาน");
            }

            var oldValues = GetScalarSnapshot(job);

            string trackingStatusToRemove;
            string newStatus;

            switch (status.Trim().ToUpperInvariant())
            {
                case "BO":
                    trackingStatusToRemove = "DO";
                    newStatus = "AR";
                    break;

                case "DO":
                    trackingStatusToRemove = "AR";
                    newStatus = "WP";
                    break;

                case "AR":
                    trackingStatusToRemove = "WP";
                    newStatus = "ST";
                    break;

                default:
                    return Failure(
                        $"ไม่รองรับการย้อนสถานะ {status}");
            }

            var tracking = await db.TblJobTrackings
                .FirstOrDefaultAsync(
                    w =>
                        w.JobDetailCode == code &&
                        w.TrackingStatus ==
                            trackingStatusToRemove,
                    cancellationToken);

            if (tracking != null)
            {
                db.TblJobTrackings.Remove(tracking);
            }

            job.TrackingStatus = newStatus;

            var newValues = GetScalarSnapshot(job);

            await AddAuditAsync(
                AuditActions.Update,
                nameof(TblJobDetail),
                code,
                $"ย้อนสถานะงาน {code} จาก {status} เป็น {newStatus}",
                oldValues,
                newValues,
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
                nameof(ReverseStatus),
                code);
        }
    }

    public async Task<List<TrackingModel>> GetListTrackingByJob(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new List<TrackingModel>();
        }

        return await db.TblJobTrackings
            .AsNoTracking()
            .Where(a => a.JobDetailCode == code)
            .OrderBy(a => a.TrackingDate)
            .Select(a => new TrackingModel
            {
                job_detail_code = a.JobDetailCode,
                tracking_status = a.TrackingStatus,
                tracking_date = a.TrackingDate
            })
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Update Advance Payment
    // =====================================================
    public async Task<ResponseViewModel> UpdateAdvanceData(
        FinishJobModel model,
        TblJobadvance jobAdvance,
        TblJobcashing jobCashing,
        CancellationToken cancellationToken = default)
    {
        if (model == null ||
            jobAdvance == null ||
            jobCashing == null ||
            string.IsNullOrWhiteSpace(model.job_detail_code))
        {
            return Failure("ข้อมูลการจ่ายเงินสำรองไม่ครบถ้วน");
        }

        var arrayValidation =
            ValidateAdvanceArrays(model);

        if (arrayValidation != null)
        {
            return arrayValidation;
        }

        var notificationMessage = string.Empty;

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var now = DateTime.Now;

            /*
             * ใช้ CodeRepository ที่ Inject มาแทน new CodeRepository(db)
             * เพื่อไม่สร้าง dependency ภายใน method
             */
            var transactionCode = await codeRepo.GetBudgetTransactionCode();

            if (string.IsNullOrWhiteSpace(transactionCode))
            {
                await transaction.RollbackAsync(cancellationToken);

                return Failure(
                    "ไม่สามารถสร้างเลขที่รายการ Budget ได้");
            }

            var order = await db.ViewJobListActives
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            var advance = await db.TblJobadvances
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            var jobDetail = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            var budget = await db.TblBudgets
                .FirstOrDefaultAsync(
                    w => w.BudgetCode ==
                         TransportBudgetCode,
                    cancellationToken);

            if (order == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูลงาน");
            }

            if (cashing == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูล Cashing");
            }

            if (advance == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูล Advance");
            }

            if (jobDetail == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบ Job Detail");
            }

            if (budget == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูลงบประมาณ");
            }

            var oldValues = new
            {
                Job = GetScalarSnapshot(jobDetail),
                Cashing = GetScalarSnapshot(cashing),
                Advance = GetScalarSnapshot(advance),
                Budget = GetScalarSnapshot(budget)
            };

            decimal total = 0;
            decimal totalOtherCashing = 0;

            cashing.CashingFuel =
                jobCashing.CashingFuel;

            cashing.CashingExpressway =
                jobCashing.CashingExpressway;

            total += jobCashing.CashingExpressway;

            AddBudgetTransactionDetail(
                model.job_detail_code,
                transactionCode,
                "ค่าทางด่วน",
                "C",
                jobCashing.CashingExpressway,
                model.username);

            await db.TblJobcashingOthers
                .Where(w => w.CashingCode == cashing.CashingCode)
                .ExecuteDeleteAsync(cancellationToken);

            if (model.valCashing != null)
            {
                for (var i = 0;
                     i < model.valCashing.Count();
                     i++)
                {
                    if (string.IsNullOrWhiteSpace(
                            model.nameCashing[i]) ||
                        model.valCashing[i] == 0)
                    {
                        continue;
                    }

                    totalOtherCashing +=
                        model.valCashing[i];

                    db.TblJobcashingOthers.Add(
                        new TblJobcashingOther
                        {
                            CashingOther =
                                model.nameCashing[i],

                            CashingOtherValue =
                                model.valCashing[i],

                            CashingOtherValueup = 0,

                            CashingCode =
                                cashing.CashingCode
                        });

                    AddBudgetTransactionDetail(
                        model.job_detail_code,
                        transactionCode,
                        model.nameCashing[i],
                        "C",
                        model.valCashing[i],
                        model.username);
                }
            }

            total += totalOtherCashing;

            advance.AdvanceLiftoff =
                jobAdvance.AdvanceLiftoff;

            advance.AdvanceLifton =
                jobAdvance.AdvanceLifton;

            advance.AdvancePort =
                jobAdvance.AdvancePort;

            advance.AdvanceYard =
                jobAdvance.AdvanceYard;

            advance.AdvanceTotal =
                jobAdvance.AdvanceLiftoff +
                jobAdvance.AdvanceLifton +
                jobAdvance.AdvancePort +
                jobAdvance.AdvanceYard;

            total += advance.AdvanceTotal;

            if (advance.AdvanceLiftoff > 0)
            {
                AddBudgetTransactionDetail(
                    model.job_detail_code,
                    transactionCode,
                    "ยกตู้หนัก",
                    "A",
                    jobAdvance.AdvanceLiftoff,
                    model.username);
            }

            if (advance.AdvanceLifton > 0)
            {
                AddBudgetTransactionDetail(
                    model.job_detail_code,
                    transactionCode,
                    "ยกตู้เปล่า",
                    "A",
                    jobAdvance.AdvanceLifton,
                    model.username);
            }

            if (advance.AdvanceYard > 0)
            {
                AddBudgetTransactionDetail(
                    model.job_detail_code,
                    transactionCode,
                    "ค่าผ่านลาน",
                    "A",
                    jobAdvance.AdvanceYard,
                    model.username);
            }

            if (advance.AdvancePort > 0)
            {
                AddBudgetTransactionDetail(
                    model.job_detail_code,
                    transactionCode,
                    "ค่าผ่านท่า",
                    "A",
                    jobAdvance.AdvancePort,
                    model.username);
            }

            await db.TblJobadvanceOthers
                .Where(w => w.AdvanceCode == advance.AdvanceCode)
                .ExecuteDeleteAsync(cancellationToken);

            decimal totalOtherAdvance = 0;

            if (model.valAdvance != null)
            {
                for (var i = 0;
                     i < model.valAdvance.Count();
                     i++)
                {
                    if (string.IsNullOrWhiteSpace(
                            model.nameAdvance[i]) ||
                        model.valAdvance[i] == 0)
                    {
                        continue;
                    }

                    totalOtherAdvance +=
                        model.valAdvance[i];

                    db.TblJobadvanceOthers.Add(
                        new TblJobadvanceOther
                        {
                            AdvanceOther =
                                model.nameAdvance[i],

                            AdvanceOtherValue =
                                model.valAdvance[i],

                            AdvanceOtherValueup = 0,
                            SupplierOtherValue = 0,
                            AdvanceCode = advance.AdvanceCode
                        });

                    AddBudgetTransactionDetail(
                        model.job_detail_code,
                        transactionCode,
                        model.nameAdvance[i],
                        "A",
                        model.valAdvance[i],
                        model.username);
                }
            }

            total += totalOtherAdvance;

            var jobBudget = await (
                from a in db.TblBudgetTransactions
                join b in db.TblJobBudgetTransactions
                    on a.TransactionCode equals b.TransactionCode
                where b.JobDetailCode ==
                      model.job_detail_code
                orderby a.TransactionDate descending
                select a)
                .FirstOrDefaultAsync(cancellationToken);

            if (jobBudget == null)
            {
                db.TblBudgetTransactions.Add(
                    new TblBudgetTransaction
                    {
                        TransactionCode =
                            transactionCode,

                        TransactionType = "JOB",
                        BudgetCode = TransportBudgetCode,
                        TransactionDate = now,
                        Balance = budget.Balance,
                        Amount = total,
                        AmountDiff = 0,
                        PaymentType = "MIN",
                        IsEnabled = true,
                        CreateDate = now,
                        CreateBy = model.username,
                        UpdateDate = now,
                        UpdateBy = model.username
                    });

                budget.Balance -= total;

                db.TblJobBudgetTransactions.Add(
                    new TblJobBudgetTransaction
                    {
                        JobDetailCode =
                            model.job_detail_code,

                        TransactionCode =
                            transactionCode
                    });

                var missionName =
                    (order.MissionName ?? string.Empty)
                        .Replace("&", " ");

                var startName =
                    order.StartName ?? string.Empty;

                notificationMessage =
                    "จ่ายเงินสำรองจำนวน : " +
                    total.ToString("#,##0.00") + " บ.\n" +
                    "งานเลขที่ : " +
                    model.job_detail_code + "\n" +
                    "ลูกค้า : " +
                    order.CustomerAcronym + "\n" +
                    "สถานที่ : " +
                    startName + " - " + missionName + "\n" +
                    "ยอดคงเหลือ : " +
                    budget.Balance.ToString("#,##0.00") + " บ.";
            }
            else
            {
                var diff =
                    jobBudget.Amount - total;

                db.TblBudgetTransactions.Add(
                    new TblBudgetTransaction
                    {
                        TransactionCode =
                            transactionCode,

                        TransactionType = "JOB",
                        BudgetCode = TransportBudgetCode,
                        TransactionDate = now,
                        Balance = budget.Balance,
                        Amount = total,
                        AmountDiff = diff,
                        PaymentType = "UPDATE",
                        IsEnabled = true,
                        CreateDate = now,
                        CreateBy = model.username,
                        UpdateDate = now,
                        UpdateBy = model.username
                    });

                budget.Balance += diff;

                db.TblJobBudgetTransactions.Add(
                    new TblJobBudgetTransaction
                    {
                        JobDetailCode =
                            model.job_detail_code,

                        TransactionCode =
                            transactionCode
                    });

                var missionName =
                    (order.MissionName ?? string.Empty)
                        .Replace("&", " ");

                var startName =
                    order.StartName ?? string.Empty;

                notificationMessage =
                    "แก้ไขการจ่ายเงินสำรองของงานเลขที่ " +
                    model.job_detail_code + "\n" +
                    "ลูกค้า : " +
                    order.CustomerAcronym + "\n" +
                    "สถานที่ : " +
                    startName + " - " + missionName + "\n" +
                    "จากเดิมจ่าย : " +
                    jobBudget.Amount.ToString("#,##0.00") + " บ.\n" +
                    "ยอดจ่ายใหม่ : " +
                    total.ToString("#,##0.00") + " บ.\n" +
                    "ยอดคงเหลือ : " +
                    budget.Balance.ToString("#,##0.00") + " บ.";
            }

            jobDetail.IsAdvancePaid = true;
            jobDetail.IsAdvanceProve =
                model.is_document;

            var newValues = new
            {
                Job = GetScalarSnapshot(jobDetail),
                Cashing = GetScalarSnapshot(cashing),
                Advance = GetScalarSnapshot(advance),
                Budget = GetScalarSnapshot(budget),
                TransactionCode = transactionCode,
                Total = total
            };

            await AddAuditAsync(
                AuditActions.Update,
                nameof(TblJobDetail),
                model.job_detail_code,
                $"แก้ไขการจ่ายเงินสำรองงาน {model.job_detail_code} โดย {model.username} (paid)",
                oldValues,
                newValues,
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var response = Success();
            response.notifMessage = notificationMessage;

            return response;
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
                nameof(UpdateAdvanceData),
                model.job_detail_code);
        }

    }

    // =====================================================
    // Update Job Data
    // =====================================================
    public async Task<ResponseViewModel> UpdateData(
        UpdateDataModel dataToUpdate,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (dataToUpdate == null ||
            string.IsNullOrWhiteSpace(
                dataToUpdate.job_detail_code))
        {
            return Failure("ไม่พบข้อมูลงาน");
        }

        try
        {
            var detail = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         dataToUpdate.job_detail_code,
                    cancellationToken);

            if (detail == null)
            {
                return Failure("ไม่พบข้อมูลงาน");
            }

            var oldValues = GetScalarSnapshot(detail);

            var jobMaster = await db.TblJobMasters
                .FirstOrDefaultAsync(
                    w => w.JobCode == detail.JobCode,
                    cancellationToken);

            if (jobMaster == null)
            {
                return Failure("ไม่พบข้อมูล Job Master");
            }

            var truck = await db.TblTruckDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.TruckCode == detail.TruckCode,
                    cancellationToken);

            jobMaster.JobDate =
                dataToUpdate.job_date;

            detail.TrailerLicense =
                NormalizeText(
                    dataToUpdate.trailer_license);

            detail.ContainerNo =
                NormalizeUpper(
                    dataToUpdate.container_no);

            detail.SealNo =
                NormalizeUpper(
                    dataToUpdate.seal_no);

            detail.TareWeight =
                dataToUpdate.tare_weight;

            detail.FinishPlaceCode =
                dataToUpdate.job_type != "FLOOR"
                    ? dataToUpdate.finish_place_code
                    : EmptyPlaceCode;

            detail.UpdateDate = DateTime.Now;
            detail.UpdateBy = username;

            if (dataToUpdate.job_type != "FLOOR" &&
                dataToUpdate.finish_place_code !=
                    EmptyPlaceCode &&
                dataToUpdate.job_type == "IMPORT" &&
                detail.IsAdvancePaid == false)
            {
                var advance = await db.TblJobadvances
                    .FirstOrDefaultAsync(
                        w => w.JobDetailCode ==
                             dataToUpdate.job_detail_code,
                        cancellationToken);

                var container = await db.TblContainerSizes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        w => w.ContainerSizeCode ==
                             dataToUpdate.container_size_code,
                        cancellationToken);

                var finishYardPort =
                    await GetYardPortAsync(
                        dataToUpdate.finish_place_code,
                        cancellationToken);

                var lift =
                    await GetYardLiftAsync(
                        dataToUpdate.finish_place_code,
                        cancellationToken);

                if (advance == null ||
                    container == null ||
                    finishYardPort == null ||
                    lift == null)
                {
                    return Failure(
                        "ข้อมูลสำหรับคำนวณค่า Advance ไม่ครบถ้วน");
                }

                advance.AdvanceLiftoff = 0;

                advance.AdvanceLifton =
                    container.ContainerSizeGroup == "S"
                        ? lift.ShortliftoffFee
                        : lift.LongliftoffFee;

                advance.AdvanceYard =
                    finishYardPort.ImportFee;
            }

            if (detail.SupplierCode ==
                    InternalSupplierCode &&
                detail.IsAdvancePaid == false)
            {
                if (truck == null)
                {
                    return Failure(
                        "ไม่พบข้อมูลประเภทรถ");
                }

                var cashing = await db.TblJobcashings
                    .FirstOrDefaultAsync(
                        w => w.JobDetailCode ==
                             dataToUpdate.job_detail_code,
                        cancellationToken);

                if (cashing == null)
                {
                    return Failure(
                        "ไม่พบข้อมูล Cashing");
                }

                cashing.CashingFuel =
                    await GetFuelRateAsync(
                        detail.MissionPlaceCode,
                        truck.TruckTypegroupCode,
                        cancellationToken);

                cashing.IsEnabled = true;
            }

            var newValues = GetScalarSnapshot(detail);

            await AddAuditAsync(
                AuditActions.Update,
                nameof(TblJobDetail),
                dataToUpdate.job_detail_code,
                $"แก้ไขข้อมูลงาน {dataToUpdate.job_detail_code} โดย {username}",
                oldValues,
                newValues,
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
                nameof(UpdateData),
                dataToUpdate.job_detail_code);
        }
    }

    // =====================================================
    // Change Driver
    // =====================================================
    public async Task<ResponseViewModel> ChangeDriver(
        ChangeDriverModel model,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.job_detail_code))
        {
            return Failure("ไม่พบข้อมูลงาน");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var jobDetail = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            if (jobDetail == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูลงาน");
            }

            var oldValues = GetScalarSnapshot(jobDetail);
            var oldSupplierCode = jobDetail.SupplierCode;
            var oldDriverCode = jobDetail.DriverCode;

            /*
             * ใช้รถใหม่ในการคำนวณ เพราะหลังจบ method
             * JobDetail จะเปลี่ยนเป็น new_truck_code
             */
            var newTruck = await (
                from a in db.TblTrucks.AsNoTracking()
                join b in db.TblTruckDetails.AsNoTracking()
                    on a.TruckCode equals b.TruckCode
                where a.TruckCode ==
                      model.new_truck_code
                select new
                {
                    a.SupplierCode,
                    b.TruckTypegroupCode
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (newTruck == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูลรถคันใหม่");
            }

            await ChangeDriverDataAsync(
                model,
                cancellationToken);

            await db.TblSupplierAllowances
                .Where(w =>
                    w.SupplierCode == oldSupplierCode &&
                    w.JobDetailCode == model.job_detail_code)
                .ExecuteDeleteAsync(cancellationToken);

            /*
             * ต้นฉบับใช้ supplier_code เก่าใส่ SupplierAllowance
             * ทั้งที่ Job ถูกเปลี่ยนเป็น new_supplier_code
             * ตัวนี้แก้ให้ relation สอดคล้องกับ Job หลังเปลี่ยน
             */
            var supplierTrip =
                await GetSupplierCostAsync(
                    model.new_supplier_code,
                    newTruck.TruckTypegroupCode,
                    jobDetail.MissionPlaceCode,
                    cancellationToken);

            db.TblSupplierAllowances.Add(
                new TblSupplierAllowance
                {
                    SupplierCode =
                        model.new_supplier_code,

                    JobDetailCode =
                        model.job_detail_code,

                    CostValue =
                        supplierTrip
                });

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            if (cashing != null &&
                model.new_supplier_code !=
                    InternalSupplierCode)
            {
                cashing.CashingFuel = 0;
            }

            jobDetail.SupplierCode =
                model.new_supplier_code;

            jobDetail.DriverCode =
                string.IsNullOrWhiteSpace(
                    model.new_driver_code)
                    ? DefaultDriverCode
                    : model.new_driver_code;

            jobDetail.TruckCode =
                string.IsNullOrWhiteSpace(
                    model.new_truck_code)
                    ? DefaultTruckCode
                    : model.new_truck_code;

            jobDetail.TrailerLicense =
                NormalizeText(model.trailer_license);

            jobDetail.UpdateBy = username;
            jobDetail.UpdateDate = DateTime.Now;

            var newValues = GetScalarSnapshot(jobDetail);

            await AddAuditAsync(
                AuditActions.Update,
                nameof(TblJobDetail),
                model.job_detail_code,
                $"เปลี่ยนคนขับงาน {model.job_detail_code} จาก {oldDriverCode} เป็น {jobDetail.DriverCode} โดย {username}",
                oldValues,
                newValues,
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
                nameof(ChangeDriver),
                model.job_detail_code);
        }
    }

    // =====================================================
    // Cancel Job
    // =====================================================
    public async Task<ResponseViewModel> CancelJob(
        CancelJobModel model,
        CancellationToken cancellationToken = default)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.job_detail_code))
        {
            return Failure("ไม่พบข้อมูลงาน");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var detail = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            if (detail == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("ไม่พบข้อมูลงาน");
            }

            if (detail.IsCanceled == true)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("งานนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues = GetScalarSnapshot(detail);

            detail.CostValue = model.trip_cost;
            detail.IsCanceled = true;
            detail.UpdateDate = DateTime.Now;
            detail.UpdateBy = model.create_by;

            var job = await db.TblJobMasters
                .FirstOrDefaultAsync(
                    w => w.JobCode == detail.JobCode,
                    cancellationToken);

            if (job != null)
            {
                job.LiftedAmount =
                    job.LiftedAmount - 1;
            }

            var advance = await db.TblJobadvances
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            if (advance != null)
            {
                advance.IsEnabled = false;
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            if (cashing != null)
            {
                cashing.IsEnabled = false;
            }

            await CancelDriverCostAsync(
                detail,
                model.driver_cost,
                cancellationToken);

            var newValues = GetScalarSnapshot(detail);

            await AddAuditAsync(
                AuditActions.Update,
                nameof(TblJobDetail),
                model.job_detail_code,
                $"ยกเลิกงาน {model.job_detail_code} โดย {model.create_by}",
                oldValues,
                newValues,
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
                nameof(CancelJob),
                model.job_detail_code);
        }
    }

    // =====================================================
    // Search
    // =====================================================
    public async Task<List<JobViewModel>> GetOrderBySearch(
        string code,
        CancellationToken cancellationToken = default)
    {
        code = code?.Trim().ToUpperInvariant()
               ?? string.Empty;

        if (code.Length == 0)
        {
            return new List<JobViewModel>();
        }

        var rawData = await db.ViewJobSummaries
            .AsNoTracking()
            .Where(a =>
                a.JobDetailCode.Contains(code) ||
                a.BookingNo.Contains(code) ||
                a.JobInvoice.Contains(code) ||
                a.ContainerNo.Contains(code))
            .Select(a => new
            {
                a.JobType,
                a.BookingNo,
                a.JobCode,
                a.JobInvoice,
                a.ContainerNo,
                a.JobDetailCode,
                a.MissionName,
                a.CustomerAcronym,
                a.JobDate,
                a.SupplierName,
                a.TrackingStatus
            })
            .ToListAsync(cancellationToken);

        return rawData
            .Select(a => new JobViewModel
            {
                job_type = a.JobType,
                booking_no = a.BookingNo,
                job_code = a.JobCode,
                job_invoice = a.JobInvoice,
                container_no = a.ContainerNo,
                job_detail_code = a.JobDetailCode,
                mission_name = a.MissionName,
                customer_name = a.CustomerAcronym,
                job_date =
                    a.JobDate.ToString("dd-MM-yyyy"),
                supplier_name = a.SupplierName,
                tracking_status = a.TrackingStatus
            })
            .ToList();
    }

    public async Task<List<JobViewModel>> GetTarryJobList(
        CancellationToken cancellationToken = default)
    {
        return await db.ViewJobListActives
            .AsNoTracking()
            .Where(a =>
                a.IsDocument == false &&
                a.IsCanceled == false &&
                a.JobDetailCode != null &&
                a.IsSingleTrip == true)
            .Select(a => new JobViewModel
            {
                job_code = a.JobCode,
                job_type = a.JobType,
                customer_name = a.CustomerNameTh,
                booking_no = a.BookingNo,
                container_type = a.ContainerType,
                job_status = a.JobStatus,
                start_name = a.StartName,
                job_date_value = a.JobDate,
                seal_no = a.SealNo,
                cashing_total = a.CashingTotal,
                is_billing = a.IsBilling,
                mission_name = a.MissionName,
                job_detail_code = a.JobDetailCode,
                trailer_license = a.TrailerLicense,
                truck_license = a.TruckLicense,
                container_no = a.ContainerNo,
                is_document = a.IsDocument,
                hash_code = a.HashCode
            })
            .ToListAsync(cancellationToken);
    }

    // =====================================================
    // Private Business Helpers
    // =====================================================
    private async Task<decimal> GetDriverCostAsync(
        string placeCode,
        CancellationToken cancellationToken)
    {
        return await db.TblDriverRates
            .AsNoTracking()
            .Where(w => w.PlaceCode == placeCode)
            .Select(w => (decimal?)w.RateValue)
            .FirstOrDefaultAsync(cancellationToken)
            ?? 0;
    }

    private async Task<TblYardCashing?> GetYardLiftAsync(
        string yardPortCode,
        CancellationToken cancellationToken)
    {
        return await db.TblYardCashings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.YardportCode == yardPortCode,
                cancellationToken);
    }

    private async Task<TblYardport?> GetYardPortAsync(
        string yardPortCode,
        CancellationToken cancellationToken)
    {
        return await db.TblYardports
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.YardportCode == yardPortCode,
                cancellationToken);
    }

    private async Task CancelDriverCostAsync(
        TblJobDetail jobDetail,
        decimal driverCost,
        CancellationToken cancellationToken)
    {
        if (jobDetail.SupplierCode !=
            InternalSupplierCode)
        {
            return;
        }

        var driver = await db.TblDriverCosts
            .FirstOrDefaultAsync(
                w =>
                    w.DriverCode == jobDetail.DriverCode &&
                    w.JobDetailCode ==
                        jobDetail.JobDetailCode,
                cancellationToken);

        if (driver == null)
        {
            return;
        }

        /*
         * รักษา business rule เดิม
         */
        const decimal holiday = 0;

        driver.TripValue =
            driverCost + holiday;

        driver.FuelValue = 0;
        driver.WasteValue = 0;
        driver.ExpresswayValue = 0;
    }

    private async Task EditDriverDataAsync(
        ChangeDriverModel model,
        CancellationToken cancellationToken)
    {
        var driverCost = await db.TblDriverCosts
            .FirstOrDefaultAsync(
                w =>
                    w.JobDetailCode ==
                        model.job_detail_code &&
                    w.CostType == "A",
                cancellationToken);

        if (model.supplier_code ==
                InternalSupplierCode &&
            model.new_supplier_code ==
                InternalSupplierCode)
        {
            var wage =
                await GetDriverCostAsync(
                    model.mission_code,
                    cancellationToken);

            if (driverCost != null)
            {
                driverCost.TripValue = wage;
                driverCost.DriverCode =
                    model.new_driver_code;

                driverCost.FuelValue = 0;
            }
            else
            {
                db.TblDriverCosts.Add(
                    new TblDriverCost
                    {
                        JobDetailCode =
                            model.job_detail_code,

                        DriverCode =
                            model.new_driver_code,

                        TripValue = wage,
                        CostType = "A",
                        FuelValue = 0,
                        ExpresswayValue = 0
                    });
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            if (cashing != null)
            {
                cashing.IsEnabled = true;
            }
        }
        else if (model.supplier_code !=
                     InternalSupplierCode &&
                 model.new_supplier_code ==
                     InternalSupplierCode)
        {
            var wage =
                await GetDriverCostAsync(
                    model.mission_code,
                    cancellationToken);

            if (driverCost != null)
            {
                driverCost.TripValue = wage;
                driverCost.DriverCode =
                    model.new_driver_code;

                driverCost.FuelValue = 0;
            }
            else
            {
                db.TblDriverCosts.Add(
                    new TblDriverCost
                    {
                        JobDetailCode =
                            model.job_detail_code,

                        DriverCode =
                            model.new_driver_code,

                        TripValue = wage,
                        CostType = "A",
                        FuelValue = 0,
                        ExpresswayValue = 0
                    });
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            if (cashing != null)
            {
                cashing.IsEnabled = true;
            }
        }
        else
        {
            if (driverCost != null)
            {
                db.TblDriverCosts.Remove(
                    driverCost);
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            if (cashing != null)
            {
                cashing.IsEnabled = false;
            }
        }
    }

    private async Task ChangeDriverDataAsync(
        ChangeDriverModel model,
        CancellationToken cancellationToken)
    {
        var driverCost = await db.TblDriverCosts
            .FirstOrDefaultAsync(
                w =>
                    w.JobDetailCode ==
                        model.job_detail_code &&
                    w.CostType == "A",
                cancellationToken);

        if (model.supplier_code ==
                InternalSupplierCode &&
            model.new_supplier_code ==
                InternalSupplierCode)
        {
            if (driverCost != null)
            {
                driverCost.TripValue =
                    model.driver_cost;

                driverCost.FuelValue = 0;
                driverCost.WasteValue = 0;
                driverCost.CostType = "B";

                var wage =
                    await GetDriverCostAsync(
                        model.mission_code,
                        cancellationToken);

                db.TblDriverCosts.Add(
                    new TblDriverCost
                    {
                        JobDetailCode =
                            model.job_detail_code,

                        DriverCode =
                            model.new_driver_code,

                        TripValue = wage,
                        FuelValue = 0,
                        CostType = "A",
                        ExpresswayValue = 0
                    });
            }
            else
            {
                /*
                 * รักษา behavior เดิมของไฟล์ที่ผู้ใช้ส่งมา
                 */
                db.TblDriverCosts.Add(
                    new TblDriverCost
                    {
                        JobDetailCode =
                            model.job_detail_code,

                        DriverCode =
                            model.driver_code,

                        TripValue =
                            model.driver_cost,

                        FuelValue = 0,
                        CostType = "A",
                        ExpresswayValue = 0
                    });
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            if (cashing != null)
            {
                cashing.IsEnabled = true;
            }
        }
        else if (model.supplier_code !=
                     InternalSupplierCode &&
                 model.new_supplier_code ==
                     InternalSupplierCode)
        {
            if (driverCost != null)
            {
                driverCost.TripValue =
                    model.driver_cost;

                driverCost.FuelValue = 0;
                driverCost.WasteValue = 0;
                driverCost.CostType = "A";
            }
            else
            {
                var wage =
                    await GetDriverCostAsync(
                        model.mission_code,
                        cancellationToken);

                db.TblDriverCosts.Add(
                    new TblDriverCost
                    {
                        JobDetailCode =
                            model.job_detail_code,

                        DriverCode =
                            model.new_driver_code,

                        TripValue = wage,
                        FuelValue = 0,
                        CostType = "A",
                        ExpresswayValue = 0
                    });
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            if (cashing != null)
            {
                cashing.IsEnabled = true;
            }
        }
        else
        {
            if (driverCost != null)
            {
                db.TblDriverCosts.Remove(
                    driverCost);
            }

            var cashing = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode ==
                         model.job_detail_code,
                    cancellationToken);

            if (cashing != null)
            {
                cashing.IsEnabled = false;
            }
        }
    }

    private async Task<int> GetFuelRateAsync(
        string placeCode,
        string? truckType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(placeCode) ||
            string.IsNullOrWhiteSpace(truckType))
        {
            return 0;
        }

        var zoneCode = await db.TblIndustries
            .AsNoTracking()
            .Where(w => w.IndustryCode == placeCode)
            .Select(w => w.ZoneCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(zoneCode))
        {
            return 0;
        }

        return await db.TblFuelRates
            .AsNoTracking()
            .Where(w =>
                w.ZoneCode == zoneCode &&
                w.TruckTypegroupCode == truckType)
            .Select(w => (int?)w.FuelRateValue)
            .FirstOrDefaultAsync(cancellationToken)
            ?? 0;
    }

    private async Task<decimal> GetSupplierCostAsync(
        string? supplierCode,
        string? truckType,
        string placeCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(supplierCode) ||
            string.IsNullOrWhiteSpace(placeCode))
        {
            return 0;
        }

        var fleetTypeCode = await db.TblSuppliers
            .AsNoTracking()
            .Where(w => w.SupplierCode == supplierCode)
            .Select(w => w.FleetTypeCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (fleetTypeCode == null)
        {
            return 0;
        }

        if (fleetTypeCode == "F1")
        {
            var fleetRate = await (
                from a in db.TblFleetInternals.AsNoTracking()
                join b in db.TblFleetRates.AsNoTracking()
                    on a.FleetCode equals b.FleetCode
                where a.SupplierCode == supplierCode
                      && b.PlaceCode == placeCode
                select b)
                .FirstOrDefaultAsync(cancellationToken);

            if (fleetRate == null)
            {
                return 0;
            }

            return truckType switch
            {
                "01" => fleetRate.Truck4w,
                "02" => fleetRate.Truck6w,
                "03" => fleetRate.Truck10w,
                _ => 0
            };
        }

        return await db.TblSupplierRates
            .AsNoTracking()
            .Where(w =>
                w.SupplierCode == supplierCode &&
                w.PlaceCode == placeCode)
            .Select(w => (decimal?)w.CostRate)
            .FirstOrDefaultAsync(cancellationToken)
            ?? 0;
    }

    // =====================================================
    // Budget Detail Ledger
    // =====================================================
    private void AddBudgetTransactionDetail(
        string jobDetailCode,
        string transactionCode,
        string description,
        string paymentType,
        decimal value,
        string username)
    {
        var entity =
            new LogBudgetTransaction();

        var entry =
            db.Entry(entity);

        /*
         * 3 field นี้ยืนยันชื่อได้จาก repository เดิม:
         * JobDetailCode, TransactionCode, PaymentType
         */
        SetPropertyIfExists(
            entry,
            "JobDetailCode",
            jobDetailCode);

        SetPropertyIfExists(
            entry,
            "TransactionCode",
            transactionCode);

        SetPropertyIfExists(
            entry,
            "PaymentType",
            paymentType);

        /*
         * รองรับชื่อ column ของ entity รุ่นเก่า/ใหม่
         * โดยไม่ผูก compile-time กับ property ที่อาจต่างกัน
         */
        SetFirstPropertyIfExists(
            entry,
            new[]
            {
                "PaymentName",
                "PaymentDetail",
                "TransactionName",
                "Description",
                "Remark",
                "Detail"
            },
            description);

        SetFirstPropertyIfExists(
            entry,
            new[]
            {
                "PaymentValue",
                "Amount",
                "Value"
            },
            value);

        SetPropertyIfExists(
            entry,
            "CreateDate",
            DateTime.Now);

        SetPropertyIfExists(
            entry,
            "CreateBy",
            username);

        SetPropertyIfExists(
            entry,
            "UpdateDate",
            DateTime.Now);

        SetPropertyIfExists(
            entry,
            "UpdateBy",
            username);

        SetPropertyIfExists(
            entry,
            "IsEnabled",
            true);

        db.LogBudgetTransactions.Add(entity);
    }

    private static void SetPropertyIfExists(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
        string propertyName,
        object? value)
    {
        if (entry.Metadata.FindProperty(propertyName) != null)
        {
            entry.Property(propertyName).CurrentValue =
                value;
        }
    }

    private static void SetFirstPropertyIfExists(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
        IEnumerable<string> propertyNames,
        object? value)
    {
        foreach (var propertyName in propertyNames)
        {
            if (entry.Metadata.FindProperty(propertyName) == null)
            {
                continue;
            }

            entry.Property(propertyName).CurrentValue =
                value;

            return;
        }
    }

    // =====================================================
    // Audit / Utility Helpers
    // =====================================================
    private async Task AddAuditAsync(
        string actionName,
        string entityName,
        string entityId,
        string description,
        object? oldValues,
        object? newValues,
        CancellationToken cancellationToken)
    {
        await auditLogService.AddAsync(
            new AuditLogEntry
            {
                ModuleName = "Job",
                ActionName = actionName,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                OldValues = oldValues,
                NewValues = newValues,
                Result = AuditResults.Success
            },
            cancellationToken);
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

    private void CopyEditableJobValues(
        TblJobDetail target,
        TblJobDetail source)
    {
        var protectedProperties =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                nameof(TblJobDetail.JobDetailCode),
                nameof(TblJobDetail.JobCode),
                nameof(TblJobDetail.IsEnabled),
                nameof(TblJobDetail.IsCanceled),
                "CreateDate",
                "CreateBy",
                "RowVersion",
                "Timestamp"
            };

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

            if (protectedProperties.Contains(propertyName))
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

    private static ResponseViewModel? ValidateFinishArrays(
        FinishJobModel model)
    {
        if (model.valCashing != null)
        {
            var count =
                model.valCashing.Count();

            if (model.nameCashing == null ||
                model.valWithdrawCashing == null ||
                model.nameCashing.Count() < count ||
                model.valWithdrawCashing.Count() < count)
            {
                return Failure(
                    "ข้อมูลค่าใช้จ่าย Cashing ไม่ครบถ้วน");
            }
        }

        if (model.valAdvance != null)
        {
            var count =
                model.valAdvance.Count();

            if (model.nameAdvance == null ||
                model.valWithdrawAdvance == null ||
                model.nameAdvance.Count() < count ||
                model.valWithdrawAdvance.Count() < count)
            {
                return Failure(
                    "ข้อมูลค่าใช้จ่าย Advance ไม่ครบถ้วน");
            }
        }

        return null;
    }

    private static ResponseViewModel? ValidateAdvanceArrays(
        FinishJobModel model)
    {
        if (model.valCashing != null)
        {
            var count =
                model.valCashing.Count();

            if (model.nameCashing == null ||
                model.nameCashing.Count() < count)
            {
                return Failure(
                    "ข้อมูลค่าใช้จ่าย Cashing ไม่ครบถ้วน");
            }
        }

        if (model.valAdvance != null)
        {
            var count =
                model.valAdvance.Count();

            if (model.nameAdvance == null ||
                model.nameAdvance.Count() < count)
            {
                return Failure(
                    "ข้อมูลค่าใช้จ่าย Advance ไม่ครบถ้วน");
            }
        }

        return null;
    }

    private static ResponseViewModel Success(
        string message = "บันทึกข้อมูลเรียบร้อยแล้ว")
    {
        return new ResponseViewModel
        {
            result = true,
            resultMessage = message
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
                "Error while rolling back Job transaction.");
        }
    }
}
