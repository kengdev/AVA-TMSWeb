using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class FormBillingRepository
{
    private readonly DbavalogContext db;

    public FormBillingRepository(DbavalogContext context)
    {
        db = context;
    }

    public async Task<TblCustomer> GetCustomerByCode(string customer_code)
    {
        var customer = await db.TblCustomers
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.CustomerCode == customer_code);

        if (customer == null)
            return null;

        customer.TblCustomerAddress = await db.TblCustomerAddresses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.CustomerCode == customer_code);

        return customer;
    }

    public async Task<TblBillingMaster> GetBillingDataByCode(string code)
    {
        return await db.TblBillingMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.BillingCode == code);
    }

    public async Task<List<BillingFormViewModel>> GetBilling(string billing_code)
    {
        return await db.ViewBillingListActives
            .AsNoTracking()
            .Where(a => a.BillingCode == billing_code)
            .Select(a => new BillingFormViewModel
            {
                job_detail_code = a.JobDetailCode,
                billing_code = a.BillingCode,
                container_no = a.ContainerNo,
                container_size = a.ContainerSize,
                container_type = a.ContainerType,
                job_invoice = a.JobInvoice,
                booking_no = a.BookingNo,
                mission_name = a.MissionName,
                cost_value = a.BillingValue.Value,
                advance_lifton = a.AdvanceLiftonup.Value,
                advance_liftoff = a.AdvanceLiftoffup.Value,
                advance_port = a.AdvancePortup.Value,
                advance_yard = a.AdvanceYardup.Value,
                billing_date = a.BillingDate.Value,
                billing_remark = a.BillingRemark,
                is_vat = a.IsVat.Value,
                job_date = a.JobDate,
                customer_code = a.CustomerCode,
                job_remark = a.JobRemark,
                job_description = a.StartName + " - " + a.MissionName,
                is_advance = a.IsAdvance.Value
            })
            .OrderBy(o => o.job_date)
            .ToListAsync();
    }

    public async Task<List<AdvanceFormViewModel>> GetAdvance(string billing_code)
    {
        return await (
            from a in db.TblJobDetails.AsNoTracking()
            join b in db.TblJobMasters.AsNoTracking()
                on a.JobCode equals b.JobCode
            join c in db.TblJobadvances.AsNoTracking()
                on a.JobDetailCode equals c.JobDetailCode
            join d in db.TblJobadvanceOthers.AsNoTracking()
                on c.AdvanceCode equals d.AdvanceCode
            join e in db.TblPlaces.AsNoTracking()
                on a.StartPlaceCode equals e.PlaceCode
            join f in db.TblPlaces.AsNoTracking()
                on a.MissionPlaceCode equals f.PlaceCode
            join g in db.TblBillingDetails.AsNoTracking()
                on a.JobDetailCode equals g.JobDetailCode
            where g.BillingCode == billing_code
            select new AdvanceFormViewModel
            {
                job_detail_code = a.JobDetailCode,
                job_date = b.JobDate,
                booking_no = b.BookingNo,
                container_no = a.ContainerNo,
                container_size = b.ContainerSizeName,
                start_name = e.PlaceName,
                mission_name = f.PlaceName,
                advance_other = d.AdvanceOther,
                advance_other_value = d.AdvanceOtherValueup,
                job_invoice = b.JobInvoice
            }
        ).ToListAsync();
    }

    public async Task<List<AdvanceFormViewModel>> GetAdvanceOther(string billing_code)
    {
        return await (
            from a in db.TblJobDetails.AsNoTracking()
            join b in db.TblJobMasters.AsNoTracking()
                on a.JobCode equals b.JobCode
            join c in db.TblJobadvances.AsNoTracking()
                on a.JobDetailCode equals c.JobDetailCode
            join d in db.TblJobadvanceOthers.AsNoTracking()
                on c.AdvanceCode equals d.AdvanceCode
            join e in db.TblPlaces.AsNoTracking()
                on a.StartPlaceCode equals e.PlaceCode
            join f in db.TblPlaces.AsNoTracking()
                on a.MissionPlaceCode equals f.PlaceCode
            join g in db.TblBillingOthers.AsNoTracking()
                on d.Id equals g.AdvanceId
            where g.BillingCode == billing_code
            select new AdvanceFormViewModel
            {
                job_detail_code = a.JobDetailCode,
                job_date = b.JobDate,
                booking_no = b.BookingNo,
                container_no = a.ContainerNo,
                container_size = b.ContainerSizeName,
                start_name = e.PlaceName,
                mission_name = f.PlaceName,
                advance_other = d.AdvanceOther,
                advance_other_value = d.AdvanceOtherValueup,
                job_invoice = b.JobInvoice,
                customer_code = b.CustomerCode
            }
        ).ToListAsync();
    }

    public async Task<List<BillingFormViewModel>> GetBillingOther(string billing_code)
    {
        return await db.ViewBillingOtherListActives
            .AsNoTracking()
            .Where(a => a.BillingCode == billing_code)
            .Select(a => new BillingFormViewModel
            {
                job_detail_code = a.JobDetailCode,
                billing_code = a.BillingCode,
                container_no = a.ContainerNo,
                container_size = a.ContainerSize,
                container_type = a.ContainerType,
                job_invoice = a.JobInvoice,
                booking_no = a.BookingNo,
                mission_name = a.MissionName,
                cost_value = a.CostValue.Value,
                advance_lifton = a.AdvanceLiftonup.Value,
                advance_liftoff = a.AdvanceLiftoffup.Value,
                advance_port = a.AdvancePortup.Value,
                advance_yard = a.AdvanceYardup.Value,
                billing_date = a.BillingDate.Value,
                billing_remark = a.BillingRemark,
                is_vat = a.IsVat.Value,
                job_date = a.JobDate,
                customer_code = a.CustomerCode,
                job_remark = a.JobRemark,
                job_description = a.StartName + " - " + a.MissionName,
                is_advance = a.IsAdvance.Value
            })
            .ToListAsync();
    }
}
