using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class ExportRepository
{
    private readonly DbavalogContext db;

    public ExportRepository(DbavalogContext context)
    {
        db = context;
    }

    public async Task<CheckListViewModel> GetCheckListValue(string code)
    {
        var model = await (
            from a in db.TblDrivers.AsNoTracking()
            join b in db.TblTruckChecklists.AsNoTracking()
                on a.DriverCode equals b.DriverCode
            join c in db.TblTrucks.AsNoTracking()
                on b.TruckCode equals c.TruckCode
            join d in db.TblTruckDetails.AsNoTracking()
                on c.TruckCode equals d.TruckCode
            where b.ChecklistCode == code
            select new CheckListViewModel
            {
                checklist_code = b.ChecklistCode,
                driver_name = a.DriverFullname,
                truck_license = c.TruckLicense,
                truck_no = d.TruckNo,
                checklist_date = b.ChecklistDate
            }
        ).FirstOrDefaultAsync();

        if (model == null)
            return null;

        model.detail = await (
            from a in db.TblTruckChecklists.AsNoTracking()
            join b in db.TblTruckChecklistDetails.AsNoTracking()
                on a.ChecklistCode equals b.ChecklistCode
            join c in db.TblTruckChecklistChoices.AsNoTracking()
                on b.TruckCheckCode equals c.TruckCheckCode
            where a.ChecklistCode == code
            orderby c.TruckCheckCode, c.TruckCheckGroup, c.TruckCheckIndex
            select new CheckListDetailViewModel
            {
                code = a.ChecklistCode,
                name = c.TruckCheckName,
                value = b.TruckCheckValue,
                issued = b.TruckCheckFixed,
                list_group = c.TruckCheckGroup
            }
        ).ToListAsync();

        return model;
    }

    public async Task<List<CheckListViewModel>> GetAllCheckListByMonth(
        DateTime start_date)
    {
        var startDate = new DateTime(
            start_date.Year,
            start_date.Month,
            1);

        var endDate = startDate.AddMonths(1);

        var checklistData = await (
            from a in db.TblDrivers.AsNoTracking()
            join b in db.TblTruckChecklists.AsNoTracking()
                on a.DriverCode equals b.DriverCode
            join c in db.TblTrucks.AsNoTracking()
                on b.TruckCode equals c.TruckCode
            join d in db.TblTruckDetails.AsNoTracking()
                on c.TruckCode equals d.TruckCode
            where b.ChecklistDate >= DateOnly.FromDateTime(startDate)
                  && b.ChecklistDate < DateOnly.FromDateTime(endDate)
            select new CheckListViewModel
            {
                checklist_code = b.ChecklistCode,
                driver_code = a.DriverCode,
                driver_name = a.DriverFullname,
                truck_license = c.TruckLicense,
                truck_no = d.TruckNo,
                checklist_date = b.ChecklistDate
            }
        ).ToListAsync();

        if (checklistData.Count == 0)
            return checklistData;

        var checklistCodes = checklistData
            .Select(x => x.checklist_code)
            .Distinct()
            .ToList();

        // เดิม query Detail ทีละ Checklist
        // เปลี่ยนเป็นโหลด Detail ของ Checklist ทั้งหมดครั้งเดียว
        var details = await (
            from b in db.TblTruckChecklistDetails.AsNoTracking()
            join c in db.TblTruckChecklistChoices.AsNoTracking()
                on b.TruckCheckCode equals c.TruckCheckCode
            where checklistCodes.Contains(b.ChecklistCode)
            orderby c.TruckCheckCode, c.TruckCheckGroup, c.TruckCheckIndex
            select new CheckListDetailViewModel
            {
                code = b.ChecklistCode,
                name = c.TruckCheckName,
                value = b.TruckCheckValue,
                issued = b.TruckCheckFixed,
                list_group = c.TruckCheckGroup
            }
        ).ToListAsync();

        var detailLookup = details
            .GroupBy(x => x.code)
            .ToDictionary(
                g => g.Key,
                g => g.ToList());

        foreach (var item in checklistData)
        {
            item.detail = detailLookup.TryGetValue(
                item.checklist_code,
                out var itemDetails)
                    ? itemDetails
                    : new List<CheckListDetailViewModel>();
        }

        return checklistData;
    }

    public async Task<List<ExpInvoiceModel>> GetCustomerTrackingList(
        DateTime start,
        DateTime end)
    {
        return await (
            from a in db.TblJobMasters.AsNoTracking()
            join b in db.TblJobDetails.AsNoTracking()
                on a.JobCode equals b.JobCode
            join c in db.TblPlaces.AsNoTracking()
                on b.MissionPlaceCode equals c.PlaceCode
            join d in db.TblTrucks.AsNoTracking()
                on b.TruckCode equals d.TruckCode
            join e in db.ViewDriverListActives.AsNoTracking()
                on new { b.DriverCode, b.TruckCode }
                equals new { e.DriverCode, e.TruckCode }
            join f in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals f.CustomerCode
            join g in db.TblPlaces.AsNoTracking()
                on b.StartPlaceCode equals g.PlaceCode
            join h in db.TblSuppliers.AsNoTracking()
                on b.SupplierCode equals h.SupplierCode
            where b.IsDocument == false
                  && a.IsEnabled != false
                  && b.IsCanceled == false
                  && a.JobDate >= start
                  && a.JobDate <= end
            select new ExpInvoiceModel
            {
                job_code = a.JobCode,
                job_detail_code = b.JobDetailCode,
                start_name = g.PlaceName,
                mission_name = c.PlaceName,
                booking_no = a.BookingNo,
                customer_name = f.CustomerAcronym,
                container_size = a.ContainerSizeName,
                container_no = b.ContainerNo,
                seal_no = b.SealNo,
                tare_weight = b.TareWeight,
                tracking_status = b.TrackingStatus,
                truck_license = d.TruckLicense,
                trailer_license = b.TrailerLicense,
                driver_fullname = e.DriverFullname,
                driver_phone = e.DriverPhone,
                job_remark = a.JobRemark,
                closing_time = a.ClosingTime,
                job_date = a.JobDate,
                supplier = h.SupplierName,
                industrial = c.PlaceName
            }
        ).ToListAsync();
    }

    public async Task<List<ExpPaymentModel>> GetPaymentList(
        DateTime start,
        DateTime end)
    {
        return await db.ViewJobAdvanceCashingListActives
            .AsNoTracking()
            .Where(a =>
                a.IsDocument == false
                && a.IsSingleTrip == false
                && a.IsCanceled == false
                && a.JobDate >= start
                && a.JobDate <= end)
            .Select(a => new ExpPaymentModel
            {
                job_detail_code = a.JobDetailCode,
                job_date = a.JobDate,
                job_type = a.JobType,
                booking_no = a.BookingNo,
                container_no = a.ContainerNo,
                container_size = a.ContainerSizeName,
                truck_license = a.TruckLicense,
                driver_fullname = a.DriverFullname,
                supplier = a.SupplierName,
                start_name = a.Start,
                mission_name = a.Industrial,
                industrial = a.Industrial,
                fuel = a.CashingFuel ?? 0,
                port = a.AdvancePort ?? 0,
                lift = a.AdvanceLifton != null
                    ? a.AdvanceLifton.Value + (a.AdvanceYard ?? 0)
                    : 0,
                express = a.CashingExpressway ?? 0,
                sh = a.JobType != "IMPORT" ? a.Shore : 0
            })
            .ToListAsync();
    }

    public async Task<List<JobLoadPlaceModel>> GetJobByCustomer(
        string code,
        string month,
        string year)
    {
        var m = int.Parse(month);
        var y = int.Parse(year);

        var startDate = new DateTime(y, m, 1);
        var endDate = startDate.AddMonths(1);

        return await (
            from a in db.TblJobMasters.AsNoTracking()
            join b in db.TblJobDetails.AsNoTracking()
                on a.JobCode equals b.JobCode
            join c in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals c.CustomerCode
            join d in db.TblPlaces.AsNoTracking()
                on b.MissionPlaceCode equals d.PlaceCode
            join e in db.TblJobadvances.AsNoTracking()
                on b.JobDetailCode equals e.JobDetailCode
            where a.CustomerCode == code
                  && a.JobDate >= startDate
                  && a.JobDate < endDate
                  && a.IsEnabled == true
                  && b.IsEnabled == true
            orderby a.JobDate, d.PlaceName
            select new JobLoadPlaceModel
            {
                job_invoice = a.JobInvoice,
                job_code = b.JobDetailCode,
                mission_name = d.PlaceName,
                booking_no = a.BookingNo,
                container_no = b.ContainerNo,
                customer_code = a.CustomerCode,
                customer_name = c.CustomerAcronym,
                trip_cost = b.CostValue,
                advance_cost = e.AdvanceTotal,
                job_date = a.JobDate,
                is_billing = b.IsBilling
            }
        ).ToListAsync();
    }

    public async Task<List<ExpOverdueModel>> GetOverdueList(
        string customer_code,
        string month,
        string year)
    {
        var m = int.Parse(month);
        var y = int.Parse(year);

        // BillingDate อาจเป็น DateOnly หรือ DateTime ตาม entity รุ่นที่ใช้
        // จึงคง Month/Year เพื่อไม่เปลี่ยน type contract ของ model
        return await (
            from a in db.TblBillingMasters.AsNoTracking()
            join b in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals b.CustomerCode
            join c in db.TblPlaces.AsNoTracking()
                on a.MissionPlaceCode equals c.PlaceCode
            where a.IsEnabled == true
                  && a.IsClearing == false
                  && a.CustomerCode == customer_code
                  && a.BillingDate.Month == m
                  && a.BillingDate.Year == y
            select new ExpOverdueModel
            {
                billing_code = a.BillingCode,
                billing_date = a.BillingDate,
                customer_name = b.CustomerAcronym,
                billing_type = a.BillingType,
                mission_name = c.PlaceName,
                transport_value = a.TransportTotal,
                advance_value = a.AdvanceTotal,
                billing_total = a.BillingTotal
            }
        ).ToListAsync();
    }
}
