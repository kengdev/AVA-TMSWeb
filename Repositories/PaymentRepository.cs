using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMSWeb.Models;
using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using TMSWeb.Services;

namespace TMSWeb.Repositories;

public class PaymentRepository
{
    private readonly DbavalogContext db;
    private readonly CodeRepository codeRepo;
    private readonly IAuditLogService auditLogService;
    private readonly ILogger<PaymentRepository> logger;
    public PaymentRepository(
        DbavalogContext context,
        CodeRepository _codeRepo,
        IAuditLogService _auditLogService,
        ILogger<PaymentRepository> _logger)
    {
        db = context;
        codeRepo = _codeRepo;
        auditLogService = _auditLogService;
        logger = _logger;
    }

    public async Task<List<DriverPaymentViewModel>> GetDriverPayment(string month, string year)
    {
        var m = int.Parse(month);
        var y = int.Parse(year);
        var startDate = new DateOnly(y, m, 1);
        var endDate = startDate.AddMonths(1);

        var data = await db.TblAllowances
            .AsNoTracking()
            .Where(w => w.IsEnabled == true &&
                        w.StartDate >= startDate &&
                        w.StartDate < endDate)
            .GroupBy(g => new { g.StartDate, g.EndDate })
            .Select(s => new DriverPaymentViewModel
            {
                start_date = s.Key.StartDate,
                end_date = s.Key.EndDate,
                allowance_total = s.Sum(x => x.AllowanceTotal)
            })
            .ToListAsync();

        foreach (var item in data)
        {
            item.start_date_text = item.start_date.ToString("dd MMMM yyyy");
            item.end_date_text = item.end_date.ToString("dd MMMM yyyy");
            item.start_date_form = item.start_date.ToString("yyyy-MM-dd");
            item.end_date_form = item.end_date.ToString("yyyy-MM-dd");
        }

        return data;
    }

    public async Task<List<DriverPaymentViewModel>> GetDriverPaymentByDate(DateOnly start_date)
    {
        var data = await (from a in db.TblAllowances
                          join b in db.ViewDriverListActives on a.DriverCode equals b.DriverCode
                          where a.StartDate == start_date && a.IsEnabled == true
                          select new DriverPaymentViewModel
                          {
                              allowance_code = a.AllowanceCode,
                              driver_code = b.DriverCode,
                              driver_fullname = b.DriverFullname,
                              truck_license = b.TruckLicense != null ? b.TruckLicense : "-",
                              truck_no = b.TruckNo != null ? b.TruckNo : "-",
                              allowance_total = a.AllowanceTotal
                          }).ToListAsync();
        return data.OrderBy(o => o.truck_no).ToList();
    }

    public async Task<List<DriverPaymentDetailViewModel>> GetDriverPaymentListByCode(string code)
    {
        var data = await (from b in db.TblAllowanceDetails
                          join c in db.TblAllowances on b.AllowanceCode equals c.AllowanceCode
                          join d in db.TblJobDetails on b.JobDetailCode equals d.JobDetailCode into d1
                          from aj in d1.DefaultIfEmpty()

                          join q in db.TblJobMasters on aj.JobCode equals q.JobCode into q1
                          from jd in q1.DefaultIfEmpty()

                          join i in db.TblPlaces on aj.MissionPlaceCode equals i.PlaceCode into i1
                          from jp in i1.DefaultIfEmpty()
                          where c.AllowanceCode == code
                          select new DriverPaymentDetailViewModel
                          {
                              working_date = b.AllowanceDate.ToDateTime(TimeOnly.Parse("00:00 AM")),
                              allowance_code = b.AllowanceCode,
                              job_detail_code = b.JobDetailCode,
                              booking_no = jd.BookingNo == null ? "-" : jd.BookingNo,
                              container_no = aj.ContainerNo == null ? "-" : aj.ContainerNo,
                              trip_place = jp.PlaceName == null ? "-" : jp.PlaceName,
                              trip_value = b.TripValue,
                              fuel_value = b.FuelValue,
                              waste_value = b.WasteValue,
                              other_value = b.OtherValue,
                              driver_code = c.DriverCode,
                              is_approve = aj.IsApprove,
                              id = b.Id,
                              mission_name = jp.PlaceName
                          }).ToListAsync();
        return data;
    }

    public async Task<List<SubstractPaymentViewModel>> GetDriverSubtractByCode(string allowance_code)
    {
        var data = await (from a in db.TblDriverSpecialvalues
                          where a.AllowanceCode == allowance_code
                          select new SubstractPaymentViewModel
                          {
                              sub_type = a.SpecialvalueType,
                              sub_name = a.SpecialvalueName,
                              sub_value = a.Specialvalue
                          }).ToListAsync();

        return data;
    }

    public async Task<DriverViewModel?> GetTruckDriverByDriver(string driver_code)
    {
        var data = await (from a in db.ViewDriverListActives
                          where a.DriverCode == driver_code
                          select new DriverViewModel
                          {
                              driver_code = a.DriverCode,
                              //id_card = a.id,
                              //gender_id = a.DriverGender,
                              fullname = a.DriverFullname,
                              //age = DateTime.Now.Year - a.BirthDate.Year,
                              phone = a.DriverPhone,
                              //driver_birthdate = a.BirthDate.ToString("yyyy-MM-dd"),
                              //driver_img = a.EmpImg,
                              truck_license = a.TruckLicense != null ? a.TruckLicense + " [" + a.TruckNo + "]" : "-",
                              supplyer_name = a.SupplierCode != null ? a.SupplierFullname : "-"
                          }).FirstOrDefaultAsync();

        return data;
    }

    public async Task<ResponseViewModel> EditDriverPayment(
        DriverPaymentModel paymentToAdd,
        string username)
    {
        if (paymentToAdd == null)
        {
            return Failure("ไม่พบข้อมูลเบี้ยเลี้ย");
        }

        var count =
            paymentToAdd.allowance_detail_id?.Count ?? 0;

        if (count == 0 ||
            paymentToAdd.trip == null ||
            paymentToAdd.fuel == null ||
            paymentToAdd.waste == null ||
            paymentToAdd.other == null ||
            paymentToAdd.trip.Count != count ||
            paymentToAdd.fuel.Count != count ||
            paymentToAdd.waste.Count != count ||
            paymentToAdd.other.Count != count)
        {
            return Failure(
                "ข้อมูลรายละเอียดเบี้ยเลี้ยไม่ครบหรือจำนวนรายการไม่ตรงกัน");
        }

        if (paymentToAdd.allowance_detail_id
            .Distinct()
            .Count() != count)
        {
            return Failure(
                "พบรายการเบี้ยเลี้ยซ้ำ");
        }

        if (paymentToAdd.subtractName != null &&
            (paymentToAdd.subtractVal == null ||
             paymentToAdd.subtractType == null ||
             paymentToAdd.subtractName.Count !=
             paymentToAdd.subtractVal.Count ||
             paymentToAdd.subtractName.Count !=
             paymentToAdd.subtractType.Count))
        {
            return Failure(
                "ข้อมูลรายการเพิ่ม/หักไม่ครบหรือจำนวนรายการไม่ตรงกัน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            decimal sumTotal = 0;
            decimal sumSubtract = 0;
            decimal sumAddition = 0;

            var ids = paymentToAdd
                .allowance_detail_id
                .Distinct()
                .ToList();

            var allowanceDetails =
                await db.TblAllowanceDetails
                    .Where(w =>
                        w.AllowanceCode ==
                        paymentToAdd.allowance_code &&
                        ids.Contains(w.Id))
                    .ToListAsync();

            if (allowanceDetails.Count != ids.Count)
            {
                await tr.RollbackAsync();

                return Failure(
                    "พบรายละเอียดเบี้ยเลี้ยบางรายการไม่อยู่ในเอกสารนี้");
            }

            var allowanceById =
                allowanceDetails.ToDictionary(x => x.Id);

            for (var i = 0; i < count; i++)
            {
                var id =
                    paymentToAdd.allowance_detail_id[i];

                var trip =
                    paymentToAdd.trip[i];

                var fuel =
                    paymentToAdd.fuel[i];

                var waste =
                    paymentToAdd.waste[i];

                var other =
                    paymentToAdd.other[i];

                sumTotal +=
                    trip + fuel + waste + other;

                var allowanceDetail =
                    allowanceById[id];

                allowanceDetail.TripValue = trip;
                allowanceDetail.FuelValue = fuel;
                allowanceDetail.WasteValue = waste;
                allowanceDetail.OtherValue = other;
            }

            var special =
                await db.TblDriverSpecialvalues
                    .Where(w =>
                        w.AllowanceCode ==
                        paymentToAdd.allowance_code &&
                        w.DriverCode ==
                        paymentToAdd.driver_code)
                    .ToListAsync();

            if (special.Count > 0)
            {
                db.TblDriverSpecialvalues.RemoveRange(
                    special);
            }

            if (paymentToAdd.subtractName != null)
            {
                var newSpecialValues =
                    new List<TblDriverSpecialvalue>();

                for (var i = 0;
                     i < paymentToAdd.subtractName.Count;
                     i++)
                {
                    var name =
                        paymentToAdd.subtractName[i];

                    var value =
                        paymentToAdd.subtractVal![i];

                    var type =
                        paymentToAdd.subtractType![i];

                    if (string.IsNullOrWhiteSpace(name) ||
                        value <= 0)
                    {
                        continue;
                    }

                    if (type == "sub")
                    {
                        sumSubtract += value;
                    }
                    else
                    {
                        sumAddition += value;
                    }

                    newSpecialValues.Add(
                        new TblDriverSpecialvalue
                        {
                            SpecialvalueType = type,
                            SpecialvalueName = name,
                            Specialvalue = value,
                            DriverCode =
                                paymentToAdd.driver_code,
                            AllowanceCode =
                                paymentToAdd.allowance_code
                        });
                }

                if (newSpecialValues.Count > 0)
                {
                    db.TblDriverSpecialvalues.AddRange(
                        newSpecialValues);
                }
            }

            var allowance =
                await db.TblAllowances
                    .FirstOrDefaultAsync(
                        w =>
                            w.AllowanceCode ==
                            paymentToAdd.allowance_code);

            if (allowance == null)
            {
                await tr.RollbackAsync();
                return Failure("ไม่พบข้อมูลเบี้ยเลี้ย");
            }

            allowance.SubtractTotal = sumSubtract;
            allowance.AdditionalTotal = sumAddition;
            allowance.AllowanceTotal =
                (sumTotal + sumAddition) -
                sumSubtract;

            allowance.UpdateDate = DateTime.Now;
            allowance.UpdateBy = username;

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "EditDriverPayment",
                paymentToAdd.allowance_code);
        }
    }

    public async Task<List<DriverPaymentDetailViewModel>> GetOrderList(string driver_code, DateTime start, DateTime end)
    {
        var list = new List<DriverPaymentDetailViewModel>();
        var startDate = start.Date;
        var endExclusive = end.Date.AddDays(1);
        var dayCount = (endExclusive - startDate).Days;

        var driver = await db.ViewDriverListActives
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.DriverCode == driver_code);

        var data = await db.ViewDriverPaymentListActives
            .AsNoTracking()
            .Where(w => w.JobDate >= startDate &&
                        w.JobDate < endExclusive &&
                        w.DriverCode == driver_code)
            .ToListAsync();

        var jobsByDate = data
            .GroupBy(w => w.JobDate.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        for (int i = 0; i < dayCount; i++)
        {
            var currentDate = startDate.AddDays(i);

            if (!jobsByDate.TryGetValue(currentDate, out var workingDay) || workingDay.Count == 0)
            {
                list.Add(new DriverPaymentDetailViewModel
                {
                    driver_code = driver_code,
                    working_date = currentDate,
                    trip_date = currentDate.ToString("ddd, dd MMM yyyy"),
                    mission_name = "-",
                    zone_name = "-",
                    job_detail_code = "-",
                    booking_no = "-",
                    container_no = "-",
                    trip_value = 0,
                    fuel_value = 0,
                    waste_value = 0,
                    other_value = 0,
                    code = currentDate.ToString("ddMMyyyy")
                });
                continue;
            }

            foreach (var working in workingDay)
            {
                list.Add(new DriverPaymentDetailViewModel
                {
                    working_date = working.JobDate,
                    trip_date = working.JobDate.ToString("ddd, dd MMM yyyy"),
                    driver_code = driver_code,
                    job_detail_code = working.JobDetailCode,
                    booking_no = working.BookingNo,
                    container_no = working.ContainerNo,
                    mission_name = working.MissionName,
                    zone_name = working.ZoneName,
                    allownace_date = currentDate,
                    trip_value = working.TripValue,
                    fuel_value = driver != null && (driver.TruckTypeName == "6TR" || driver.TruckTypeName == "6WD")
                        ? working.FuelBalance
                        : 0,
                    waste_value = working.WasteValue,
                    other_value = working.OtherValue,
                    code = working.HashCode
                });
            }
        }

        return list;
    }

    public async Task<ViewDriverPaymentListActive?> GetDriverJobByCode(string code, string driver_code)
    {
        var data = await (from a in db.ViewDriverPaymentListActives.AsNoTracking()
                          where a.JobDetailCode == code && a.DriverCode == driver_code
                          select a).FirstOrDefaultAsync();

        return data;
    }

    public async Task<JobViewModel?> GetJobByCode(string code)
    {
        var data = await (from a in db.TblJobMasters
                          join b in db.TblJobDetails on a.JobCode equals b.JobCode
                          join c in db.TblPlaces on b.MissionPlaceCode equals c.PlaceCode
                          join d in db.TblSupplierAllowances on b.JobDetailCode equals d.JobDetailCode
                          where b.JobDetailCode == code
                          select new JobViewModel
                          {
                              cost_value = d.CostValue,
                              job_date = a.JobDate.ToString("dd-MM-yyyy"),
                              job_invoice = b.SupplierInvoice,
                              mission_name = c.PlaceName,
                              container_no = b.ContainerNo,
                              job_detail_code = b.JobDetailCode
                          }).FirstOrDefaultAsync();

        return data;
    }

    public async Task<ResponseViewModel> AddDriverPayment(
        TblAllowance model,
        OtherPaymentViewModel other)
    {
        if (model == null)
        {
            return Failure("ไม่พบข้อมูลเบี้ยเลี้ย");
        }

        if (other == null)
        {
            return Failure("ไม่พบข้อมูลรายการเพิ่ม/หัก");
        }

        if (other.subtractName != null &&
            (other.subtractVal == null ||
             other.subtractType == null ||
             other.subtractName.Count !=
             other.subtractVal.Count ||
             other.subtractName.Count !=
             other.subtractType.Count))
        {
            return Failure(
                "ข้อมูลรายการเพิ่ม/หักไม่ครบหรือจำนวนรายการไม่ตรงกัน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            model.AllowanceCode =
                await codeRepo.GetAllownceCode();

            var details =
                model.TblAllowanceDetails?.ToList()
                ?? new List<TblAllowanceDetail>();

            var jobCodes = details
                .Where(x =>
                    !string.IsNullOrWhiteSpace(
                        x.JobDetailCode) &&
                    x.JobDetailCode != "-")
                .Select(x => x.JobDetailCode)
                .Distinct()
                .ToList();

            var cashings = jobCodes.Count == 0
                ? new List<TblJobcashing>()
                : await db.TblJobcashings
                    .Where(w =>
                        jobCodes.Contains(
                            w.JobDetailCode))
                    .ToListAsync();

            var cashingByJob = cashings
                .GroupBy(x => x.JobDetailCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.First());

            var driverCosts = jobCodes.Count == 0
                ? new List<TblDriverCost>()
                : await db.TblDriverCosts
                    .Where(w =>
                        w.DriverCode ==
                        model.DriverCode &&
                        jobCodes.Contains(
                            w.JobDetailCode))
                    .ToListAsync();

            var driverCostByJob =
                driverCosts
                    .GroupBy(x => x.JobDetailCode)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First());

            foreach (var detail in details)
            {
                var jobCode = detail.JobDetailCode;

                if (!string.IsNullOrWhiteSpace(jobCode) &&
                    jobCode != "-" &&
                    cashingByJob.TryGetValue(
                        jobCode,
                        out var cashing))
                {
                    cashing.CashingFuelup =
                        cashing.CashingFuel -
                        (int)(detail.FuelValue / 20);
                }

                if (!string.IsNullOrWhiteSpace(jobCode) &&
                    jobCode != "-" &&
                    driverCostByJob.TryGetValue(
                        jobCode,
                        out var driverCost))
                {
                    driverCost.IsWeeklypayment = true;
                }

                detail.AllowanceCode =
                    model.AllowanceCode;
            }

            model.SubtractTotal = 0;

            model.AllowanceTotal =
                details.Sum(
                    s =>
                        s.TripValue +
                        s.FuelValue +
                        s.WasteValue +
                        s.OtherValue);

            model.ExchangeTotal =
                details
                    .Where(w => w.TripType == "B")
                    .Sum(s => s.TripValue);

            model.IsEnabled = true;

            decimal sumSubtract = 0;
            decimal sumAddition = 0;

            var special =
                new List<TblDriverSpecialvalue>();

            if (other.subtractName != null)
            {
                for (var i = 0;
                     i < other.subtractName.Count;
                     i++)
                {
                    var name =
                        other.subtractName[i];

                    var value =
                        other.subtractVal![i];

                    var type =
                        other.subtractType![i];

                    if (string.IsNullOrWhiteSpace(name) ||
                        value <= 0)
                    {
                        continue;
                    }

                    if (type == "sub")
                    {
                        sumSubtract += value;
                    }
                    else
                    {
                        sumAddition += value;
                    }

                    special.Add(
                        new TblDriverSpecialvalue
                        {
                            SpecialvalueType = type,
                            SpecialvalueName = name,
                            Specialvalue = value,
                            DriverCode =
                                model.DriverCode,
                            AllowanceCode =
                                model.AllowanceCode
                        });
                }
            }

            model.TblDriverSpecialvalues =
                special;

            model.AdditionalTotal =
                sumAddition;

            model.SubtractTotal =
                sumSubtract;

            model.AllowanceTotal =
                (model.AllowanceTotal +
                 sumAddition) -
                sumSubtract;

            db.TblAllowances.Add(model);

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "AddDriverPayment",
                model.AllowanceCode);
        }
    }

    public async Task<ResponseViewModel> RemoveDriverPayment(
        string code,
        string username)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัสเบี้ยเลี้ย");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var allowance =
                await db.TblAllowances
                    .FirstOrDefaultAsync(
                        w => w.AllowanceCode == code);

            if (allowance == null)
            {
                await tr.RollbackAsync();
                return Failure($"ไม่พบเบี้ยเลี้ย {code}");
            }

            if (allowance.IsEnabled != true)
            {
                await tr.RollbackAsync();
                return Failure("รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var allowanceDetails =
                await db.TblAllowanceDetails
                    .Where(w => w.AllowanceCode == code)
                    .ToListAsync();

            var jobCodes = allowanceDetails
                .Select(x => x.JobDetailCode)
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x) &&
                    x != "-")
                .Distinct()
                .ToList();

            if (jobCodes.Count > 0)
            {
                var driverCosts =
                    await db.TblDriverCosts
                        .Where(w =>
                            w.DriverCode ==
                            allowance.DriverCode &&
                            jobCodes.Contains(
                                w.JobDetailCode))
                        .ToListAsync();

                foreach (var driverCost in driverCosts)
                {
                    driverCost.IsWeeklypayment = false;
                }
            }

            if (allowanceDetails.Count > 0)
            {
                db.TblAllowanceDetails.RemoveRange(
                    allowanceDetails);
            }

            var specialValues =
                await db.TblDriverSpecialvalues
                    .Where(w =>
                        w.AllowanceCode == code)
                    .ToListAsync();

            if (specialValues.Count > 0)
            {
                db.TblDriverSpecialvalues.RemoveRange(
                    specialValues);
            }

            allowance.IsEnabled = false;
            allowance.UpdateDate = DateTime.Now;
            allowance.UpdateBy = username;

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "RemoveDriverPayment",
                code);
        }
    }

    public async Task<ResponseViewModel> RemoveDriverPaymentItem(
        int id,
        string code,
        string username)
    {
        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var allowanceDetail =
                await db.TblAllowanceDetails
                    .FirstOrDefaultAsync(
                        w =>
                            w.Id == id &&
                            w.AllowanceCode == code);

            if (allowanceDetail == null)
            {
                await tr.RollbackAsync();

                return Failure(
                    "ไม่พบรายการเบี้ยเลี้ยที่ต้องการลบ");
            }

            var allowance =
                await db.TblAllowances
                    .FirstOrDefaultAsync(
                        w => w.AllowanceCode == code);

            if (allowance == null)
            {
                await tr.RollbackAsync();
                return Failure("ไม่พบข้อมูลเบี้ยเลี้ย");
            }

            if (!string.IsNullOrWhiteSpace(
                    allowanceDetail.JobDetailCode) &&
                allowanceDetail.JobDetailCode != "-")
            {
                var driverCost =
                    await db.TblDriverCosts
                        .FirstOrDefaultAsync(
                            w =>
                                w.JobDetailCode ==
                                allowanceDetail.JobDetailCode &&
                                w.DriverCode ==
                                allowance.DriverCode);

                if (driverCost != null)
                {
                    driverCost.IsWeeklypayment = false;
                }
            }

            var remainingDetails =
                await db.TblAllowanceDetails
                    .AsNoTracking()
                    .Where(w =>
                        w.AllowanceCode == code &&
                        w.Id != id)
                    .ToListAsync();

            allowance.AllowanceTotal =
                remainingDetails.Sum(
                    x =>
                        x.TripValue +
                        x.FuelValue +
                        x.WasteValue +
                        x.OtherValue) +
                allowance.AdditionalTotal -
                allowance.SubtractTotal;

            allowance.ExchangeTotal =
                remainingDetails
                    .Where(x => x.TripType == "B")
                    .Sum(x => x.TripValue);

            allowance.UpdateDate = DateTime.Now;
            allowance.UpdateBy = username;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Remove payment.",
                    ActionName = AuditActions.Delete,
                    EntityName =
                        nameof(TblAllowanceDetail),
                    EntityId = id.ToString(),
                    Description =
                        $"ลบรายการจ่ายเบี้ยเลี้ย {id} จาก {code} โดย {username}",
                    OldValues =
                        GetScalarSnapshot(
                            allowanceDetail),
                    Result =
                        AuditResults.Success
                });

            db.TblAllowanceDetails.Remove(
                allowanceDetail);

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "RemoveDriverPaymentItem",
                code);
        }
    }

    public async Task<List<SupplierPaymentViewModel>> GetFleetAllowanceList(string month, string year)
    {
        var data = await (from a in db.TblFleetpayments
                          join d in db.TblTrucks on a.TruckCode equals d.TruckCode
                          join e in db.TblTruckDetails on d.TruckCode equals e.TruckCode
                          join c in db.TblSuppliers on d.SupplierCode equals c.SupplierCode
                          where a.IsEnabled == true &&
                              a.FleetpaymentMonth == month &&
                              a.FleetpaymentYear == year
                          select new SupplierPaymentViewModel
                          {
                              truck_license = d.TruckLicense + " [" + e.TruckNo + "]",
                              supplierpayment_code = a.FleetpaymentCode,
                              supplier_name = a.ReceiverName,
                              supplierpayment_date = a.CreateDate.ToString("dd-MM-yyyy"),
                              supplierpayment_amount = a.FleetpaymentAmount,
                              supplierpayment_net = a.FleetpaymentNetTotal
                          }).ToListAsync();
        return data;
    }

    public async Task<List<JobViewModel>> GetFleetJob(DateTime start, DateTime end, string truck)
    {
        var data = await (from a in db.TblJobDetails
                          join b in db.TblSupplierAllowances on a.JobDetailCode equals b.JobDetailCode
                          join c in db.TblJobMasters on a.JobCode equals c.JobCode
                          join e in db.TblTrucks on a.TruckCode equals e.TruckCode
                          join f in db.TblPlaces on a.MissionPlaceCode equals f.PlaceCode
                          join g in db.TblCustomers on c.CustomerCode equals g.CustomerCode
                          join i in db.TblPlaces on a.StartPlaceCode equals i.PlaceCode
                          where a.IsApprove == true &&
                             b.IsPayment == false &&
                             c.JobDate >= start &&
                             c.JobDate <= end &&
                             a.TruckCode == truck
                          select new JobViewModel
                          {
                              job_detail_code = a.JobDetailCode,
                              booking_no = c.BookingNo,
                              container_no = a.ContainerNo,
                              truck_license = e.TruckLicense,
                              start_name = i.PlaceName,
                              mission_name = f.PlaceName,
                              cost_value = b.CostValue,
                              job_date = c.JobDate.ToString("yyyy-MM-dd"),
                              customer_name = g.CustomerAcronym
                          }).ToListAsync();
        return data.OrderBy(o => o.job_date).ToList();
    }

    public async Task<ResponseViewModel> AddFleetPaymentData(TblFleetpayment model)
    {
        if (model == null)
        {
            return Failure("ไม่พบข้อมูล Fleet Payment");
        }

        var details = model.TblFleetpaymentDetails?.ToList()
            ?? new List<TblFleetpaymentDetail>();

        if (details.Count == 0)
        {
            return Failure("ไม่พบรายการงานสำหรับสร้าง Fleet Payment");
        }

        var duplicateJob = details
            .GroupBy(x => x.JobDetailCode)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateJob != null)
        {
            return Failure($"งาน {duplicateJob.Key} ถูกเลือกซ้ำ");
        }

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);

        try
        {
            var supplier = await (
                from truck in db.TblTrucks.AsNoTracking()
                join supplierRow in db.TblSuppliers.AsNoTracking()
                    on truck.SupplierCode equals supplierRow.SupplierCode
                where truck.TruckCode == model.TruckCode
                select supplierRow)
                .FirstOrDefaultAsync();

            if (supplier == null)
            {
                await tr.RollbackAsync();
                return Failure("ไม่พบข้อมูล Supplier ของรถที่เลือก");
            }

            model.FleetpaymentCode =
                await codeRepo.GetFleetPaymentCode();

            var jobCodes = details
                .Select(x => x.JobDetailCode)
                .Distinct()
                .ToList();

            var supplierAllowances = await db.TblSupplierAllowances
                .Where(w =>
                    w.SupplierCode == supplier.SupplierCode &&
                    jobCodes.Contains(w.JobDetailCode))
                .ToListAsync();

            var supplierAllowanceByJob = supplierAllowances
                .GroupBy(x => x.JobDetailCode)
                .ToDictionary(g => g.Key, g => g.First());

            var missingAllowance = jobCodes
                .FirstOrDefault(code =>
                    !supplierAllowanceByJob.ContainsKey(code));

            if (missingAllowance != null)
            {
                await tr.RollbackAsync();
                return Failure(
                    $"ไม่พบ Supplier Allowance ของงาน {missingAllowance}");
            }

            var alreadyPaid = supplierAllowances
                .FirstOrDefault(x => x.IsPayment == true);

            if (alreadyPaid != null)
            {
                await tr.RollbackAsync();
                return Failure(
                    $"งาน {alreadyPaid.JobDetailCode} ถูกนำไปทำ Fleet Payment แล้ว");
            }

            var exportJobCodes = await (
                from detail in db.TblJobDetails.AsNoTracking()
                join master in db.TblJobMasters.AsNoTracking()
                    on detail.JobCode equals master.JobCode
                where jobCodes.Contains(detail.JobDetailCode) &&
                      master.JobType == "EXPORT"
                select detail.JobDetailCode)
                .Distinct()
                .ToListAsync();

            var exportJobSet = exportJobCodes.ToHashSet();

            var driverCosts = await db.TblDriverCosts
                .AsNoTracking()
                .Where(w => jobCodes.Contains(w.JobDetailCode))
                .ToListAsync();

            var tripByJob = driverCosts
                .GroupBy(x => x.JobDetailCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.TripValue));

            var cashings = await db.TblJobcashings
                .AsNoTracking()
                .Where(w =>
                    jobCodes.Contains(w.JobDetailCode) &&
                    w.IsEnabled == true)
                .ToListAsync();

            var cashByJob = cashings
                .GroupBy(x => x.JobDetailCode)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Express = g.Sum(x => x.CashingExpresswayup),
                        Fuel = g.Sum(x => x.CashingFuelup)
                    });

            decimal shore = 0;

            foreach (var detail in details)
            {
                var jobCode = detail.JobDetailCode;
                var allowance = supplierAllowanceByJob[jobCode];

                allowance.IsPayment = true;

                if (exportJobSet.Contains(jobCode))
                {
                    shore += 50;
                }

                tripByJob.TryGetValue(
                    jobCode,
                    out var tripValue);

                cashByJob.TryGetValue(
                    jobCode,
                    out var cashValue);

                detail.TransportVatValue =
                    supplier.IsSpecialVat == true
                        ? 0
                        : detail.TransportValue * 1m / 100m;

                detail.TransportTotalValue =
                    detail.TransportValue -
                    detail.TransportVatValue;

                detail.TripValue = tripValue;
                detail.ExpressValue =
                    cashValue?.Express ?? 0;

                detail.FuelLitreValue =
                    cashValue?.Fuel ?? 0;

                detail.FuelTotalValue =
                    detail.FuelLitreValue *
                    model.FuelRate;

                detail.PaymentValue =
                    detail.TransportTotalValue -
                    detail.TripValue -
                    detail.FuelTotalValue -
                    detail.ExpressValue;

                detail.FleetpaymentCode =
                    model.FleetpaymentCode;
            }

            model.ReceiverName = supplier.SupplierFullname;
            model.ReceiverAddress = supplier.SupplierAddress;
            model.ReceiverTax = supplier.SupplierVatNo;
            model.FleetpaymentRemark ??= "-";
            model.ShoreTotal = shore;
            model.AreaTotal = 2500;
            model.IsEnabled = true;

            RecalculateFleetPaymentTotals(
                model,
                details);

            db.TblFleetpayments.Add(model);

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);
            return Error(
                ex,
                "AddFleetPaymentData",
                model.FleetpaymentCode);
        }
    }

    public async Task<ResponseViewModel> RemoveFleetPayment(
        string code,
        string username)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัส Fleet Payment");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var payment =
                await db.TblFleetpayments
                    .FirstOrDefaultAsync(
                        w =>
                            w.FleetpaymentCode ==
                            code);

            if (payment == null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"ไม่พบ Fleet Payment {code}");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync();
                return Failure("รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            var details =
                await db.TblFleetpaymentDetails
                    .Where(w =>
                        w.FleetpaymentCode ==
                        code)
                    .ToListAsync();

            var jobCodes = details
                .Select(x => x.JobDetailCode)
                .Distinct()
                .ToList();

            var supplierCode =
                await db.TblTrucks
                    .AsNoTracking()
                    .Where(w =>
                        w.TruckCode ==
                        payment.TruckCode)
                    .Select(w => w.SupplierCode)
                    .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(
                    supplierCode) &&
                jobCodes.Count > 0)
            {
                var supplierAllowances =
                    await db.TblSupplierAllowances
                        .Where(w =>
                            w.SupplierCode ==
                            supplierCode &&
                            jobCodes.Contains(
                                w.JobDetailCode))
                        .ToListAsync();

                foreach (var allowance in
                         supplierAllowances)
                {
                    allowance.IsPayment = false;
                }
            }

            if (details.Count > 0)
            {
                db.TblFleetpaymentDetails.RemoveRange(
                    details);
            }

            payment.IsEnabled = false;
            payment.UpdateDate = DateTime.Now;
            payment.UpdateBy = username;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Remove AVA payment.",
                    ActionName =
                        AuditActions.Delete,
                    EntityName =
                        nameof(TblFleetpayment),
                    EntityId =
                        code,
                    Description =
                        $"ลบ AVA Payment {code} โดย {username}",
                    OldValues =
                        oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "RemoveFleetPayment",
                code);
        }
    }

    public async Task<ResponseViewModel> EditFleetPaymentData(TblFleetpayment model)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.FleetpaymentCode))
        {
            return Failure("ไม่พบข้อมูล Fleet Payment");
        }

        var details = model.TblFleetpaymentDetails?.ToList()
            ?? new List<TblFleetpaymentDetail>();

        if (details.Count == 0)
        {
            return Failure("ไม่พบรายการงานสำหรับ Fleet Payment");
        }

        var duplicateJob = details
            .GroupBy(x => x.JobDetailCode)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateJob != null)
        {
            return Failure($"งาน {duplicateJob.Key} ถูกเลือกซ้ำ");
        }

        await using var tr = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);

        try
        {
            var payment = await db.TblFleetpayments
                .FirstOrDefaultAsync(
                    w => w.FleetpaymentCode ==
                         model.FleetpaymentCode);

            if (payment == null)
            {
                await tr.RollbackAsync();
                return Failure(
                    $"ไม่พบ Fleet Payment {model.FleetpaymentCode}");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync();
                return Failure("Fleet Payment รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var supplier = await (
                from truck in db.TblTrucks.AsNoTracking()
                join supplierRow in db.TblSuppliers.AsNoTracking()
                    on truck.SupplierCode equals supplierRow.SupplierCode
                where truck.TruckCode == model.TruckCode
                select supplierRow)
                .FirstOrDefaultAsync();

            if (supplier == null)
            {
                await tr.RollbackAsync();
                return Failure("ไม่พบข้อมูล Supplier ของรถที่เลือก");
            }

            var oldDetails = await db.TblFleetpaymentDetails
                .Where(w =>
                    w.FleetpaymentCode ==
                    model.FleetpaymentCode)
                .ToListAsync();

            var oldJobCodes = oldDetails
                .Select(x => x.JobDetailCode)
                .Distinct()
                .ToHashSet();

            var newJobCodes = details
                .Select(x => x.JobDetailCode)
                .Distinct()
                .ToList();

            var allJobCodes = oldJobCodes
                .Union(newJobCodes)
                .ToList();

            var supplierAllowances = await db.TblSupplierAllowances
                .Where(w =>
                    w.SupplierCode == supplier.SupplierCode &&
                    allJobCodes.Contains(w.JobDetailCode))
                .ToListAsync();

            var supplierAllowanceByJob = supplierAllowances
                .GroupBy(x => x.JobDetailCode)
                .ToDictionary(g => g.Key, g => g.First());

            var missingAllowance = newJobCodes
                .FirstOrDefault(code =>
                    !supplierAllowanceByJob.ContainsKey(code));

            if (missingAllowance != null)
            {
                await tr.RollbackAsync();
                return Failure(
                    $"ไม่พบ Supplier Allowance ของงาน {missingAllowance}");
            }

            var conflictingAllowance = supplierAllowances
                .FirstOrDefault(x =>
                    newJobCodes.Contains(x.JobDetailCode) &&
                    x.IsPayment == true &&
                    !oldJobCodes.Contains(x.JobDetailCode));

            if (conflictingAllowance != null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"งาน {conflictingAllowance.JobDetailCode} " +
                    "ถูกนำไปทำ Payment อื่นแล้ว");
            }

            foreach (var jobCode in oldJobCodes)
            {
                if (!newJobCodes.Contains(jobCode) &&
                    supplierAllowanceByJob.TryGetValue(
                        jobCode,
                        out var oldAllowance))
                {
                    oldAllowance.IsPayment = false;
                }
            }

            foreach (var jobCode in newJobCodes)
            {
                supplierAllowanceByJob[jobCode].IsPayment = true;
            }

            var exportJobCodes = await (
                from detail in db.TblJobDetails.AsNoTracking()
                join master in db.TblJobMasters.AsNoTracking()
                    on detail.JobCode equals master.JobCode
                where newJobCodes.Contains(detail.JobDetailCode) &&
                      master.JobType == "EXPORT"
                select detail.JobDetailCode)
                .Distinct()
                .ToListAsync();

            var exportJobSet =
                exportJobCodes.ToHashSet();

            var driverCosts = await db.TblDriverCosts
                .AsNoTracking()
                .Where(w =>
                    newJobCodes.Contains(
                        w.JobDetailCode))
                .ToListAsync();

            var tripByJob = driverCosts
                .GroupBy(x => x.JobDetailCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.TripValue));

            var cashings = await db.TblJobcashings
                .AsNoTracking()
                .Where(w =>
                    newJobCodes.Contains(
                        w.JobDetailCode) &&
                    w.IsEnabled == true)
                .ToListAsync();

            var cashByJob = cashings
                .GroupBy(x => x.JobDetailCode)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Express =
                            g.Sum(
                                x =>
                                    x.CashingExpresswayup),

                        Fuel =
                            g.Sum(
                                x =>
                                    x.CashingFuelup)
                    });

            decimal shore = 0;

            foreach (var detail in details)
            {
                var jobCode =
                    detail.JobDetailCode;

                if (exportJobSet.Contains(jobCode))
                {
                    shore += 50;
                }

                tripByJob.TryGetValue(
                    jobCode,
                    out var tripValue);

                cashByJob.TryGetValue(
                    jobCode,
                    out var cashValue);

                detail.TransportVatValue =
                    supplier.IsSpecialVat == true
                        ? 0
                        : detail.TransportValue *
                          1m / 100m;

                detail.TransportTotalValue =
                    detail.TransportValue -
                    detail.TransportVatValue;

                detail.TripValue =
                    tripValue;

                detail.ExpressValue =
                    cashValue?.Express ?? 0;

                detail.FuelLitreValue =
                    cashValue?.Fuel ?? 0;

                detail.FuelTotalValue =
                    detail.FuelLitreValue *
                    model.FuelRate;

                detail.PaymentValue =
                    detail.TransportTotalValue -
                    detail.TripValue -
                    detail.FuelTotalValue -
                    detail.ExpressValue;

                detail.FleetpaymentCode =
                    model.FleetpaymentCode;
            }

            var oldValues = new
            {
                FleetPayment =
                    GetScalarSnapshot(payment),

                Details =
                    oldDetails
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            CopyEditableScalarValues(
                payment,
                model,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    nameof(TblFleetpayment.FleetpaymentCode),
                    nameof(TblFleetpayment.IsEnabled),
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            payment.ReceiverName =
                supplier.SupplierFullname;

            payment.ReceiverAddress =
                supplier.SupplierAddress;

            payment.ReceiverTax =
                supplier.SupplierVatNo;

            payment.FleetpaymentRemark ??= "-";
            payment.ShoreTotal = shore;
            payment.AreaTotal = 2500;

            RecalculateFleetPaymentTotals(
                payment,
                details);

            if (oldDetails.Count > 0)
            {
                db.TblFleetpaymentDetails.RemoveRange(
                    oldDetails);
            }

            db.TblFleetpaymentDetails.AddRange(
                details);

            var newValues = new
            {
                FleetPayment =
                    GetScalarSnapshot(payment),

                Details =
                    details
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Update AVA payment.",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblFleetpayment),
                    EntityId =
                        payment.FleetpaymentCode,
                    Description =
                        $"แก้ไขข้อมูล AVA Payment {payment.FleetpaymentCode} โดย {payment.UpdateBy}",
                    OldValues = oldValues,
                    NewValues = newValues,
                    Result = AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "EditFleetPaymentData",
                model.FleetpaymentCode);
        }
    }

    public async Task<List<JobViewModel>> GetListFleetJobByCode(string code)
    {
        var data = await (from a in db.TblJobDetails
                          join b in db.TblSupplierAllowances on a.JobDetailCode equals b.JobDetailCode
                          join c in db.TblJobMasters on a.JobCode equals c.JobCode
                          join e in db.TblTrucks on a.TruckCode equals e.TruckCode
                          join f in db.TblPlaces on a.MissionPlaceCode equals f.PlaceCode
                          join g in db.TblCustomers on c.CustomerCode equals g.CustomerCode
                          join h in db.TblFleetpaymentDetails on a.JobDetailCode equals h.JobDetailCode
                          join i in db.TblPlaces on a.StartPlaceCode equals i.PlaceCode
                          where h.FleetpaymentCode == code
                          select new JobViewModel
                          {
                              job_detail_code = a.JobDetailCode,
                              booking_no = c.BookingNo,
                              container_no = a.ContainerNo,
                              truck_license = e.TruckLicense,
                              start_name = i.PlaceName,
                              mission_name = f.PlaceName,
                              job_date = c.JobDate.ToString("yyyy-MM-dd"),
                              job_date_value = c.JobDate,
                              express_value = h.ExpressValue,
                              customer_name = g.CustomerAcronym,
                              transport_value = h.TransportValue,
                              trip_value = h.TripValue,
                              fuel_litre = h.FuelLitreValue,
                              fuel_value = h.FuelTotalValue,
                              payment_value = h.PaymentValue
                          }).ToListAsync();
        return data.OrderBy(o => o.job_date).ToList();
    }

    public async Task<FleetPaymentViewModel?> GetFleetPaymentByCode(string code)
    {
        var data = await (from a in db.TblFleetpayments
                          join b in db.TblTrucks on a.TruckCode equals b.TruckCode
                          join c in db.TblSuppliers on b.SupplierCode equals c.SupplierCode
                          where a.FleetpaymentCode == code
                          select new FleetPaymentViewModel
                          {
                              code = a.FleetpaymentCode,
                              supplier_name = b.TruckLicense + " [" + c.SupplierFullname + "]",
                              month = a.FleetpaymentMonth,
                              year = a.FleetpaymentYear,
                              salary = a.SalaryTotal,
                              shore = a.ShoreTotal,
                              area = a.AreaTotal,
                              maintenance = a.MaintenanceTotal,
                              net_total = a.FleetpaymentNetTotal
                          }).FirstOrDefaultAsync();

        return data;
    }

    public async Task<ResponseViewModel> EditFleetTransportCost(
        string code,
        int value,
        string user)
    {
        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var paymentDetail = await db.TblFleetpaymentDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code);

            if (paymentDetail == null)
            {
                await tr.RollbackAsync();
                return Failure(
                    $"ไม่พบรายการ Fleet Payment ของงาน {code}");
            }

            var payment = await db.TblFleetpayments
                .FirstOrDefaultAsync(
                    w => w.FleetpaymentCode ==
                         paymentDetail.FleetpaymentCode);

            if (payment == null)
            {
                await tr.RollbackAsync();
                return Failure("ไม่พบ Fleet Payment");
            }

            var supplier = await (
                from supplierRow in db.TblSuppliers.AsNoTracking()
                join truck in db.TblTrucks.AsNoTracking()
                    on supplierRow.SupplierCode
                    equals truck.SupplierCode
                where truck.TruckCode ==
                      payment.TruckCode
                select supplierRow)
                .FirstOrDefaultAsync();

            if (supplier == null)
            {
                await tr.RollbackAsync();
                return Failure("ไม่พบข้อมูล Supplier");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            paymentDetail.TransportValue = value;
            paymentDetail.TransportVatValue =
                supplier.IsSpecialVat == true
                    ? 0
                    : paymentDetail.TransportValue *
                      1m / 100m;

            paymentDetail.TransportTotalValue =
                paymentDetail.TransportValue -
                paymentDetail.TransportVatValue;

            paymentDetail.PaymentValue =
                paymentDetail.TransportTotalValue -
                paymentDetail.TripValue -
                paymentDetail.FuelTotalValue -
                paymentDetail.ExpressValue;

            var jobPayment = await db.TblSupplierAllowances
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code &&
                         w.SupplierCode == supplier.SupplierCode);

            if (jobPayment != null)
            {
                jobPayment.CostValue = value;
            }

            var details = await db.TblFleetpaymentDetails
                .Where(w =>
                    w.FleetpaymentCode ==
                    payment.FleetpaymentCode)
                .ToListAsync();

            RecalculateFleetPaymentTotals(
                payment,
                details);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Update AVA transport.",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblFleetpayment),
                    EntityId = payment.FleetpaymentCode,
                    Description =
                        $"แก้ไขค่าขนส่ง {payment.FleetpaymentCode} โดย {user}",
                    OldValues = oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result = AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "EditFleetTransportCost",
                code);
        }
    }

    public async Task<ResponseViewModel> EditFleetTripCost(
        string code,
        int value,
        string user)
    {
        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var paymentDetail = await db.TblFleetpaymentDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code);

            if (paymentDetail == null)
            {
                await tr.RollbackAsync();
                return Failure(
                    $"ไม่พบรายการ Fleet Payment ของงาน {code}");
            }

            var payment = await db.TblFleetpayments
                .FirstOrDefaultAsync(
                    w => w.FleetpaymentCode ==
                         paymentDetail.FleetpaymentCode);

            if (payment == null)
            {
                await tr.RollbackAsync();
                return Failure("ไม่พบ Fleet Payment");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            paymentDetail.TripValue = value;

            paymentDetail.PaymentValue =
                paymentDetail.TransportTotalValue -
                paymentDetail.TripValue -
                paymentDetail.FuelTotalValue -
                paymentDetail.ExpressValue;

            var jobTrip = await db.TblDriverCosts
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code);

            if (jobTrip != null)
            {
                jobTrip.TripValue = value;
            }

            var details = await db.TblFleetpaymentDetails
                .Where(w =>
                    w.FleetpaymentCode ==
                    payment.FleetpaymentCode)
                .ToListAsync();

            RecalculateFleetPaymentTotals(
                payment,
                details);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Update AVA trip.",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblFleetpayment),
                    EntityId = payment.FleetpaymentCode,
                    Description =
                        $"แก้ไขค่าเที่ยว {payment.FleetpaymentCode} โดย {user}",
                    OldValues = oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result = AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "EditFleetTripCost",
                code);
        }
    }

    public async Task<ResponseViewModel> EditFleetFuelCost(
        string code,
        int value,
        string user)
    {
        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var paymentDetail = await db.TblFleetpaymentDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code);

            if (paymentDetail == null)
            {
                await tr.RollbackAsync();
                return Failure(
                    $"ไม่พบรายการ Fleet Payment ของงาน {code}");
            }

            var payment = await db.TblFleetpayments
                .FirstOrDefaultAsync(
                    w => w.FleetpaymentCode ==
                         paymentDetail.FleetpaymentCode);

            if (payment == null)
            {
                await tr.RollbackAsync();
                return Failure("ไม่พบ Fleet Payment");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            paymentDetail.FuelLitreValue = value;
            paymentDetail.FuelTotalValue =
                paymentDetail.FuelLitreValue *
                payment.FuelRate;

            paymentDetail.PaymentValue =
                paymentDetail.TransportTotalValue -
                paymentDetail.TripValue -
                paymentDetail.FuelTotalValue -
                paymentDetail.ExpressValue;

            var jobFuel = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code);

            if (jobFuel != null)
            {
                // Fleet Payment ใช้ค่าที่อนุมัติแล้ว (...up)
                jobFuel.CashingFuelup = value;
            }

            var details = await db.TblFleetpaymentDetails
                .Where(w =>
                    w.FleetpaymentCode ==
                    payment.FleetpaymentCode)
                .ToListAsync();

            RecalculateFleetPaymentTotals(
                payment,
                details);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Update AVA fuel.",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblFleetpayment),
                    EntityId = payment.FleetpaymentCode,
                    Description =
                        $"แก้ไขค่าน้ำมัน {payment.FleetpaymentCode} โดย {user}",
                    OldValues = oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result = AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "EditFleetFuelCost",
                code);
        }
    }

    public async Task<ResponseViewModel> EditFleetExpressCost(
        string code,
        decimal value,
        string user)
    {
        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var paymentDetail = await db.TblFleetpaymentDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code);

            if (paymentDetail == null)
            {
                await tr.RollbackAsync();
                return Failure(
                    $"ไม่พบรายการ Fleet Payment ของงาน {code}");
            }

            var payment = await db.TblFleetpayments
                .FirstOrDefaultAsync(
                    w => w.FleetpaymentCode ==
                         paymentDetail.FleetpaymentCode);

            if (payment == null)
            {
                await tr.RollbackAsync();
                return Failure("ไม่พบ Fleet Payment");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            paymentDetail.ExpressValue = value;

            paymentDetail.PaymentValue =
                paymentDetail.TransportTotalValue -
                paymentDetail.TripValue -
                paymentDetail.FuelTotalValue -
                paymentDetail.ExpressValue;

            var jobExpress = await db.TblJobcashings
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code);

            if (jobExpress != null)
            {
                // Fleet Payment ใช้ค่าที่อนุมัติแล้ว (...up)
                jobExpress.CashingExpresswayup = value;
            }

            var details = await db.TblFleetpaymentDetails
                .Where(w =>
                    w.FleetpaymentCode ==
                    payment.FleetpaymentCode)
                .ToListAsync();

            RecalculateFleetPaymentTotals(
                payment,
                details);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName = "Update AVA express.",
                    ActionName = AuditActions.Update,
                    EntityName = nameof(TblFleetpayment),
                    EntityId = payment.FleetpaymentCode,
                    Description =
                        $"แก้ไขค่าทางด่วน {payment.FleetpaymentCode} โดย {user}",
                    OldValues = oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result = AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "EditFleetExpressCost",
                code);
        }
    }

    public async Task<TblFleetpayment?> GetFleetPaymentDataByCode(string code)
    {
        var data = await db.TblFleetpayments.AsNoTracking().FirstOrDefaultAsync(w => w.FleetpaymentCode == code);
        return data;
    }

    public async Task<List<SupplierPaymentViewModel>> GetSupplierList(string month, string year)
    {
        var m = int.Parse(month);
        var y = int.Parse(year);
        var startDate = new DateOnly(y, m, 1);
        var endDate = startDate.AddMonths(1);

        var data = await (from a in db.TblSupplierpayments
                          join b in db.TblSuppliers on a.SupplierCode equals b.SupplierCode
                          where a.IsEnabled == true &&
                                a.SupplierCode != "SP18001" &&
                                a.SupplierpaymentDate >= startDate &&
                                a.SupplierpaymentDate < endDate
                          select new SupplierPaymentViewModel
                          {
                              supplier_code = b.SupplierCode,
                              supplierpayment_code = a.SupplierpaymentCode,
                              supplier_name = b.SupplierName,
                              supplierpayment_date = a.SupplierpaymentDate.ToString("dd-MM-yyyy"),
                              supplierpayment_amount = a.SupplierpaymentAmount,
                              supplierpayment_net = a.SupplierpaymentNetTotal
                          }).ToListAsync();

        return data;
    }

    public async Task<List<JobViewModel>> GetSupplierJob(DateTime start, DateTime end, string supplyer)
    {
        var data = await (from a in db.TblJobDetails
                          join b in db.TblSupplierAllowances on a.JobDetailCode equals b.JobDetailCode
                          join c in db.TblJobMasters on a.JobCode equals c.JobCode
                          join f in db.TblPlaces on a.MissionPlaceCode equals f.PlaceCode
                          join g in db.TblCustomers on c.CustomerCode equals g.CustomerCode
                          join h in db.TblSuppliers on a.SupplierCode equals h.SupplierCode
                          join i in db.TblPlaces on a.StartPlaceCode equals i.PlaceCode
                          where a.IsApprove == true &&
                             b.IsPayment == false &&
                             a.SupplierCode == supplyer &&
                             c.JobDate >= start &&
                             c.JobDate <= end
                          select new JobViewModel
                          {
                              job_detail_code = a.JobDetailCode,
                              job_invoice = a.SupplierInvoice,
                              booking_no = c.BookingNo,
                              container_no = a.ContainerNo,
                              start_name = i.PlaceName,
                              mission_name = f.PlaceName,
                              cost_value = b.CostValue,
                              job_date = c.JobDate.ToString("yyyy-MM-dd"),
                              customer_name = g.CustomerAcronym,
                              supplier_name = h.SupplierName
                          }).ToListAsync();
        return data.OrderBy(o => o.job_date).ToList();
    }

    public async Task<ResponseViewModel> AddSupplierPaymentData(
        TblSupplierpayment model)
    {
        if (model == null)
        {
            return Failure("ไม่พบข้อมูล Supplier Payment");
        }

        var details =
            model.TblSupplierpaymentDetails?.ToList()
            ?? new List<TblSupplierpaymentDetail>();

        if (details.Count == 0)
        {
            return Failure(
                "ไม่พบรายการงานสำหรับสร้าง Supplier Payment");
        }

        var duplicateJob = details
            .GroupBy(x => x.JobDetailCode)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateJob != null)
        {
            return Failure(
                $"งาน {duplicateJob.Key} ถูกเลือกซ้ำ");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            model.SupplierpaymentCode =
                await codeRepo.GetSupplyerPaymentCode();

            var jobCodes = details
                .Select(x => x.JobDetailCode)
                .Distinct()
                .ToList();

            var supplierAllowances =
                await db.TblSupplierAllowances
                    .Where(w =>
                        jobCodes.Contains(
                            w.JobDetailCode))
                    .ToListAsync();

            var allowancesByJob =
                supplierAllowances
                    .GroupBy(x => x.JobDetailCode)
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToList());

            var selectedAllowances =
                new List<TblSupplierAllowance>(
                    jobCodes.Count);

            foreach (var jobCode in jobCodes)
            {
                if (!allowancesByJob.TryGetValue(
                        jobCode,
                        out var jobAllowances) ||
                    jobAllowances.Count == 0)
                {
                    await tr.RollbackAsync();

                    return Failure(
                        $"ไม่พบ Supplier Allowance ของงาน {jobCode}");
                }

                TblSupplierAllowance? allowance;

                if (!string.IsNullOrWhiteSpace(
                        model.SupplierCode))
                {
                    allowance =
                        jobAllowances.FirstOrDefault(
                            x =>
                                x.SupplierCode ==
                                model.SupplierCode);
                }
                else
                {
                    allowance =
                        jobAllowances.Count == 1
                            ? jobAllowances[0]
                            : null;
                }

                if (allowance == null)
                {
                    await tr.RollbackAsync();

                    return Failure(
                        $"ไม่สามารถระบุ Supplier ของงาน {jobCode} ได้ชัดเจน");
                }

                selectedAllowances.Add(allowance);
            }

            var supplierCodes = selectedAllowances
                .Select(x => x.SupplierCode)
                .Distinct()
                .ToList();

            if (supplierCodes.Count != 1)
            {
                await tr.RollbackAsync();

                return Failure(
                    "งานที่เลือกต้องเป็น Supplier รายเดียวกันทั้งหมด");
            }

            var alreadyPaid =
                selectedAllowances
                    .FirstOrDefault(
                        x => x.IsPayment == true);

            if (alreadyPaid != null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"งาน {alreadyPaid.JobDetailCode} ถูกนำไปทำ Supplier Payment แล้ว");
            }

            model.SupplierCode =
                supplierCodes[0];

            var allowanceByJob =
                selectedAllowances.ToDictionary(
                    x => x.JobDetailCode);

            var advances = await db.TblJobadvances
                .AsNoTracking()
                .Where(w =>
                    jobCodes.Contains(
                        w.JobDetailCode))
                .ToListAsync();

            var advanceByJob = advances
                .GroupBy(x => x.JobDetailCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.First());

            var missingAdvance = jobCodes
                .FirstOrDefault(
                    code =>
                        !advanceByJob.ContainsKey(
                            code));

            if (missingAdvance != null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"ไม่พบ Advance ของงาน {missingAdvance}");
            }

            var advanceCodes = advances
                .Select(x => x.AdvanceCode)
                .Distinct()
                .ToList();

            var advanceOthers =
                await db.TblJobadvanceOthers
                    .AsNoTracking()
                    .Where(w =>
                        advanceCodes.Contains(
                            w.AdvanceCode))
                    .ToListAsync();

            var additionalByAdvance =
                advanceOthers
                    .GroupBy(x => x.AdvanceCode)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                            g.Sum(
                                x =>
                                    x.SupplierOtherValue));

            foreach (var detail in details)
            {
                var jobCode =
                    detail.JobDetailCode;

                var advance =
                    advanceByJob[jobCode];

                var supplierAllowance =
                    allowanceByJob[jobCode];

                supplierAllowance.IsPayment = true;

                additionalByAdvance.TryGetValue(
                    advance.AdvanceCode,
                    out var additionalCharge);

                detail.SupplierpaymentCode =
                    model.SupplierpaymentCode;

                detail.PortValue =
                    model.IsPort == true
                        ? advance.AdvancePortup
                        : 0;

                detail.YardValue =
                    model.IsYard == true
                        ? advance.AdvanceYardup
                        : 0;

                detail.LiftonValue =
                    model.IsLifton == true
                        ? advance.AdvanceLiftonup
                        : 0;

                // ระบบเดิมยังไม่มี flag สำหรับ Liftoff ใน model
                detail.LiftoffValue = 0;

                detail.AdditionalchargeValue =
                    additionalCharge;

                detail.AdvanceValue =
                    detail.PortValue +
                    detail.YardValue +
                    detail.LiftonValue +
                    detail.LiftoffValue;

                detail.TotalValue =
                    detail.SupplierpaymentValue +
                    detail.AdvanceValue +
                    detail.AdditionalchargeValue;
            }

            RecalculateSupplierPaymentTotals(
                model,
                details);

            model.IsEnabled = true;

            db.TblSupplierpayments.Add(model);

            var now = DateTime.Now;

            var pnd =
                new TblWithholdingTax
                {
                    WithholdingTaxCode =
                        await codeRepo.GetTaxPaymentCode(),

                    WithholdingTaxDate =
                        model.SupplierpaymentDate,

                    PaymentTypeCode = "05",

                    SupplierCode =
                        model.SupplierCode,

                    PaymentTotal =
                        model.TransportTotal,

                    TaxTotal =
                        model.TransportVat,

                    PaymentNetTotal =
                        model.TransportNet,

                    IsEnabled = true,
                    CreateDate = now,
                    CreateBy = model.CreateBy,
                    UpdateDate = now,
                    UpdateBy = model.CreateBy
                };

            db.TblWithholdingTaxes.Add(pnd);

            db.TblWithholdingTaxSupplierpayments.Add(
                new TblWithholdingTaxSupplierpayment
                {
                    WithholdingTaxCode =
                        pnd.WithholdingTaxCode,

                    SupplierpaymentCode =
                        model.SupplierpaymentCode,

                    CreateDate = now,
                    CreateBy = model.CreateBy
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "AddSupplierPaymentData",
                model.SupplierpaymentCode);
        }
    }

    public async Task<TblSupplierpayment?> GetSupplyerPaymentDataByCode(string code)
    {
        var model = await db.TblSupplierpayments.AsNoTracking().FirstOrDefaultAsync(w => w.SupplierpaymentCode == code);
        return model;
    }

    public async Task<List<JobViewModel>> GetSupplierJobByCode(string code)
    {
        var data = await (from a in db.TblJobDetails
                          join b in db.TblSupplierAllowances on a.JobDetailCode equals b.JobDetailCode
                          join c in db.TblJobMasters on a.JobCode equals c.JobCode
                          join e in db.TblTrucks on a.TruckCode equals e.TruckCode
                          join f in db.TblPlaces on a.MissionPlaceCode equals f.PlaceCode
                          join g in db.TblCustomers on c.CustomerCode equals g.CustomerCode
                          join h in db.TblSuppliers on a.SupplierCode equals h.SupplierCode
                          join i in db.TblPlaces on a.StartPlaceCode equals i.PlaceCode
                          join j in db.TblSupplierpaymentDetails on a.JobDetailCode equals j.JobDetailCode
                          where j.SupplierpaymentCode == code
                          orderby c.JobDate
                          select new JobViewModel
                          {
                              job_detail_code = a.JobDetailCode,
                              job_invoice = a.SupplierInvoice,
                              booking_no = c.BookingNo,
                              container_no = a.ContainerNo,
                              truck_license = e.TruckLicense,
                              start_name = i.PlaceName,
                              mission_name = f.PlaceName,
                              job_date_value = c.JobDate,
                              job_date = c.JobDate.ToString("yyyy-MM-dd"),
                              customer_name = g.CustomerAcronym,
                              supplier_name = h.SupplierName,
                              cost_value = b.CostValue,
                              advance_value = j.AdvanceValue
                          }).ToListAsync();
        return data;
    }

    public async Task<ResponseViewModel> EditSupplierPaymentData(
        TblSupplierpayment model)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(
                model.SupplierpaymentCode))
        {
            return Failure(
                "ไม่พบข้อมูล Supplier Payment");
        }

        var details =
            model.TblSupplierpaymentDetails?.ToList()
            ?? new List<TblSupplierpaymentDetail>();

        if (details.Count == 0)
        {
            return Failure(
                "ไม่พบรายการงานสำหรับ Supplier Payment");
        }

        var duplicateJob = details
            .GroupBy(x => x.JobDetailCode)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateJob != null)
        {
            return Failure(
                $"งาน {duplicateJob.Key} ถูกเลือกซ้ำ");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var payment =
                await db.TblSupplierpayments
                    .FirstOrDefaultAsync(
                        w =>
                            w.SupplierpaymentCode ==
                            model.SupplierpaymentCode);

            if (payment == null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"ไม่พบ Supplier Payment {model.SupplierpaymentCode}");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync();

                return Failure(
                    "Supplier Payment รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldDetails =
                await db.TblSupplierpaymentDetails
                    .Where(w =>
                        w.SupplierpaymentCode ==
                        model.SupplierpaymentCode)
                    .ToListAsync();

            var oldValues = new
            {
                SupplierPayment =
                    GetScalarSnapshot(payment),

                Details =
                    oldDetails
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            var oldJobCodes = oldDetails
                .Select(x => x.JobDetailCode)
                .Distinct()
                .ToHashSet();

            var newJobCodes = details
                .Select(x => x.JobDetailCode)
                .Distinct()
                .ToList();

            var allJobCodes = oldJobCodes
                .Union(newJobCodes)
                .ToList();

            var supplierAllowances =
                await db.TblSupplierAllowances
                    .Where(w =>
                        allJobCodes.Contains(
                            w.JobDetailCode))
                    .ToListAsync();

            var targetSupplierCode =
                !string.IsNullOrWhiteSpace(
                    model.SupplierCode)
                    ? model.SupplierCode
                    : payment.SupplierCode;

            var supplierAllowanceByJob =
                supplierAllowances
                    .Where(x =>
                        x.SupplierCode ==
                        targetSupplierCode)
                    .GroupBy(x => x.JobDetailCode)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First());

            var missingAllowance =
                newJobCodes.FirstOrDefault(
                    code =>
                        !supplierAllowanceByJob
                            .ContainsKey(code));

            if (missingAllowance != null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"ไม่พบ Supplier Allowance ของงาน {missingAllowance}");
            }

            var conflict =
                supplierAllowanceByJob.Values
                    .FirstOrDefault(
                        x =>
                            x.IsPayment == true &&
                            !oldJobCodes.Contains(
                                x.JobDetailCode));

            if (conflict != null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"งาน {conflict.JobDetailCode} ถูกนำไปทำ Payment อื่นแล้ว");
            }

            foreach (var oldJobCode in oldJobCodes)
            {
                if (!newJobCodes.Contains(
                        oldJobCode))
                {
                    var oldAllowance =
                        supplierAllowances
                            .FirstOrDefault(
                                x =>
                                    x.JobDetailCode ==
                                    oldJobCode &&
                                    x.SupplierCode ==
                                    payment.SupplierCode);

                    if (oldAllowance != null)
                    {
                        oldAllowance.IsPayment =
                            false;
                    }
                }
            }

            foreach (var newJobCode in newJobCodes)
            {
                supplierAllowanceByJob[
                    newJobCode].IsPayment = true;
            }

            var advances = await db.TblJobadvances
                .AsNoTracking()
                .Where(w =>
                    newJobCodes.Contains(
                        w.JobDetailCode))
                .ToListAsync();

            var advanceByJob = advances
                .GroupBy(x => x.JobDetailCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.First());

            var missingAdvance =
                newJobCodes.FirstOrDefault(
                    code =>
                        !advanceByJob.ContainsKey(
                            code));

            if (missingAdvance != null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"ไม่พบ Advance ของงาน {missingAdvance}");
            }

            var advanceCodes = advances
                .Select(x => x.AdvanceCode)
                .Distinct()
                .ToList();

            var advanceOthers =
                await db.TblJobadvanceOthers
                    .AsNoTracking()
                    .Where(w =>
                        advanceCodes.Contains(
                            w.AdvanceCode))
                    .ToListAsync();

            var additionalByAdvance =
                advanceOthers
                    .GroupBy(x => x.AdvanceCode)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                            g.Sum(
                                x =>
                                    x.SupplierOtherValue));

            foreach (var detail in details)
            {
                var jobCode =
                    detail.JobDetailCode;

                var advance =
                    advanceByJob[jobCode];

                additionalByAdvance.TryGetValue(
                    advance.AdvanceCode,
                    out var additionalCharge);

                detail.SupplierpaymentCode =
                    model.SupplierpaymentCode;

                detail.PortValue =
                    model.IsPort == true
                        ? advance.AdvancePortup
                        : 0;

                detail.YardValue =
                    model.IsYard == true
                        ? advance.AdvanceYardup
                        : 0;

                detail.LiftonValue =
                    model.IsLifton == true
                        ? advance.AdvanceLiftonup
                        : 0;

                detail.LiftoffValue = 0;

                detail.AdditionalchargeValue =
                    additionalCharge;

                detail.AdvanceValue =
                    detail.PortValue +
                    detail.YardValue +
                    detail.LiftonValue +
                    detail.LiftoffValue;

                detail.TotalValue =
                    detail.SupplierpaymentValue +
                    detail.AdvanceValue +
                    detail.AdditionalchargeValue;
            }

            CopyEditableScalarValues(
                payment,
                model,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    nameof(TblSupplierpayment.SupplierpaymentCode),
                    nameof(TblSupplierpayment.SupplierCode),
                    nameof(TblSupplierpayment.IsEnabled),
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            payment.SupplierCode =
                targetSupplierCode;

            RecalculateSupplierPaymentTotals(
                payment,
                details);

            if (oldDetails.Count > 0)
            {
                db.TblSupplierpaymentDetails.RemoveRange(
                    oldDetails);
            }

            db.TblSupplierpaymentDetails.AddRange(
                details);

            await SynchronizeSupplierWithholdingTaxAsync(
                payment,
                model.UpdateBy);

            var newValues = new
            {
                SupplierPayment =
                    GetScalarSnapshot(payment),

                Details =
                    details
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Update supplier payment.",

                    ActionName =
                        AuditActions.Update,

                    EntityName =
                        nameof(TblSupplierpayment),

                    EntityId =
                        payment.SupplierpaymentCode,

                    Description =
                        $"แก้ไข Supplier Payment {payment.SupplierpaymentCode} โดย {payment.UpdateBy}",

                    OldValues =
                        oldValues,

                    NewValues =
                        newValues,

                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "EditSupplierPaymentData",
                model.SupplierpaymentCode);
        }
    }

    public async Task<ResponseViewModel> EditSupplierTripCost(
        string code,
        decimal value,
        string username)
    {
        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var paymentDetail =
                await db.TblSupplierpaymentDetails
                    .FirstOrDefaultAsync(
                        w => w.JobDetailCode == code);

            if (paymentDetail == null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"ไม่พบ Supplier Payment ของงาน {code}");
            }

            var payment =
                await db.TblSupplierpayments
                    .FirstOrDefaultAsync(
                        w =>
                            w.SupplierpaymentCode ==
                            paymentDetail.SupplierpaymentCode);

            if (payment == null)
            {
                await tr.RollbackAsync();
                return Failure("ไม่พบ Supplier Payment");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            var cost =
                await db.TblSupplierAllowances
                    .FirstOrDefaultAsync(
                        w => w.JobDetailCode == code &&
                             w.SupplierCode ==
                             payment.SupplierCode);

            if (cost != null)
            {
                cost.CostValue = value;
            }

            paymentDetail.SupplierpaymentValue =
                value;

            paymentDetail.TotalValue =
                paymentDetail.SupplierpaymentValue +
                paymentDetail.AdvanceValue +
                paymentDetail.AdditionalchargeValue;

            var details =
                await db.TblSupplierpaymentDetails
                    .Where(w =>
                        w.SupplierpaymentCode ==
                        payment.SupplierpaymentCode)
                    .ToListAsync();

            RecalculateSupplierPaymentTotals(
                payment,
                details);

            await SynchronizeSupplierWithholdingTaxAsync(
                payment,
                username);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Update supplier trip.",
                    ActionName =
                        AuditActions.Update,
                    EntityName =
                        nameof(TblSupplierpayment),
                    EntityId =
                        payment.SupplierpaymentCode,
                    Description =
                        $"แก้ไขข้อมูลค่าเที่ยว Supplier {payment.SupplierpaymentCode} โดย {username}",
                    OldValues =
                        oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "EditSupplierTripCost",
                code);
        }
    }

    public async Task<ResponseViewModel> EditSupplierInvoiceJob(
        string code,
        string value,
        string username)
    {
        try
        {
            var job = await db.TblJobDetails
                .FirstOrDefaultAsync(
                    w => w.JobDetailCode == code);

            if (job == null)
            {
                return Failure($"ไม่พบงาน {code}");
            }

            var oldValues =
                GetScalarSnapshot(job);

            job.SupplierInvoice = value;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Update supplier invoice.",
                    ActionName =
                        AuditActions.Update,
                    EntityName =
                        nameof(TblJobDetail),
                    EntityId = code,
                    Description =
                        $"แก้ไข Supplier Invoice ของงาน {code} โดย {username}",
                    OldValues =
                        oldValues,
                    NewValues =
                        GetScalarSnapshot(job),
                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "EditSupplierInvoiceJob",
                code);
        }
    }

    public async Task<ResponseViewModel> EditSupplierAdvanceCost(
        string code,
        decimal value,
        string username)
    {
        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var paymentDetail =
                await db.TblSupplierpaymentDetails
                    .FirstOrDefaultAsync(
                        w => w.JobDetailCode == code);

            if (paymentDetail == null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"ไม่พบ Supplier Payment ของงาน {code}");
            }

            var payment =
                await db.TblSupplierpayments
                    .FirstOrDefaultAsync(
                        w =>
                            w.SupplierpaymentCode ==
                            paymentDetail.SupplierpaymentCode);

            if (payment == null)
            {
                await tr.RollbackAsync();
                return Failure("ไม่พบ Supplier Payment");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            paymentDetail.AdvanceValue = value;

            paymentDetail.TotalValue =
                paymentDetail.SupplierpaymentValue +
                paymentDetail.AdvanceValue +
                paymentDetail.AdditionalchargeValue;

            var details =
                await db.TblSupplierpaymentDetails
                    .Where(w =>
                        w.SupplierpaymentCode ==
                        payment.SupplierpaymentCode)
                    .ToListAsync();

            RecalculateSupplierPaymentTotals(
                payment,
                details);

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Update supplier advance.",
                    ActionName =
                        AuditActions.Update,
                    EntityName =
                        nameof(TblSupplierpayment),
                    EntityId =
                        payment.SupplierpaymentCode,
                    Description =
                        $"แก้ไขข้อมูล Advance Supplier {payment.SupplierpaymentCode} โดย {username}",
                    OldValues =
                        oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "EditSupplierAdvanceCost",
                code);
        }
    }

    public async Task<ResponseViewModel> RemoveSupplierPayment(
        string code,
        string username)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัส Supplier Payment");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var payment =
                await db.TblSupplierpayments
                    .FirstOrDefaultAsync(
                        w =>
                            w.SupplierpaymentCode ==
                            code);

            if (payment == null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"ไม่พบ Supplier Payment {code}");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync();
                return Failure("รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            var details =
                await db.TblSupplierpaymentDetails
                    .Where(w =>
                        w.SupplierpaymentCode ==
                        code)
                    .ToListAsync();

            var jobCodes =
                details
                    .Select(x => x.JobDetailCode)
                    .Distinct()
                    .ToList();

            if (jobCodes.Count > 0)
            {
                var allowances =
                    await db.TblSupplierAllowances
                        .Where(w =>
                            w.SupplierCode ==
                            payment.SupplierCode &&
                            jobCodes.Contains(
                                w.JobDetailCode))
                        .ToListAsync();

                foreach (var allowance in allowances)
                {
                    allowance.IsPayment = false;
                }
            }

            if (details.Count > 0)
            {
                db.TblSupplierpaymentDetails.RemoveRange(
                    details);
            }

            var taxPayment =
                await db.TblWithholdingTaxSupplierpayments
                    .FirstOrDefaultAsync(
                        w =>
                            w.SupplierpaymentCode ==
                            code);

            if (taxPayment != null)
            {
                var tax =
                    await db.TblWithholdingTaxes
                        .FirstOrDefaultAsync(
                            w =>
                                w.WithholdingTaxCode ==
                                taxPayment.WithholdingTaxCode);

                if (tax != null)
                {
                    tax.IsEnabled = false;
                    tax.UpdateDate = DateTime.Now;
                    tax.UpdateBy = username;
                }

                db.TblWithholdingTaxSupplierpayments.Remove(
                    taxPayment);
            }

            payment.IsEnabled = false;
            payment.UpdateDate = DateTime.Now;
            payment.UpdateBy = username;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Remove supplier payment.",
                    ActionName =
                        AuditActions.Delete,
                    EntityName =
                        nameof(TblSupplierpayment),
                    EntityId =
                        code,
                    Description =
                        $"ลบ Supplier Payment {code} โดย {username}",
                    OldValues =
                        oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "RemoveSupplierPayment",
                code);
        }
    }

    public async Task<List<PaymentModel>> GetDailyPaymentDataList(string month, string year, string username)
    {
        var m = int.Parse(month);
        var y = int.Parse(year);
        var startDate = new DateOnly(y, m, 1);
        var endDate = startDate.AddMonths(1);

        return await (from a in db.TblDailypayments
                      where a.CreateBy == username &&
                            a.PaymentDate >= startDate &&
                            a.PaymentDate < endDate &&
                            a.IsEnabled == true
                      select new PaymentModel
                      {
                          payment_date = a.PaymentDate.ToString("yyyy-MM-dd"),
                          payment_id = a.DailypaymentCode,
                          total = a.NetTotal,
                          approve_status = a.ApproveStatus
                      }).ToListAsync();
    }

    public async Task<List<PaymentModel>> GetAllDailyPaymentDataList(string month, string year)
    {
        var m = int.Parse(month);
        var y = int.Parse(year);
        var startDate = new DateOnly(y, m, 1);
        var endDate = startDate.AddMonths(1);

        return await (from a in db.TblDailypayments
                      where a.PaymentDate >= startDate &&
                            a.PaymentDate < endDate &&
                            a.IsEnabled == true
                      select new PaymentModel
                      {
                          payment_date = a.PaymentDate.ToString("yyyy-MM-dd"),
                          payment_id = a.DailypaymentCode,
                          payment_name = a.CreateBy,
                          total = a.NetTotal,
                          approve_status = a.ApproveStatus,
                      }).ToListAsync();
    }

    public async Task<List<PaymentModel>> GetMonthlyPaymentDataList(string month, string year)
    {
        var m = int.Parse(month);
        var y = int.Parse(year);
        var startDate = new DateOnly(y, m, 1);
        var endDate = startDate.AddMonths(1);

        return await (from a in db.TblMonthlypayments
                      join b in db.TblSuppliers on a.SupplierCode equals b.SupplierCode
                      where a.PaymentDate >= startDate &&
                            a.PaymentDate < endDate &&
                            a.IsEnabled == true
                      select new PaymentModel
                      {
                          payment_date = a.PaymentDate.ToString("yyyy-MM-dd"),
                          payment_id = a.MonthlyPaymentCode,
                          payment_name = b.SupplierName,
                          total = a.NetTotal
                      }).ToListAsync();
    }

    public async Task<TblDailypayment?> GetDailyPaymentData(string code)
    {
        return await db.TblDailypayments
            .AsNoTracking()
            .Include(i => i.TblDailypaymentDetails)
            .FirstOrDefaultAsync(w => w.DailypaymentCode == code);
    }

    public async Task<TblMonthlypayment?> GetMonthlyPaymentData(string code)
    {
        return await db.TblMonthlypayments
            .AsNoTracking()
            .Include(i => i.SupplierCodeNavigation)
            .Include(i => i.TblMonthlypaymentDetails)
            .FirstOrDefaultAsync(w => w.MonthlyPaymentCode == code);
    }

    public async Task<ResponseViewModel> AddDailyPayment(
        TblDailypayment model)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.DailypaymentCode))
        {
            return Failure(
                "ไม่พบข้อมูลหรือรหัสการจ่ายเงินรายวัน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var exists =
                await db.TblDailypayments
                    .AsNoTracking()
                    .AnyAsync(
                        w =>
                            w.DailypaymentCode ==
                            model.DailypaymentCode);

            if (exists)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"รหัส {model.DailypaymentCode} มีอยู่ในระบบแล้ว");
            }

            var details =
                model.TblDailypaymentDetails?.ToList()
                ?? new List<TblDailypaymentDetail>();

            foreach (var detail in details)
            {
                detail.DailypaymentCode =
                    model.DailypaymentCode;
            }

            model.IsEnabled = true;

            db.TblDailypayments.Add(model);

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "AddDailyPayment",
                model.DailypaymentCode);
        }
    }

    public async Task<ResponseViewModel> AddMonthlyPayment(
        TblMonthlypayment model)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.MonthlyPaymentCode))
        {
            return Failure(
                "ไม่พบข้อมูลหรือรหัสการจ่ายเงินรายเดือน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var exists =
                await db.TblMonthlypayments
                    .AsNoTracking()
                    .AnyAsync(
                        w =>
                            w.MonthlyPaymentCode ==
                            model.MonthlyPaymentCode);

            if (exists)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"รหัส {model.MonthlyPaymentCode} มีอยู่ในระบบแล้ว");
            }

            var details =
                model.TblMonthlypaymentDetails?.ToList()
                ?? new List<TblMonthlypaymentDetail>();

            foreach (var detail in details)
            {
                detail.MonthlyPaymentCode =
                    model.MonthlyPaymentCode;
            }

            model.IsEnabled = true;

            if (model.IsTax == true)
            {
                var now = DateTime.Now;

                var tax =
                    new TblWithholdingTax
                    {
                        WithholdingTaxCode =
                            await codeRepo.GetTaxPaymentCode(),

                        WithholdingTaxDate =
                            model.PaymentDate,

                        PaymentTypeCode =
                            model.PaymentTypeCode,

                        SupplierCode =
                            model.SupplierCode,

                        PaymentTotal =
                            model.Total,

                        TaxTotal =
                            model.Tax,

                        PaymentNetTotal =
                            model.Total -
                            model.Tax,

                        IsEnabled = true,
                        CreateDate = now,
                        CreateBy = model.CreateBy,
                        UpdateDate = now,
                        UpdateBy = model.CreateBy
                    };

                db.TblWithholdingTaxes.Add(
                    tax);

                db.TblWithholdingTaxPayments.Add(
                    new TblWithholdingTaxPayment
                    {
                        WithholdingTaxCode =
                            tax.WithholdingTaxCode,

                        MonthlyPaymentCode =
                            model.MonthlyPaymentCode,

                        CreateDate = now,
                        CreateBy = model.CreateBy
                    });
            }

            db.TblMonthlypayments.Add(model);

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "AddMonthlyPayment",
                model.MonthlyPaymentCode);
        }
    }

    public async Task<ResponseViewModel> EditDailyPayment(
        TblDailypayment model)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.DailypaymentCode))
        {
            return Failure("ไม่พบข้อมูลการจ่ายเงินรายวัน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var payment =
                await db.TblDailypayments
                    .FirstOrDefaultAsync(
                        w =>
                            w.DailypaymentCode ==
                            model.DailypaymentCode);

            if (payment == null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"ไม่พบรายการ {model.DailypaymentCode}");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync();
                return Failure("รายการนี้ถูกยกเลิกไปแล้ว");
            }

            if (payment.ApproveStatus == "A" ||
                payment.IsPaid == true)
            {
                await tr.RollbackAsync();

                return Failure(
                    "รายการที่อนุมัติหรือจ่ายแล้วไม่สามารถแก้ไขได้");
            }

            var oldDetails =
                await db.TblDailypaymentDetails
                    .AsNoTracking()
                    .Where(w =>
                        w.DailypaymentCode ==
                        model.DailypaymentCode)
                    .ToListAsync();

            var oldValues = new
            {
                Payment =
                    GetScalarSnapshot(payment),

                Details =
                    oldDetails
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            CopyEditableScalarValues(
                payment,
                model,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    nameof(TblDailypayment.DailypaymentCode),
                    nameof(TblDailypayment.IsEnabled),
                    nameof(TblDailypayment.IsPaid),
                    nameof(TblDailypayment.ApproveStatus),
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            await db.TblDailypaymentDetails
                .Where(w =>
                    w.DailypaymentCode ==
                    model.DailypaymentCode)
                .ExecuteDeleteAsync();

            var newDetails =
                model.TblDailypaymentDetails?.ToList()
                ?? new List<TblDailypaymentDetail>();

            foreach (var detail in newDetails)
            {
                detail.DailypaymentCode =
                    model.DailypaymentCode;
            }

            if (newDetails.Count > 0)
            {
                db.TblDailypaymentDetails.AddRange(
                    newDetails);
            }

            var newValues = new
            {
                Payment =
                    GetScalarSnapshot(payment),

                Details =
                    newDetails
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Update daily payment.",
                    ActionName =
                        AuditActions.Update,
                    EntityName =
                        nameof(TblDailypayment),
                    EntityId =
                        model.DailypaymentCode,
                    Description =
                        $"แก้ไขการจ่ายเงินรายวัน {model.DailypaymentCode} โดย {model.UpdateBy}",
                    OldValues =
                        oldValues,
                    NewValues =
                        newValues,
                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "EditDailyPayment",
                model.DailypaymentCode);
        }
    }

    public async Task<ResponseViewModel> EditMonthlyPayment(
        TblMonthlypayment model)
    {
        if (model == null ||
            string.IsNullOrWhiteSpace(model.MonthlyPaymentCode))
        {
            return Failure("ไม่พบข้อมูลการจ่ายเงินรายเดือน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var payment =
                await db.TblMonthlypayments
                    .FirstOrDefaultAsync(
                        w =>
                            w.MonthlyPaymentCode ==
                            model.MonthlyPaymentCode);

            if (payment == null)
            {
                await tr.RollbackAsync();

                return Failure(
                    $"ไม่พบรายการ {model.MonthlyPaymentCode}");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync();
                return Failure("รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldDetails =
                await db.TblMonthlypaymentDetails
                    .AsNoTracking()
                    .Where(w =>
                        w.MonthlyPaymentCode ==
                        model.MonthlyPaymentCode)
                    .ToListAsync();

            var oldValues = new
            {
                Payment =
                    GetScalarSnapshot(payment),

                Details =
                    oldDetails
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            CopyEditableScalarValues(
                payment,
                model,
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    nameof(TblMonthlypayment.MonthlyPaymentCode),
                    nameof(TblMonthlypayment.IsEnabled),
                    "CreateDate",
                    "CreateBy",
                    "RowVersion",
                    "Timestamp"
                });

            await db.TblMonthlypaymentDetails
                .Where(w =>
                    w.MonthlyPaymentCode ==
                    model.MonthlyPaymentCode)
                .ExecuteDeleteAsync();

            var newDetails =
                model.TblMonthlypaymentDetails?.ToList()
                ?? new List<TblMonthlypaymentDetail>();

            foreach (var detail in newDetails)
            {
                detail.MonthlyPaymentCode =
                    model.MonthlyPaymentCode;
            }

            if (newDetails.Count > 0)
            {
                db.TblMonthlypaymentDetails.AddRange(
                    newDetails);
            }

            await SynchronizeMonthlyWithholdingTaxAsync(
                payment,
                model.UpdateBy);

            var newValues = new
            {
                Payment =
                    GetScalarSnapshot(payment),

                Details =
                    newDetails
                        .Select(GetScalarSnapshot)
                        .ToList()
            };

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Update monthly payment.",
                    ActionName =
                        AuditActions.Update,
                    EntityName =
                        nameof(TblMonthlypayment),
                    EntityId =
                        model.MonthlyPaymentCode,
                    Description =
                        $"แก้ไขการจ่ายเงินรายเดือน {model.MonthlyPaymentCode} โดย {model.UpdateBy}",
                    OldValues =
                        oldValues,
                    NewValues =
                        newValues,
                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "EditMonthlyPayment",
                model.MonthlyPaymentCode);
        }
    }

    public async Task<ResponseViewModel> RemoveDailyPayment(
        string code,
        string update_by)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัสรายการจ่ายเงินรายวัน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var payment = await db.TblDailypayments
                .FirstOrDefaultAsync(
                    w => w.DailypaymentCode == code);

            if (payment == null)
            {
                await tr.RollbackAsync();
                return Failure($"ไม่พบรายการ {code}");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync();
                return Failure("รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            payment.IsEnabled = false;
            payment.UpdateDate = DateTime.Now;
            payment.UpdateBy = update_by;

            /*
             * คืน Budget เฉพาะรายการที่เคย Approved แล้ว
             * และยังไม่ถูกจ่ายจริงเท่านั้น
             * ป้องกันการลบ draft แล้วทำให้ยอด Budget เพิ่มขึ้น
             */
            if (payment.ApproveStatus == "A" &&
                payment.IsPaid == false)
            {
                var budget = await db.TblBudgets
                    .FirstOrDefaultAsync(
                        w => w.BudgetCode == "02");

                if (budget == null)
                {
                    throw new InvalidOperationException(
                        "ไม่พบ BudgetCode 02");
                }

                var currentBalance = budget.Balance;

                budget.Balance += payment.NetTotal;
                budget.UpdateDate = DateTime.Now;
                budget.UpdateBy = update_by;

                var transaction =
                    new TblBudgetTransaction
                    {
                        TransactionCode =
                            await codeRepo.GetBudgetTransactionCode(),

                        TransactionDate =
                            DateTime.Now,

                        TransactionType =
                            "OTHER",

                        Balance =
                            currentBalance,

                        Amount =
                            payment.NetTotal,

                        AmountDiff =
                            0,

                        PaymentType =
                            "UPDATE",

                        Remark =
                            payment.Remark,

                        BudgetCode =
                            "02",

                        IsEnabled =
                            true,

                        CreateDate =
                            DateTime.Now,

                        CreateBy =
                            update_by,

                        UpdateDate =
                            DateTime.Now,

                        UpdateBy =
                            update_by
                    };

                db.TblBudgetTransactions.Add(
                    transaction);

                db.TblDailypaymentTransactions.Add(
                    new TblDailypaymentTransaction
                    {
                        TransactionCode =
                            transaction.TransactionCode,

                        DailypaymentCode =
                            payment.DailypaymentCode
                    });
            }

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Remove daily payment.",
                    ActionName =
                        AuditActions.Delete,
                    EntityName =
                        nameof(TblDailypayment),
                    EntityId =
                        code,
                    Description =
                        $"ลบรายการจ่ายเงินรายวัน {code} โดย {update_by}",
                    OldValues =
                        oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "RemoveDailyPayment",
                code);
        }
    }

    public async Task<ResponseViewModel> RemoveMonthlyPayment(
        string code,
        string update_by)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัสรายการจ่ายเงินรายเดือน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var payment = await db.TblMonthlypayments
                .FirstOrDefaultAsync(
                    w => w.MonthlyPaymentCode == code);

            if (payment == null)
            {
                await tr.RollbackAsync();
                return Failure($"ไม่พบรายการ {code}");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync();
                return Failure("รายการนี้ถูกยกเลิกไปแล้ว");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            payment.IsEnabled = false;
            payment.UpdateDate = DateTime.Now;
            payment.UpdateBy = update_by;

            var taxPayment =
                await db.TblWithholdingTaxPayments
                    .FirstOrDefaultAsync(
                        w =>
                            w.MonthlyPaymentCode ==
                            payment.MonthlyPaymentCode);

            if (taxPayment != null)
            {
                var tax = await db.TblWithholdingTaxes
                    .FirstOrDefaultAsync(
                        w =>
                            w.WithholdingTaxCode ==
                            taxPayment.WithholdingTaxCode);

                if (tax != null)
                {
                    tax.IsEnabled = false;
                    tax.UpdateDate = DateTime.Now;
                    tax.UpdateBy = update_by;
                }
            }

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Remove monthly payment.",
                    ActionName =
                        AuditActions.Delete,
                    EntityName =
                        nameof(TblMonthlypayment),
                    EntityId =
                        code,
                    Description =
                        $"ลบรายการจ่ายเงินรายเดือน {code} โดย {update_by}",
                    OldValues =
                        oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "RemoveMonthlyPayment",
                code);
        }
    }

    public async Task<ResponseViewModel> Approve(
        string code,
        string update_by)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัสรายการจ่ายเงินรายวัน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var payment = await db.TblDailypayments
                .FirstOrDefaultAsync(
                    w => w.DailypaymentCode == code);

            if (payment == null)
            {
                await tr.RollbackAsync();
                return Failure($"ไม่พบรายการ {code}");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync();
                return Failure("รายการนี้ถูกยกเลิกไปแล้ว");
            }

            // ทำให้การ Approve เป็น idempotent ป้องกันหัก Budget ซ้ำ
            if (payment.ApproveStatus == "A")
            {
                await tr.RollbackAsync();
                return Success();
            }

            var oldValues =
                GetScalarSnapshot(payment);

            payment.ApproveStatus = "A";
            payment.UpdateDate = DateTime.Now;
            payment.UpdateBy = update_by;

            if (!payment.IsPaid)
            {
                await AddDailyBudget(
                    code,
                    payment.NetTotal,
                    update_by,
                    payment.Remark);
            }

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Approve daily payment.",
                    ActionName =
                        AuditActions.Update,
                    EntityName =
                        nameof(TblDailypayment),
                    EntityId =
                        code,
                    Description =
                        $"อนุมัติรายการจ่ายเงินรายวัน {code} โดย {update_by}",
                    OldValues =
                        oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "ApproveDailyPayment",
                code);
        }
    }

    public async Task<ResponseViewModel> Reject(
        string code,
        string update_by)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Failure("ไม่พบรหัสรายการจ่ายเงินรายวัน");
        }

        await using var tr =
            await db.Database.BeginTransactionAsync();

        try
        {
            var payment = await db.TblDailypayments
                .FirstOrDefaultAsync(
                    w => w.DailypaymentCode == code);

            if (payment == null)
            {
                await tr.RollbackAsync();
                return Failure($"ไม่พบรายการ {code}");
            }

            if (payment.IsEnabled != true)
            {
                await tr.RollbackAsync();
                return Failure("รายการนี้ถูกยกเลิกไปแล้ว");
            }

            /*
             * เดิมสามารถ Reject หลัง Approved ได้โดยไม่คืน Budget
             * จึงปิด flow นี้เพื่อไม่ให้สถานะกับยอดงบสวนกัน
             */
            if (payment.ApproveStatus == "A")
            {
                await tr.RollbackAsync();

                return Failure(
                    "รายการที่อนุมัติแล้วไม่สามารถ Reject ตรง ๆ ได้");
            }

            var oldValues =
                GetScalarSnapshot(payment);

            payment.ApproveStatus = "R";
            payment.UpdateDate = DateTime.Now;
            payment.UpdateBy = update_by;

            await auditLogService.AddAsync(
                new AuditLogEntry
                {
                    ModuleName =
                        "Reject daily payment.",
                    ActionName =
                        AuditActions.Update,
                    EntityName =
                        nameof(TblDailypayment),
                    EntityId =
                        code,
                    Description =
                        $"ปฏิเสธรายการจ่ายเงินรายวัน {code} โดย {update_by}",
                    OldValues =
                        oldValues,
                    NewValues =
                        GetScalarSnapshot(payment),
                    Result =
                        AuditResults.Success
                });

            await db.SaveChangesAsync();
            await tr.CommitAsync();

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(tr);

            return Error(
                ex,
                "RejectDailyPayment",
                code);
        }
    }

    public async Task AddDailyBudget(
        string code,
        decimal net_total,
        string create_by,
        string remark)
    {
        var budget = await db.TblBudgets
            .FirstOrDefaultAsync(
                w => w.BudgetCode == "02");

        if (budget == null)
        {
            throw new InvalidOperationException(
                "ไม่พบ BudgetCode 02");
        }

        var currentBalance =
            budget.Balance;

        budget.Balance -=
            net_total;

        budget.UpdateDate =
            DateTime.Now;

        budget.UpdateBy =
            create_by;

        var transaction =
            new TblBudgetTransaction
            {
                TransactionCode =
                    await codeRepo.GetBudgetTransactionCode(),

                TransactionDate =
                    DateTime.Now,

                TransactionType =
                    "OTHER",

                Balance =
                    currentBalance,

                Amount =
                    net_total,

                AmountDiff =
                    0,

                PaymentType =
                    "MIN",

                Remark =
                    remark,

                BudgetCode =
                    "02",

                IsEnabled =
                    true,

                CreateDate =
                    DateTime.Now,

                CreateBy =
                    create_by,

                UpdateDate =
                    DateTime.Now,

                UpdateBy =
                    create_by
            };

        db.TblBudgetTransactions.Add(
            transaction);

        db.TblDailypaymentTransactions.Add(
            new TblDailypaymentTransaction
            {
                TransactionCode =
                    transaction.TransactionCode,

                DailypaymentCode =
                    code
            });
    }

    private static void RecalculateFleetPaymentTotals(
        TblFleetpayment payment,
        IReadOnlyCollection<TblFleetpaymentDetail> details)
    {
        payment.FleetpaymentAmount = details.Count;
        payment.TransportTotal =
            details.Sum(x => x.TransportValue);

        payment.TransportVat =
            details.Sum(x => x.TransportVatValue);

        payment.TransportNet =
            details.Sum(x => x.TransportTotalValue);

        payment.TripTotal =
            details.Sum(x => x.TripValue);

        payment.FuelLitreTotal =
            details.Sum(x => x.FuelLitreValue);

        payment.FuelTotal =
            details.Sum(x => x.FuelTotalValue);

        payment.ExpressTotal =
            details.Sum(x => x.ExpressValue);

        payment.PaymentTotal =
            details.Sum(x => x.PaymentValue);

        payment.FleetpaymentTotal =
            payment.TransportNet -
            payment.SalaryTotal -
            payment.ShoreTotal -
            payment.AreaTotal -
            payment.MaintenanceTotal -
            payment.FuelTotal -
            payment.TripTotal -
            payment.ExpressTotal -
            payment.TyreTotal -
            payment.TyreServiceTotal -
            payment.Other;

        payment.FleetpaymentVat =
            payment.FleetpaymentTotal *
            1m / 100m;

        payment.FleetpaymentNetTotal =
            payment.FleetpaymentTotal -
            payment.FleetpaymentVat;
    }

    private static void RecalculateSupplierPaymentTotals(
        TblSupplierpayment payment,
        IReadOnlyCollection<TblSupplierpaymentDetail> details)
    {
        payment.SupplierpaymentAmount =
            details.Count;

        payment.TransportTotal =
            details.Sum(
                x => x.SupplierpaymentValue);

        payment.TransportVat =
            payment.TransportTotal *
            1m / 100m;

        payment.TransportNet =
            payment.TransportTotal -
            payment.TransportVat;

        payment.TotalPort =
            details.Sum(x => x.PortValue);

        payment.TotalYard =
            details.Sum(x => x.YardValue);

        payment.TotalLifton =
            details.Sum(x => x.LiftonValue);

        payment.TotalLiftoff =
            details.Sum(x => x.LiftoffValue);

        payment.TotalAdvance =
            details.Sum(x => x.AdvanceValue);

        payment.TotalAdditionalcharge =
            details.Sum(
                x => x.AdditionalchargeValue);

        payment.SupplierpaymentNetTotal =
            payment.TransportNet +
            payment.TotalAdvance +
            payment.TotalAdditionalcharge;
    }

    private async Task SynchronizeSupplierWithholdingTaxAsync(
        TblSupplierpayment payment,
        string? username)
    {
        var taxPayment =
            await db.TblWithholdingTaxSupplierpayments
                .FirstOrDefaultAsync(
                    w =>
                        w.SupplierpaymentCode ==
                        payment.SupplierpaymentCode);

        TblWithholdingTax? tax = null;

        if (taxPayment != null)
        {
            tax = await db.TblWithholdingTaxes
                .FirstOrDefaultAsync(
                    w =>
                        w.WithholdingTaxCode ==
                        taxPayment.WithholdingTaxCode);
        }

        var now = DateTime.Now;

        if (tax == null)
        {
            tax = new TblWithholdingTax
            {
                WithholdingTaxCode =
                    await codeRepo.GetTaxPaymentCode(),

                WithholdingTaxDate =
                    payment.SupplierpaymentDate,

                PaymentTypeCode = "05",

                SupplierCode =
                    payment.SupplierCode,

                PaymentTotal =
                    payment.TransportTotal,

                TaxTotal =
                    payment.TransportVat,

                PaymentNetTotal =
                    payment.TransportNet,

                IsEnabled = true,
                CreateDate = now,
                CreateBy = username,
                UpdateDate = now,
                UpdateBy = username
            };

            db.TblWithholdingTaxes.Add(tax);

            if (taxPayment == null)
            {
                db.TblWithholdingTaxSupplierpayments.Add(
                    new TblWithholdingTaxSupplierpayment
                    {
                        WithholdingTaxCode =
                            tax.WithholdingTaxCode,

                        SupplierpaymentCode =
                            payment.SupplierpaymentCode,

                        CreateDate = now,
                        CreateBy = username
                    });
            }
            else
            {
                taxPayment.WithholdingTaxCode =
                    tax.WithholdingTaxCode;
            }

            return;
        }

        tax.WithholdingTaxDate =
            payment.SupplierpaymentDate;

        tax.PaymentTypeCode = "05";
        tax.SupplierCode =
            payment.SupplierCode;

        tax.PaymentTotal =
            payment.TransportTotal;

        tax.TaxTotal =
            payment.TransportVat;

        tax.PaymentNetTotal =
            payment.TransportNet;

        tax.IsEnabled = true;
        tax.UpdateDate = now;
        tax.UpdateBy = username;
    }

    private async Task SynchronizeMonthlyWithholdingTaxAsync(
        TblMonthlypayment payment,
        string? username)
    {
        var taxPayment =
            await db.TblWithholdingTaxPayments
                .FirstOrDefaultAsync(
                    w =>
                        w.MonthlyPaymentCode ==
                        payment.MonthlyPaymentCode);

        TblWithholdingTax? tax = null;

        if (taxPayment != null)
        {
            tax = await db.TblWithholdingTaxes
                .FirstOrDefaultAsync(
                    w =>
                        w.WithholdingTaxCode ==
                        taxPayment.WithholdingTaxCode);
        }

        var now = DateTime.Now;

        if (payment.IsTax == true)
        {
            if (tax == null)
            {
                tax = new TblWithholdingTax
                {
                    WithholdingTaxCode =
                        await codeRepo.GetTaxPaymentCode(),

                    WithholdingTaxDate =
                        payment.PaymentDate,

                    PaymentTypeCode =
                        payment.PaymentTypeCode,

                    SupplierCode =
                        payment.SupplierCode,

                    PaymentTotal =
                        payment.Total,

                    TaxTotal =
                        payment.Tax,

                    PaymentNetTotal =
                        payment.Total -
                        payment.Tax,

                    IsEnabled = true,
                    CreateDate = now,
                    CreateBy = username,
                    UpdateDate = now,
                    UpdateBy = username
                };

                db.TblWithholdingTaxes.Add(tax);

                if (taxPayment == null)
                {
                    db.TblWithholdingTaxPayments.Add(
                        new TblWithholdingTaxPayment
                        {
                            WithholdingTaxCode =
                                tax.WithholdingTaxCode,

                            MonthlyPaymentCode =
                                payment.MonthlyPaymentCode,

                            CreateDate = now,
                            CreateBy = username
                        });
                }
                else
                {
                    taxPayment.WithholdingTaxCode =
                        tax.WithholdingTaxCode;
                }

                return;
            }

            tax.WithholdingTaxDate =
                payment.PaymentDate;

            tax.PaymentTypeCode =
                payment.PaymentTypeCode;

            tax.SupplierCode =
                payment.SupplierCode;

            tax.PaymentTotal =
                payment.Total;

            tax.TaxTotal =
                payment.Tax;

            tax.PaymentNetTotal =
                payment.Total -
                payment.Tax;

            tax.IsEnabled = true;
            tax.UpdateDate = now;
            tax.UpdateBy = username;
        }
        else if (tax != null)
        {
            tax.IsEnabled = false;
            tax.UpdateDate = now;
            tax.UpdateBy = username;
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

    private void CopyEditableScalarValues<TEntity>(
        TEntity target,
        TEntity source,
        ISet<string> protectedProperties)
        where TEntity : class
    {
        var targetEntry =
            db.Entry(target);

        var sourceEntry =
            db.Entry(source);

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

            if (protectedProperties.Contains(
                    propertyName))
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
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync();
        }
        catch (Exception rollbackEx)
        {
            logger.LogError(
                rollbackEx,
                "Error while rolling back Payment transaction.");
        }
    }

}