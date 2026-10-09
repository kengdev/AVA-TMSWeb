using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class FormRepository
{
    private readonly DbavalogContext db;

    public FormRepository(DbavalogContext context)
    {
        db = context;
    }
    public async Task<TblCustomer> GetCustomerByCode(string customer_code)
    {
        var query = await db.TblCustomers.AsNoTracking().Where(w => w.CustomerCode == customer_code).FirstOrDefaultAsync();
        return query;
    }

    public async Task<TblCustomerRule> GetCustomerRule(string code)
    {
        var query = await db.TblCustomerRules.AsNoTracking().Where(w => w.CustomerCode == code).FirstOrDefaultAsync();
        return query;
    }

    public async Task<TblPlace> GetPlaceByCode(string place_code)
    {
        var query = await db.TblPlaces.AsNoTracking().Where(w => w.PlaceCode == place_code).FirstOrDefaultAsync();
        return query;
    }

    public async Task<OrderFormViewModel> GetJobOrderData(string job_detail_code)
    {
        var data = await (from a in db.ViewJobListActives.AsNoTracking()
                          where a.JobDetailCode == job_detail_code
                          select new OrderFormViewModel
                          {
                              job_code = a.JobCode,
                              job_detail_code = a.JobDetailCode,
                              truck_license = a.TruckLicense,
                              trailer_license = a.TrailerLicense,
                              driver_fullname = a.DriverFullname,
                              customer_code = a.CustomerCode,
                              job_date = a.JobDate,
                              container_size = a.ContainerSizeName,
                              job_type = a.JobType,
                              start_name = a.StartName,
                              mission_name = a.MissionName,
                              closing_time = a.ClosingTime,
                              job_invoice = a.JobInvoice,
                              finish_name = a.FinishName,
                              booking_no = a.BookingNo,
                              agent_code = a.AgentCode,
                              container_no = a.ContainerNo,
                              seal_no = a.SealNo,
                              job_remark = a.JobRemark
                          }).FirstOrDefaultAsync();
        return data;
    }

    public async Task<ViewOrderListActive> GetJobOrderByCode(string code)
    {
        var data = await db.ViewOrderListActives.AsNoTracking().Where(w => w.JobDetailCode == code).FirstOrDefaultAsync();
        return data;
    }

    public async Task<TblJobcashing> GetJobcashingBycode(string code)
    {
        var data = await db.TblJobcashings.AsNoTracking().Where(w => w.JobDetailCode == code).FirstOrDefaultAsync();
        return data;
    }

    public async Task<DriverViewModel> GetTruckByJobCode(string code)
    {
        var data = await (from a in db.TblTrucks.AsNoTracking()
                          join b in db.TblJobDetails on a.TruckCode equals b.TruckCode
                          join c in db.TblDrivers on b.DriverCode equals c.DriverCode
                          join d in db.TblTruckDetails on a.TruckCode equals d.TruckCode
                          join e in db.TblSuppliers on a.SupplierCode equals e.SupplierCode
                          where b.JobDetailCode == code
                          select new DriverViewModel
                          {
                              truck_license = b.TruckCode != "T000000" ? a.TruckLicense : "",
                              fullname = b.DriverCode != "DV000000" ? c.DriverFullname : "",
                              truck_typegroup_code = d.TruckTypegroupCode,
                              fleet_type = e.FleetTypeCode
                          }).FirstOrDefaultAsync();
        if (data != null)
        {
            return data;
        }
        else
        {
            var model = new DriverViewModel();
            model.truck_license = "";
            model.fullname = "";
            model.fleet_type = "F2";

            return model;
        }
    }

    public async Task<List<TblReceipt>> GetReceiptByMonth(string month, string year)
    {
        var m = Convert.ToInt32(month);
        var y = Convert.ToInt32(year);
        var startDate = new DateTime(y, m, 1);
        var endDate = startDate.AddMonths(1);

        var data = await db.TblReceipts
            .AsNoTracking()
            .Where(w => w.CreateDate >= startDate &&
                        w.CreateDate < endDate &&
                        w.IsEnabled == true)
            .ToListAsync();

        return data;
    }

    public async Task<TblReceipt> GetReceiptByCode(string code)
    {
        var data = await db.TblReceipts.AsNoTracking().Where(w => w.ReceiptCode == code).FirstOrDefaultAsync();
        return data;
    }

    public async Task<TblCoversheet> GetCoverSheetByCode(string code)
    {
        var data = await db.TblCoversheets.AsNoTracking().Where(w => w.CoversheetCode == code).FirstOrDefaultAsync();
        return data;
    }

    public async Task<ViewDriverListActive> GetDriverByCode(string code)
    {
        var data = await db.ViewDriverListActives.AsNoTracking().Where(w => w.DriverCode == code).FirstOrDefaultAsync();
        return data;
    }

    public async Task<List<BillingFormViewModel>> GetBilling(string billing_code)
    {
        var data = await (from a in db.ViewBillingListActives.AsNoTracking()
                          where a.BillingCode == billing_code
                          select new BillingFormViewModel
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
                              advance_lifton = a.AdvanceLifton.Value,
                              advance_liftoff = a.AdvanceLiftoff.Value,
                              advance_port = a.AdvancePort.Value,
                              advance_yard = a.AdvanceYard.Value,
                              billing_date = a.BillingDate.Value,
                              billing_remark = a.BillingRemark,
                              is_vat = a.IsVat.Value,
                              job_date = a.JobDate,
                              customer_code = a.CustomerCode,
                              job_remark = a.JobRemark,
                              job_description = a.StartName + " - " + a.MissionName,
                              is_advance = a.IsAdvance.Value,
                          }).ToListAsync();
        return data;
    }

    public async Task<List<AdvanceFormViewModel>> GetAdvance(string billing_code)
    {
        var data = await (from a in db.TblJobDetails.AsNoTracking()
                          join b in db.TblJobMasters on a.JobCode equals b.JobCode
                          join c in db.TblJobadvances on a.JobDetailCode equals c.JobDetailCode
                          join d in db.TblJobadvanceOthers on c.AdvanceCode equals d.AdvanceCode
                          join e in db.TblPlaces on a.StartPlaceCode equals e.PlaceCode
                          join f in db.TblPlaces on a.MissionPlaceCode equals f.PlaceCode
                          join g in db.TblBillingDetails on a.JobDetailCode equals g.JobDetailCode
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
                              advance_other_value = d.AdvanceOtherValue,
                              job_invoice = b.JobInvoice
                          }).ToListAsync();
        return data;
    }

    public async Task<List<AdvanceFormViewModel>> GetAdvanceOther(string billing_code)
    {
        var data = await (from a in db.TblJobDetails.AsNoTracking()
                          join b in db.TblJobMasters on a.JobCode equals b.JobCode
                          join c in db.TblJobadvances on a.JobDetailCode equals c.JobDetailCode
                          join d in db.TblJobadvanceOthers on c.AdvanceCode equals d.AdvanceCode
                          join e in db.TblPlaces on a.StartPlaceCode equals e.PlaceCode
                          join f in db.TblPlaces on a.MissionPlaceCode equals f.PlaceCode
                          join g in db.TblBillingOthers on d.Id equals g.AdvanceId
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
                              advance_other_value = d.AdvanceOtherValue,
                              job_invoice = b.JobInvoice
                          }).ToListAsync();
        return data;
    }

    public async Task<List<BillingFormViewModel>> GetBillingOther(string billing_code)
    {
        var data = await (from a in db.ViewBillingOtherListActives.AsNoTracking()
                          where a.BillingCode == billing_code
                          select new BillingFormViewModel
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
                              advance_lifton = a.AdvanceLifton.Value,
                              advance_liftoff = a.AdvanceLiftoff.Value,
                              advance_port = a.AdvancePort.Value,
                              advance_yard = a.AdvanceYard.Value,
                              billing_date = a.BillingDate.Value,
                              billing_remark = a.BillingRemark,
                              is_vat = a.IsVat.Value,
                              job_date = a.JobDate,
                              customer_code = a.CustomerCode,
                              job_remark = a.JobRemark,
                              job_description = a.StartName + " - " + a.MissionName,
                              is_advance = a.IsAdvance.Value,
                          }).ToListAsync();
        return data;
    }

    public async Task<List<ReceiptDetailFormViewModel>> GetReceiptValue(string code)
    {
        var data = await (from a in db.TblReceiptDetails.AsNoTracking()
                          join b in db.TblReceipts on a.ReceiptCode equals b.ReceiptCode
                          join c in db.TblBillingMasters on a.BillingCode equals c.BillingCode
                          where b.ReceiptCode == code
                          select new ReceiptDetailFormViewModel
                          {
                              receipt_code = a.ReceiptCode,
                              billing_code = c.BillingCode,
                              advance_value = a.ReceiptAdvanceValue,
                              transport_value = a.ReceiptTransportValue,
                              receipt_total = a.ReceiptValue,
                              billing_type = a.BillingType
                          }).ToListAsync();
        return data;
    }

    public async Task<List<CoverSheetFormViewModel>> GetReportCoverSheet(string code)
    {
        var data = await (from a in db.TblCoversheetDetails.AsNoTracking()
                          join b in db.TblCoversheets on a.CoversheetCode equals b.CoversheetCode
                          join c in db.TblBillingMasters on a.BillingCode equals c.BillingCode
                          where b.CoversheetCode == code && b.IsEnabled == true
                          select new CoverSheetFormViewModel
                          {
                              coversheet_code = b.CoversheetCode,
                              billing_code = c.BillingCode,
                              is_advance = c.IsAdvance,
                              billing_value = c.BillingNetTotal,
                              coversheet_value = a.CoversheetValue,
                              coversheet_total = b.CoversheetTotal,
                              coversheet_date = b.CoversheetDate,
                              billing_type = c.BillingType,
                              customer_code = b.CustomerCode,
                              is_vat = b.IsVat
                          }).ToListAsync();
        return data;
    }

    public async Task<List<CoverSheetViewModel>> GetCoverSheetValue(string code)
    {
        var data = await (from a in db.TblCoversheetDetails.AsNoTracking()
                          join b in db.TblCoversheets on a.CoversheetCode equals b.CoversheetCode
                          join c in db.TblBillingMasters on a.BillingCode equals c.BillingCode
                          where b.CoversheetCode == code
                          select new CoverSheetViewModel
                          {
                              billing_code = c.BillingCode,
                              billing_type = c.BillingType,
                              transport_value = c.TransportTotal,
                              advance_value = c.AdvanceTotal,
                              billing_total = c.BillingTotal,

                          }).ToListAsync();
        return data;
    }

    public async Task<List<OceanJobFormViewModel>> GetOceanJob(string code)
    {
        var data = await (from a in db.TblJobMasters.AsNoTracking()
                          join b in db.TblJobOceans on a.JobCode equals b.JobCode
                          join c in db.TblJobPlaces on a.JobCode equals c.JobCode
                          join d in db.TblPlaces on c.StartCode equals d.PlaceCode
                          where a.JobCode == code
                          select new OceanJobFormViewModel
                          {
                              quantity = a.ContainerAmount,
                              container_type = a.ContainerSizeName,
                              vessel = b.VesselName,
                              eta_date = b.EtaDate,
                              bl_no = b.BlNo,
                              change_no = b.ChangeNo,
                              dossier = a.JobInvoice,
                              container = b.ContainerNo,
                              quantity_detail = b.OrderQuantity,
                              weight = b.OrderWeight,
                              cbm = b.OrderCbm,
                              delivery_date = b.DeliveryDate,
                              pickup_date = b.PickupDate,
                              start_address = d.PlaceName,
                              mission_address = b.PlaceCode,
                              consignee_name = b.ConsigneeName,
                              shipper_name = b.ShipperName,
                              shipping_name = b.ShippingName,
                              request_name = b.RequestName,
                              description = b.Description,
                              job_incoterm = a.JobIncoterm,
                              goods_declaration = b.GoodsDeclarationNo
                          }).ToListAsync();

        return data;
    }

    public async Task<List<OceanJobFormViewModel>> GetOceanJobAssignment(string code)
    {
        var data = await (from a in db.TblJobMasters.AsNoTracking()
                          join b in db.TblJobOceans on a.JobCode equals b.JobCode
                          join c in db.TblJobPlaces on a.JobCode equals c.JobCode
                          join d in db.TblPlaces on c.StartCode equals d.PlaceCode
                          join e in db.TblJobDetails on a.JobCode equals e.JobCode
                          join g in db.TblTrucks on e.TruckCode equals g.TruckCode
                          join h in db.ViewDriverListActives on e.DriverCode equals h.DriverCode
                          where e.JobDetailCode == code
                          select new OceanJobFormViewModel
                          {
                              license = g.TruckLicense,
                              driver_name = h.DriverFullname,
                              driver_phone = h.DriverPhone,
                              quantity = a.ContainerAmount,
                              container_type = a.ContainerSizeName,
                              vessel = b.VesselName,
                              bl_no = a.BookingNo,
                              eta_date = b.EtaDate,
                              change_no = b.ChangeNo,
                              dossier = a.JobInvoice,
                              container = b.ContainerNo,
                              quantity_detail = b.OrderQuantity,
                              weight = b.OrderWeight,
                              cbm = b.OrderCbm,
                              delivery_date = b.DeliveryDate,
                              pickup_date = b.PickupDate,
                              start_address = d.PlaceName,
                              mission_address = b.PlaceCode,
                              consignee_name = b.ConsigneeName,
                              shipper_name = b.ShipperName,
                              shipping_name = b.ShippingName,
                              request_name = b.RequestName,
                              description = b.Description
                          }).ToListAsync();

        return data;
    }

    public async Task<List<TblJobOcean>> GetJobOceanByCode(string code)
    {
        var data = await db.TblJobOceans.AsNoTracking().Where(w => w.JobCode == code).ToListAsync();
        return data;
    }

    public async Task<List<TblAllowance>> GetPaymentListByDate(DateOnly start, DateOnly end)
    {
        var data = await (from a in db.TblAllowances.AsNoTracking()
                          join c in db.ViewDriverListActives on a.DriverCode equals c.DriverCode
                          where a.StartDate == start && a.EndDate == end && a.IsEnabled == true
                          orderby c.DriverFullname
                          select a).ToListAsync();

        return data;
    }

    public async Task<List<PaymentFormViewModel>> GetPaymentList(string allowance_code, string driver_code)
    {
        var rows = await (from b in db.TblAllowanceDetails.AsNoTracking()
                          join c in db.TblAllowances on b.AllowanceCode equals c.AllowanceCode
                          join d in db.TblJobDetails on b.JobDetailCode equals d.JobDetailCode into d1
                          from aj in d1.DefaultIfEmpty()

                          join q in db.TblJobMasters on aj.JobCode equals q.JobCode into q1
                          from jd in q1.DefaultIfEmpty()

                          join i in db.TblPlaces on aj.MissionPlaceCode equals i.PlaceCode into i1
                          from jp in i1.DefaultIfEmpty()

                          where c.AllowanceCode == allowance_code &&
                                c.DriverCode == driver_code
                          orderby b.AllowanceDate
                          select new
                          {
                              b.AllowanceDate,
                              b.JobDetailCode,
                              BookingNo = jd.BookingNo,
                              ContainerNo = aj.ContainerNo,
                              PlaceName = jp.PlaceName,
                              b.TripValue,
                              b.FuelValue,
                              b.WasteValue,
                              b.OtherValue,
                              c.DriverCode
                          }).ToListAsync();

        return rows.Select(s => new PaymentFormViewModel
        {
            working_date = s.AllowanceDate.ToString("dd MMMM yyyy"),
            job_detail_code = s.JobDetailCode,
            booking_no = s.BookingNo ?? "-",
            container_no = s.ContainerNo ?? "-",
            trip_place = s.PlaceName ?? "-",
            trip_value = s.TripValue,
            fuel_value = s.FuelValue,
            waste_value = s.WasteValue,
            other_value = s.OtherValue,
            driver_code = s.DriverCode
        }).ToList();
    }

    public async Task<List<TblAllowanceDetail>> GetAllowanceDetailByCode(string code)
    {
        var data = await db.TblAllowanceDetails.AsNoTracking().Where(w => w.AllowanceCode == code).ToListAsync();
        return data;
    }

    public async Task<TblAllowance> GetAllowanceByCode(string code)
    {
        var data = await db.TblAllowances.AsNoTracking().Where(w => w.AllowanceCode == code).FirstOrDefaultAsync();
        return data;
    }

    public async Task<List<SpecialFormViewModel>> GetSubtractList(string allowance_code, string driver_code)
    {
        var data = await (from a in db.TblDriverSpecialvalues.AsNoTracking()
                          where a.AllowanceCode == allowance_code &&
                                a.DriverCode == driver_code
                          select new SpecialFormViewModel
                          {
                              type = a.SpecialvalueType,
                              name = a.SpecialvalueName,
                              value = a.Specialvalue
                          }).ToListAsync();
        return data;
    }

    public async Task<List<FleetFormViewModel>> GetFleetPayment(string code)
    {
        var detail = await (from a in db.TblJobDetails.AsNoTracking()
                            join b in db.TblJobMasters on a.JobCode equals b.JobCode
                            join c in db.TblFleetpaymentDetails on a.JobDetailCode equals c.JobDetailCode
                            join d in db.TblFleetpayments on c.FleetpaymentCode equals d.FleetpaymentCode
                            join e in db.TblPlaces on a.MissionPlaceCode equals e.PlaceCode
                            join g in db.TblPlaces on a.StartPlaceCode equals g.PlaceCode
                            where d.FleetpaymentCode == code
                            select new SupplierFormDetailViewModel
                            {
                                job_date = b.JobDate,
                                invoice_no = a.SupplierInvoice,
                                booking_no = b.BookingNo,
                                container_no = a.ContainerNo,
                                mission_name = e.PlaceName,
                                transport_value = c.TransportValue,
                                trip_value = c.TripValue,
                                fuel_value = c.FuelTotalValue,
                                total_value = c.PaymentValue,
                            }).OrderBy(o => o.job_date).ToListAsync();

        var data = await (from a in db.TblFleetpayments.AsNoTracking()
                          join b in db.TblTrucks on a.TruckCode equals b.TruckCode
                          join d in db.TblTruckDetails on b.TruckCode equals d.TruckCode
                          join c in db.TblSuppliers on b.SupplierCode equals c.SupplierCode
                          where a.FleetpaymentCode == code
                          select new FleetFormViewModel
                          {
                              truck_no = d.TruckNo,
                              license = b.TruckLicense + " (" + d.TruckNo + ")",
                              supplier_name = a.ReceiverName,
                              supplier_address = a.ReceiverAddress,
                              supplier_tax = a.ReceiverTax,
                              payment_code = a.FleetpaymentCode,
                              payment_date = a.CreateDate,
                              payment_total = a.PaymentTotal,
                              transport_total = a.TransportTotal,
                              transport_vat = a.TransportVat,
                              transport_net = a.TransportNet,
                              trip_total = a.TripTotal,
                              fuel_total = a.FuelTotal,
                              salary_total = a.SalaryTotal,
                              shore_total = a.ShoreTotal,
                              area_total = a.AreaTotal,
                              express_total = a.ExpressTotal,
                              maintenance_total = a.MaintenanceTotal,
                              tyre_total = a.TyreTotal,
                              other = a.Other,
                              tyre_service_total = a.TyreServiceTotal,
                              fleetpayment_total = a.FleetpaymentTotal,
                              fleetpayment_vat_total = a.FleetpaymentVat,
                              fleetpayment_net_total = a.FleetpaymentNetTotal,
                              remark = a.FleetpaymentRemark
                          }).ToListAsync();

        foreach (var item in data)
        {
            item.detail = detail;
        }

        return data;
    }

    public async Task<List<FleetFormViewModel>> GetFleetPaymentList(string month, string year)
    {
        var m = Convert.ToInt32(month);
        var y = Convert.ToInt32(year);
        var startDate = new DateTime(y, m, 1);
        var endDate = startDate.AddMonths(1);

        var data = await (from a in db.TblFleetpayments.AsNoTracking()
                          join b in db.TblTrucks on a.TruckCode equals b.TruckCode
                          join d in db.TblTruckDetails on b.TruckCode equals d.TruckCode
                          join c in db.TblSuppliers on b.SupplierCode equals c.SupplierCode
                          where a.CreateDate >= startDate &&
                                a.CreateDate < endDate
                          select new FleetFormViewModel
                          {
                              truck_no = d.TruckNo,
                              license = b.TruckLicense + " (" + d.TruckNo + ")",
                              supplier_name = a.ReceiverName,
                              supplier_address = a.ReceiverAddress,
                              supplier_tax = a.ReceiverTax,
                              payment_code = a.FleetpaymentCode,
                              payment_date = a.CreateDate,
                              payment_total = a.PaymentTotal,
                              transport_total = a.TransportTotal,
                              transport_vat = a.TransportVat,
                              transport_net = a.TransportNet,
                              trip_total = a.TripTotal,
                              fuel_total = a.FuelTotal,
                              salary_total = a.SalaryTotal,
                              shore_total = a.ShoreTotal,
                              area_total = a.AreaTotal,
                              express_total = a.ExpressTotal,
                              maintenance_total = a.MaintenanceTotal,
                              tyre_total = a.TyreTotal,
                              other = a.Other,
                              tyre_service_total = a.TyreServiceTotal,
                              fleetpayment_total = a.FleetpaymentTotal,
                              fleetpayment_vat_total = a.FleetpaymentVat,
                              fleetpayment_net_total = a.FleetpaymentNetTotal,
                              remark = a.FleetpaymentRemark
                          }).ToListAsync();

        if (data.Count == 0)
        {
            return data;
        }

        var paymentCodes = data
            .Select(s => s.payment_code)
            .Distinct()
            .ToList();

        var detailRows = await (from a in db.TblJobDetails.AsNoTracking()
                                join b in db.TblJobMasters on a.JobCode equals b.JobCode
                                join c in db.TblFleetpaymentDetails on a.JobDetailCode equals c.JobDetailCode
                                join d in db.TblFleetpayments on c.FleetpaymentCode equals d.FleetpaymentCode
                                join e in db.TblPlaces on a.MissionPlaceCode equals e.PlaceCode
                                join g in db.TblPlaces on a.StartPlaceCode equals g.PlaceCode
                                where paymentCodes.Contains(d.FleetpaymentCode)
                                orderby b.JobDate
                                select new
                                {
                                    PaymentCode = d.FleetpaymentCode,
                                    JobDate = b.JobDate,
                                    InvoiceNo = a.SupplierInvoice,
                                    BookingNo = b.BookingNo,
                                    ContainerNo = a.ContainerNo,
                                    MissionName = e.PlaceName,
                                    TransportValue = c.TransportValue,
                                    TripValue = c.TripValue,
                                    FuelValue = c.FuelTotalValue,
                                    TotalValue = c.PaymentValue
                                }).ToListAsync();

        var detailLookup = detailRows
            .GroupBy(g => g.PaymentCode)
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => new SupplierFormDetailViewModel
                {
                    job_date = s.JobDate,
                    invoice_no = s.InvoiceNo,
                    booking_no = s.BookingNo,
                    container_no = s.ContainerNo,
                    mission_name = s.MissionName,
                    transport_value = s.TransportValue,
                    trip_value = s.TripValue,
                    fuel_value = s.FuelValue,
                    total_value = s.TotalValue
                }).ToList());

        foreach (var item in data)
        {
            item.detail = detailLookup.TryGetValue(item.payment_code, out var itemDetails)
                ? itemDetails
                : new List<SupplierFormDetailViewModel>();
        }

        return data;
    }

    public async Task<List<PND50ViewModel>> GetPND50(string code)
    {
        var data = await (from a in db.TblWithholdingTaxes.AsNoTracking()
                          join b in db.TblSuppliers on a.SupplierCode equals b.SupplierCode
                          join c in db.TblPaymentTypes on a.PaymentTypeCode equals c.PaymentTypeCode
                          where a.WithholdingTaxCode == code
                          select new PND50ViewModel
                          {
                              code = a.WithholdingTaxCode,
                              supplyer_name = b.SupplierFullname,
                              supplyer_address = b.SupplierAddress,
                              supplyer_vat_no = b.SupplierVatNo,
                              supplyer_type = b.SupplierTypeCode,
                              total_value = a.PaymentTotal,
                              vat_value = a.TaxTotal,
                              net_value = a.PaymentNetTotal,
                              invoice_type = c.PaymentTypeName,
                              invoice_date = a.WithholdingTaxDate
                          }).ToListAsync();
        return data;
    }

    public async Task<List<PND50ViewModel>> GetPND50ByMonth(string month, string year)
    {
        var m = Convert.ToInt32(month);
        var y = Convert.ToInt32(year);
        var startDate = new DateTime(y, m, 1);
        var endDate = startDate.AddMonths(1);

        var data = await (from a in db.TblWithholdingTaxes.AsNoTracking()
                          join b in db.TblSuppliers.AsNoTracking() on a.SupplierCode equals b.SupplierCode
                          join c in db.TblPaymentTypes.AsNoTracking() on a.PaymentTypeCode equals c.PaymentTypeCode
                          where a.WithholdingTaxDate >= DateOnly.FromDateTime(startDate) &&
                                a.WithholdingTaxDate < DateOnly.FromDateTime(endDate)
                          select new PND50ViewModel
                          {
                              code = a.WithholdingTaxCode,
                              supplyer_name = b.SupplierFullname,
                              supplyer_address = b.SupplierAddress,
                              supplyer_vat_no = b.SupplierVatNo,
                              supplyer_type = b.SupplierTypeCode,
                              total_value = a.PaymentTotal,
                              vat_value = a.TaxTotal,
                              net_value = a.PaymentNetTotal,
                              invoice_type = c.PaymentTypeName,
                              invoice_date = a.WithholdingTaxDate
                          }).ToListAsync();
        return data;
    }

    public async Task<SupplierFormViewModel> GetSupplierPayment(string code)
    {
        var detail = await (from a in db.TblJobDetails.AsNoTracking()
                            join b in db.TblJobMasters on a.JobCode equals b.JobCode
                            join c in db.TblSupplierpaymentDetails on a.JobDetailCode equals c.JobDetailCode
                            join d in db.TblSupplierpayments on c.SupplierpaymentCode equals d.SupplierpaymentCode
                            join e in db.TblPlaces on a.MissionPlaceCode equals e.PlaceCode
                            join g in db.TblPlaces on a.StartPlaceCode equals g.PlaceCode
                            where d.SupplierpaymentCode == code
                            orderby b.JobDate ascending
                            select new SupplierFormDetailViewModel
                            {
                                job_date = b.JobDate,
                                invoice_no = a.SupplierInvoice,
                                booking_no = b.BookingNo,
                                container_no = a.ContainerNo,
                                mission_name = e.PlaceName,
                                transport_value = c.SupplierpaymentValue,
                                advance_value = c.AdvanceValue,
                                port_value = c.PortValue,
                                yard_value = c.YardValue,
                                liftoff_value = c.LiftoffValue,
                                lifton_value = c.LiftonValue,
                                total_value = c.TotalValue,
                                additional_value = c.AdvanceValue,
                            }).ToListAsync();

        var data = await (from a in db.TblSupplierpayments.AsNoTracking()
                          join b in db.TblSuppliers on a.SupplierCode equals b.SupplierCode
                          where a.SupplierpaymentCode == code
                          select new SupplierFormViewModel
                          {
                              additional_total = a.TotalAdditionalcharge,
                              supplier_name = b.SupplierFullname,
                              supplier_address = b.SupplierAddress,
                              supplier_tax = b.SupplierVatNo,
                              supplierpayment_code = a.SupplierpaymentCode,
                              supplierpayment_date = a.SupplierpaymentDate,
                              supplierpayment_total = a.SupplierpaymentNetTotal,
                              payment_type = a.PaymentType,
                              total_transport = a.TransportTotal,
                              transport_vat = a.TransportVat,
                              total_advance = a.TotalAdvance,
                              total_port = a.TotalPort,
                              total_yard = a.TotalYard,
                              total_lifton = a.TotalLifton,
                              total_liftoff = a.TotalLiftoff
                          }).FirstOrDefaultAsync();

        if (data != null)
        {
            data.detail = detail;
        }

        return data;
    }

    public async Task<List<AdditionalChargeViewModel>> GetSupplierAdditional(string code)
    {
        var data = await (from a in db.TblJobDetails.AsNoTracking()
                          join b in db.TblJobMasters on a.JobCode equals b.JobCode
                          join c in db.TblSupplierpaymentDetails on a.JobDetailCode equals c.JobDetailCode
                          join d in db.TblSupplierpayments on c.SupplierpaymentCode equals d.SupplierpaymentCode
                          join e in db.TblPlaces on a.MissionPlaceCode equals e.PlaceCode
                          join f in db.TblJobadvances on a.JobDetailCode equals f.JobDetailCode
                          join g in db.TblJobadvanceOthers on f.AdvanceCode equals g.AdvanceCode
                          where d.SupplierpaymentCode == code &&
                              g.SupplierOtherValue > 0
                          select new AdditionalChargeViewModel
                          {
                              container_no = a.ContainerNo,
                              booking_no = b.BookingNo,
                              job_date = b.JobDate,
                              mission_name = e.PlaceName,
                              other_name = g.AdvanceOther,
                              other_value = g.SupplierOtherValue
                          }).OrderBy(o => o.job_date).ToListAsync();
        return data;
    }

    public async Task<List<JobViewModel>> GetClearingDataListByCode(string code)
    {
        var rows = await (from a in db.TblJobMasters.AsNoTracking()
                          join b in db.TblJobDetails on a.JobCode equals b.JobCode
                          join c in db.TblShippingadvances on b.JobDetailCode equals c.JobDetailCode
                          join d in db.TblPlaces on b.MissionPlaceCode equals d.PlaceCode
                          join e in db.TblCustomers on a.CustomerCode equals e.CustomerCode
                          join f in db.TblShippingpaymentDetails on b.JobDetailCode equals f.JobDetailCode
                          where f.PaymentCode == code
                          orderby a.JobDate ascending
                          select new
                          {
                              a.JobCode,
                              a.JobDate,
                              a.JobType,
                              a.BookingNo,
                              CustomerName = e.CustomerAcronym,
                              a.ContainerType,
                              a.ContainerSizeName,
                              MissionName = d.PlaceName,
                              b.JobDetailCode,
                              b.ContainerNo,
                              CostValue = c.AdvanceValue
                          }).ToListAsync();

        return rows.Select(s => new JobViewModel
        {
            job_code = s.JobCode,
            job_date = s.JobDate.ToString("yyyy-MM-dd"),
            job_date_value = s.JobDate,
            job_type = s.JobType,
            booking_no = s.BookingNo,
            customer_name = s.CustomerName,
            container_type = s.ContainerType,
            container_size = s.ContainerSizeName,
            mission_name = s.MissionName,
            job_detail_code = s.JobDetailCode,
            container_no = s.ContainerNo,
            cost_value = s.CostValue
        }).ToList();
    }

    public async Task<TblShippingpayment> GetClearingDataByCode(string code)
    {
        var data = await db.TblShippingpayments.AsNoTracking().Where(w => w.PaymentCode == code).FirstOrDefaultAsync();
        return data;
    }

    /*public TblAllowance GetAllowanceByCode(string code)
    {
        var data = db.TblAllowance.Where(w => w.AllowanceCode == code).FirstOrDefault();
        return data;
    }


    public TblSupplyer GetSupplyerData(string code)
    {
        var data = db.TblSupplyer.Where(w => w.SupplyerCode == code).FirstOrDefault();
        return data;
    }


    

    public PND50ViewModel GetPND50Data(string code)
    {
        var data = (from a in db.TblSupplyerpayment
                    join b in db.TblSupplyer on a.SupplyerCode equals b.SupplyerCode
                    where a.SupplyerpaymentCode == code
                    select new PND50ViewModel
                    {
                        code = a.SupplyerpaymentCode,
                        supplyer_name = b.SupplyerName,
                        supplyer_address = b.SupplyerAddress,
                        supplyer_vat_no = b.SupplyerVatNo,
                        total_value = a.SupplyerpaymentNetTotal,
                        vat = (a.SupplyerpaymentNetTotal * 3) / 100
                    }).FirstOrDefault();
        return data;
    }*/
}