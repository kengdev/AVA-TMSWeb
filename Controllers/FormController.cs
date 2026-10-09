using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace TMSWeb.Controllers;

public class FormController : Controller
{
    private readonly QRCodeRepository qrCodeRepo;
    private readonly FormRepository formRepo;
    private readonly DbavalogContext db;

    public FormController(QRCodeRepository _qrCodeRepo, FormRepository _formRepo, DbavalogContext _db)
    {
        qrCodeRepo = _qrCodeRepo;
        formRepo = _formRepo;
        db = _db;
    }

    public async Task<IActionResult> Order(string code)
    {
        var model = new OrderFormViewModel();
        var data = await formRepo.GetJobOrderByCode(code);
        //var truck = rpt.GetTruckByJobCode(code);

        //model.truck_license = truck.truck_license;
        //model.trailer_license = truck.trailer_license;
        model.truck_typegroup_code = data.TruckTypegroupCode;
        //model.driver_fullname = truck.fullname;
        model.other_address = data.OtherAddress;
        model.port_no = data.PortCode;
        model.industry = data.IndustryNameTh;
        model.job_detail_code = data.JobDetailCode;
        model.container_size_code = data.ContainerSizeCode;
        model.container_size = data.ContainerSizeName;
        model.job_type = data.JobType;
        model.mission_name = data.MissionName;
        model.zone_name = data.ZoneName;
        model.receipt_name = data.ReceiptName;
        model.receipt_tax = data.ReceiptTax;
        model.receipt_address = data.ReceiptAddress;
        model.booking_no = data.BookingNo;
        model.agent_code = data.AgentCode;
        model.start_name = data.LiftonName;
        model.finish_name = data.LiftoffName;
        model.closing_time = data.ClosingTime;
        model.job_date = data.JobDate;
        model.job_invoice = data.JobInvoice;
        model.job_remark = data.JobRemark;
        model.is2short = data.Is2short;
        model.fuel_rate = data.FuelValue;
        model.lift = data.LiftonValue;
        model.port = data.PortValue;
        model.yard = data.YardValue;
        model.express_way = data.ExpresswayValue;
        model.barcode = qrCodeRepo.JobCodeURL(data.HashCode);

        return new ViewAsPdf("_Order", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> OrderAssignment(string code)
    {
        var model = new OrderFormViewModel();
        var data = await formRepo.GetJobOrderByCode(code);
        var cashing = await formRepo.GetJobcashingBycode(code);
        var truck = await formRepo.GetTruckByJobCode(code);
        var ocean = await formRepo.GetJobOceanByCode(data.JobCode);
        string booking = "";
        string delivery = "";
        if (ocean.Count() > 0)
        {
            for (int i = 0; i < ocean.Count(); i++)
            {
                booking += ", " + ocean[i].BlNo;
                if (i < 1)
                {
                    delivery += ", " + ocean[i].DeliveryDate.ToString("HH:mm");
                }
                else
                {
                    if (ocean[i].DeliveryDate != ocean[i - 1].DeliveryDate)
                    {
                        delivery += ", " + ocean[i].DeliveryDate.ToString("HH:mm");
                    }
                }
            }

            delivery = delivery.Substring(1);
            booking = booking.Substring(1);
        }
        else
        {
            delivery = data.JobDate.ToString("HH:mm");
            booking = data.BookingNo;
        }

        model.other_address = data.OtherAddress;
        model.fleet_type = truck.fleet_type;
        model.truck_license = truck.truck_license;
        model.trailer_license = truck.trailer_license;
        model.truck_typegroup_code = data.TruckTypegroupCode;
        model.driver_fullname = truck.fullname;
        model.port_no = data.PortCode;
        model.industry = data.IndustryNameTh;
        model.job_detail_code = data.JobDetailCode;
        model.container_size_code = data.ContainerSizeCode;
        model.container_size = data.ContainerSizeName;
        model.job_type = data.JobType;
        model.mission_name = data.MissionName;
        model.zone_name = data.ZoneName;
        model.receipt_name = data.ReceiptName;
        model.receipt_tax = data.ReceiptTax;
        model.receipt_address = data.ReceiptAddress;
        model.booking_no = booking;
        model.agent_code = data.AgentCode;
        model.start_name = data.LiftonName;
        model.finish_name = data.LiftoffName;
        model.closing_time = data.ClosingTime;
        model.job_date = data.JobDate;
        model.delivery_date = delivery;
        model.job_invoice = data.JobInvoice;
        model.job_remark = data.JobRemark;
        model.is2short = data.Is2short;
        model.fuel_rate = truck.fleet_type == "F1" ? cashing.CashingFuel : 0;
        model.lift = data.LiftonValue;
        model.port = data.PortValue;
        model.yard = data.YardValue;
        model.express_way = data.ExpresswayValue;
        model.barcode = qrCodeRepo.JobCodeURL(data.HashCode);

        return new ViewAsPdf("_OrderAssignment", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> DomesticAssignment(string code)
    {
        var model = new OrderFormViewModel();
        var data = await formRepo.GetJobOrderByCode(code);
        var cashing = await formRepo.GetJobcashingBycode(code);
        var truck = await formRepo.GetTruckByJobCode(code);
        string booking = "";
        string delivery = "";

        delivery = data.JobDate.ToString("HH:mm");
        booking = data.BookingNo;
        model.other_address = data.OtherAddress;
        model.fleet_type = truck.fleet_type;
        model.truck_license = truck.truck_license;
        model.trailer_license = truck.trailer_license;
        model.truck_typegroup_code = data.TruckTypegroupCode;
        model.driver_fullname = truck.fullname;
        model.port_no = data.PortCode;
        model.industry = data.IndustryNameTh;
        model.job_detail_code = data.JobDetailCode;
        model.container_size_code = data.ContainerSizeCode;
        model.container_size = data.ContainerSizeName;
        model.job_type = data.JobType;
        model.mission_name = data.MissionName;
        model.zone_name = data.ZoneName;
        model.receipt_name = data.ReceiptName;
        model.receipt_tax = data.ReceiptTax;
        model.receipt_address = data.ReceiptAddress;
        model.booking_no = booking;
        model.agent_code = data.AgentCode;
        model.start_name = data.LiftonName;
        model.finish_name = data.LiftoffName;
        model.closing_time = data.ClosingTime;
        model.job_date = data.JobDate;
        model.delivery_date = delivery;
        model.job_invoice = data.JobInvoice;
        model.job_remark = data.JobRemark;
        model.is2short = data.Is2short;
        model.fuel_rate = truck.fleet_type == "F1" ? cashing.CashingFuel : 0;
        model.lift = data.LiftonValue;
        model.port = data.PortValue;
        model.yard = data.YardValue;
        model.express_way = data.ExpresswayValue;
        model.barcode = qrCodeRepo.JobCodeURL(data.HashCode);

        return new ViewAsPdf("_DomesticAssignment", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> TruckOrder(string code)
    {
        var data = await formRepo.GetJobOrderByCode(code);
        var truck = await formRepo.GetTruckByJobCode(code);

        ViewBag.Truck = truck;
        ViewBag.Data = data;
        ViewBag.Code = code;
        return View();
    }

    public async Task<IActionResult> _TruckOrder(string code)
    {
        var data = await formRepo.GetJobOrderByCode(code);
        var truck = await formRepo.GetTruckByJobCode(code);

        ViewBag.Truck = truck;
        ViewBag.Data = data;
        return View();
    }

    public async Task<IActionResult> BillingPdf(string code, string col)
    {
        var model = new BillingPdfFormViewModel();
        var data = await formRepo.GetBilling(code);
        var advance = await formRepo.GetAdvance(code);
        var item = data.FirstOrDefault();
        var customer = await formRepo.GetCustomerByCode(item.customer_code);

        model.col = col;
        model.billing = data.OrderBy(o => o.job_date).ToList();
        model.advance = advance.OrderBy(o => o.job_date).ToList();
        model.customer = customer;

        var filename = "invoice-" + DateTime.Now.ToString("ddMMyy") + ".pdf";
        return new Rotativa.AspNetCore.ViewAsPdf("_BillingPdf", model)
        {
            FileName = filename,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            IsLowQuality = true,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageMargins = { Left = 2, Bottom = 2, Right = 2, Top = 7 },
            CustomSwitches = "--disable-smart-shrinking --no-outline"
        };
    }

    public async Task<IActionResult> AdvancePdf(string code, string col, string html)
    {
        var model = new BillingPdfFormViewModel();
        var data = await formRepo.GetBilling(code);
        var advance = await formRepo.GetAdvance(code);
        var item = data.OrderBy(o => o.job_date).FirstOrDefault();
        var customer = await formRepo.GetCustomerByCode(item.customer_code);
        model.col = col;
        model.billing = data.OrderBy(o => o.job_date).ToList();
        model.advance = advance.OrderBy(o => o.job_date).ToList();
        model.customer = customer;

        var filename = "invoice-" + DateTime.Now.ToString("ddMMyy") + ".pdf";
        return new Rotativa.AspNetCore.ViewAsPdf("_AdvancePdf", model)
        {
            FileName = filename,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            IsLowQuality = true,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageMargins = { Left = 2, Bottom = 2, Right = 2, Top = 7 },
            CustomSwitches = "--disable-smart-shrinking --no-outline"
        };
    }

    public async Task<IActionResult> TransportPdf(string code, string col)
    {
        var model = new BillingPdfFormViewModel();
        var data = await formRepo.GetBilling(code);
        var item = data.OrderBy(o => o.job_date).FirstOrDefault();
        var customer = await formRepo.GetCustomerByCode(item.customer_code);

        model.col = col;
        model.billing = data;
        model.customer = customer;

        var filename = "invoice-" + DateTime.Now.ToString("ddMMyy") + ".pdf";
        return new ViewAsPdf("_InvoiceTransportPdf", model)
        {
            FileName = filename,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            IsLowQuality = true,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageMargins = { Left = 2, Bottom = 2, Right = 2, Top = 7 },
            CustomSwitches = "--disable-smart-shrinking --no-outline"
        };
    }

    public async Task<IActionResult> AdvanceOtherPdf(string code, string col)
    {
        var model = new BillingPdfFormViewModel();
        var data = await formRepo.GetBillingOther(code);
        var advance = await formRepo.GetAdvanceOther(code);
        var item = data.OrderBy(o => o.job_date).FirstOrDefault();
        var customer = await formRepo.GetCustomerByCode(item.customer_code);
        model.col = col;
        model.billing = data.OrderBy(o => o.job_date).ToList();
        model.advance = advance.OrderBy(o => o.job_date).ToList();
        model.customer = customer;

        var filename = "invoice-" + DateTime.Now.ToString("ddMMyy") + ".pdf";
        return new ViewAsPdf("_AdvanceOtherPdf", model)
        {
            FileName = filename,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            IsLowQuality = true,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageMargins = { Left = 2, Bottom = 2, Right = 2, Top = 7 },
            CustomSwitches = "--disable-smart-shrinking --no-outline"
        };
    }

    public async Task<IActionResult> Coversheet(string code)
    {
        var model = new CoverSheetPdfFormViewModel();
        var data = await formRepo.GetReportCoverSheet(code);
        var item = data.FirstOrDefault();
        var customer = await formRepo.GetCustomerByCode(item.customer_code);

        model.coversheet = data.OrderBy(o => o.billing_code).ToList();
        model.customer = customer;

        //var filename = "coversheet-" + DateTime.Now.ToString("ddMMyy") + ".pdf";
        return new ViewAsPdf("_CoverSheetPdf", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> Receipt(string code)
    {
        var data = await formRepo.GetReceiptByCode(code);
        var customer = await formRepo.GetCustomerByCode(data.CustomerCode);
        var rule = await formRepo.GetCustomerRule(data.CustomerCode);
        var mos = await formRepo.GetReceiptValue(code);
        var model = new ReceiptFormViewModel();

        model.customer_name = data.CustomerName;
        model.customer_address = data.CustomerAddress;
        model.customer_tax = data.CustomerTax;
        model.create_date = data.CreateDate;
        model.receipt_code = data.ReceiptCode;
        model.receipt_part_code = data.ReceiptPartCode;
        model.receipt_name = data.ReceiptName;
        model.receipt_type = data.ReceiptType;
        model.receipt_date = data.ReceiptDate;
        model.bank_name = data.BankName;
        model.bank_branch_name = data.BankBranchName;
        model.cheque_no = data.ChequeNo;
        model.receipt_total = data.ReceiptTotal;
        model.receipt_remark = data.ReceiptRemark;
        model.transport_value = data.ReceiptTransport;
        model.advance_value = data.ReceiptAdvance;
        model.is_excharge = data.IsExcharge;
        model.excharge_topic = data.ExchargeTopic;

        ViewBag.Data = model;
        return PartialView();
    }

    public async Task<IActionResult> _Receipt(string code)
    {
        var data = await formRepo.GetReceiptByCode(code);
        var customer = await formRepo.GetCustomerByCode(data.CustomerCode);
        var rule = await formRepo.GetCustomerRule(data.CustomerCode);
        var mos = await formRepo.GetReceiptValue(code);
        var model = new ReceiptFormViewModel();

        model.customer_name = data.CustomerName;
        model.customer_address = data.CustomerAddress;
        model.customer_tax = data.CustomerTax;
        model.create_date = data.CreateDate;
        model.receipt_code = data.ReceiptCode;
        model.receipt_part_code = data.ReceiptPartCode;
        model.receipt_name = data.ReceiptName;
        model.receipt_type = data.ReceiptType;
        model.receipt_date = data.ReceiptDate;
        model.bank_name = data.BankName;
        model.bank_branch_name = data.BankBranchName;
        model.cheque_no = data.ChequeNo;
        model.receipt_total = data.ReceiptTotal;
        model.receipt_remark = data.ReceiptRemark;
        model.transport_value = data.ReceiptTransport;
        model.advance_value = data.ReceiptAdvance;
        model.is_excharge = data.IsExcharge;
        model.excharge_topic = data.ExchargeTopic;


        return new ViewAsPdf("_ReceiptPdf", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 10 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> GetReceiptPDFList(string month, string year)
    {
        var data = await formRepo.GetReceiptByMonth(month, year);
        var list = new List<ReceiptFormViewModel>();
        foreach (var item in data)
        {
            var customer = await formRepo.GetCustomerByCode(item.CustomerCode);
            var rule = await formRepo.GetCustomerRule(item.CustomerCode);
            var mos = await formRepo.GetReceiptValue(item.ReceiptCode);
            var model = new ReceiptFormViewModel();

            model.customer_name = item.CustomerName;
            model.customer_address = item.CustomerAddress;
            model.customer_tax = item.CustomerTax;
            model.create_date = item.CreateDate;
            model.receipt_code = item.ReceiptCode;
            model.receipt_part_code = item.ReceiptPartCode;
            model.receipt_name = item.ReceiptName;
            model.receipt_type = item.ReceiptType;
            model.receipt_date = item.ReceiptDate;
            model.bank_name = item.BankName;
            model.bank_branch_name = item.BankBranchName;
            model.cheque_no = item.ChequeNo;
            model.receipt_total = item.ReceiptTotal;
            model.receipt_remark = item.ReceiptRemark;
            model.transport_value = item.ReceiptTransport;
            model.advance_value = item.ReceiptAdvance;
            model.is_excharge = item.IsExcharge;
            model.excharge_topic = item.ExchargeTopic;

            list.Add(model);
        }

        return new ViewAsPdf("_ReceiptListPdf", list)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 10 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> PageReceiptPdf(string code)
    {
        var data = await formRepo.GetReceiptByCode(code);
        var customer = await formRepo.GetCustomerByCode(data.CustomerCode);
        var rule = await formRepo.GetCustomerRule(data.CustomerCode);
        var mos = await formRepo.GetReceiptValue(code);
        var model = new ReceiptFormViewModel();

        model.customer_name = data.CustomerName;
        model.customer_address = data.CustomerAddress;
        model.customer_tax = data.CustomerTax;
        model.create_date = data.CreateDate;
        model.receipt_code = data.ReceiptCode;
        model.receipt_part_code = data.ReceiptPartCode;
        model.receipt_name = data.ReceiptName;
        model.receipt_type = data.ReceiptType;
        model.receipt_date = data.ReceiptDate;
        model.bank_name = data.BankName;
        model.bank_branch_name = data.BankBranchName;
        model.cheque_no = data.ChequeNo;
        model.receipt_total = data.ReceiptTotal;
        model.receipt_remark = data.ReceiptRemark;
        model.transport_value = data.ReceiptTransport;
        model.advance_value = data.ReceiptAdvance;
        model.is_excharge = data.IsExcharge;
        model.excharge_topic = data.ExchargeTopic;

        /*foreach (var item in mos)
        {
            if (rule.IsTransportOther == true)
            {
                if (item.billing_type == "T" || item.billing_type == "O")
                {
                    model.transport_value = model.transport_value + item.transport_value + item.advance_value;
                }
                else if (item.billing_type == "A")
                {
                    model.advance_value = model.advance_value + item.advance_value;
                }
                else if (item.billing_type == "M")
                {
                    model.transport_value = model.transport_value + item.transport_value;
                    model.advance_value = model.advance_value + item.advance_value;
                }
            }
            else
            {
                if (item.billing_type == "T")
                {
                    model.transport_value = model.transport_value + item.transport_value;
                }
                else if (item.billing_type == "A" || item.billing_type == "O")
                {
                    model.advance_value = model.advance_value + item.advance_value;
                }
                else if (item.billing_type == "M")
                {
                    model.transport_value = model.transport_value + item.transport_value;
                    model.advance_value = model.advance_value + item.advance_value;
                }
            }
        }*/
        return new ViewAsPdf("_ReceiptPdf", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> Ceva(string code)
    {
        var data = await formRepo.GetOceanJob(code);

        return new ViewAsPdf("_CevaForm", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> CevaAssignment(string code)
    {
        var data = await formRepo.GetOceanJobAssignment(code);

        return new ViewAsPdf("_CevaFormAssignment", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> Triumph(string code)
    {
        var data = await formRepo.GetOceanJob(code);

        return new ViewAsPdf("_TriumphForm", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> DriverPayment(string code, string driver_code)
    {
        var list = new List<DriverPaymentFormViewModel>();
        var model = new DriverPaymentFormViewModel();
        var data = await formRepo.GetPaymentList(code, driver_code);
        var driver_allowance = await formRepo.GetAllowanceByCode(code);
        var allowance = await formRepo.GetAllowanceDetailByCode(code);
        var driver = await formRepo.GetDriverByCode(driver_code);
        var special = await formRepo.GetSubtractList(code, driver_code);
        var substract = special.Where(w => w.type == "sub").ToList();
        var addit = special.Where(w => w.type == "add").ToList();

        model.data = data;
        model.driver_name = driver.DriverFullname;
        model.truck_license = driver.TruckLicense;
        model.truck_no = driver.TruckNo;
        model.trip_value = allowance.Sum(s => s.TripValue);
        model.fuel_value = allowance.Sum(s => s.FuelValue);
        model.waste_value = allowance.Sum(s => s.WasteValue);
        model.other_value = allowance.Sum(s => s.OtherValue);
        model.total_substract = substract.Sum(s => s.value);
        model.total_addit = addit.Sum(s => s.value);
        model.allowance = allowance;
        model.substract = substract;
        model.addit = addit;
        model.net_total = driver_allowance.AllowanceTotal;
        list.Add(model);

        return new ViewAsPdf("_DriverPaymentPdf", list)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> DriverPaymentList(DateOnly start, DateOnly end)
    {
        var list = new List<DriverPaymentFormViewModel>();
        var allowance = await formRepo.GetPaymentListByDate(start, end);
        foreach (var item in allowance)
        {
            var model = new DriverPaymentFormViewModel();
            var data = await formRepo.GetPaymentList(item.AllowanceCode, item.DriverCode);
            var detail = await formRepo.GetAllowanceDetailByCode(item.AllowanceCode);
            var driver = await formRepo.GetDriverByCode(item.DriverCode);
            var special = await formRepo.GetSubtractList(item.AllowanceCode, item.DriverCode);
            var substract = special.Where(w => w.type == "sub").ToList();
            var addit = special.Where(w => w.type == "add").ToList();

            model.driver_name = driver.DriverFullname;
            model.truck_license = driver.TruckLicense;
            model.truck_no = driver.TruckNo;
            model.data = data;
            model.trip_value = detail.Sum(s => s.TripValue);
            model.fuel_value = detail.Sum(s => s.FuelValue);
            model.waste_value = detail.Sum(s => s.WasteValue);
            model.other_value = detail.Sum(s => s.OtherValue);
            model.total_substract = substract.Sum(s => s.value);
            model.total_addit = addit.Sum(s => s.value);
            model.allowance = detail;
            model.substract = substract;
            model.addit = addit;
            model.net_total = item.AllowanceTotal;
            list.Add(model);
        }

        list = list.OrderBy(o => o.truck_no).ToList();
        return new ViewAsPdf("_DriverPaymentPdf", list)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> FleetPayment(string code)
    {
        var data = await formRepo.GetFleetPayment(code);
        return new ViewAsPdf("_FleetAllowance", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> FleetPaymentList(string month, string year)
    {
        var data = await formRepo.GetFleetPaymentList(month, year);

        return new ViewAsPdf("_FleetAllowance", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> SupplierPayment(string code)
    {
        var data = await formRepo.GetSupplierPayment(code);
        var additional = await formRepo.GetSupplierAdditional(data.supplierpayment_code);

        data.addit = additional;
        return new ViewAsPdf("_SupplierPayment", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> PND50(string code)
    {
        var data = await formRepo.GetPND50(code);

        return new ViewAsPdf("_PND50", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> GetPND50PDFList(string month, string year)
    {
        var data = await formRepo.GetPND50ByMonth(month, year);

        return new ViewAsPdf("_PND50", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> ClearingList(string code)
    {
        var model = new ClearingFormViewModel();
        var data = await formRepo.GetClearingDataByCode(code);
        var list = await formRepo.GetClearingDataListByCode(code);

        model.data = data;
        model.list = list;
        return new ViewAsPdf("_ClearingList", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    [AllowAnonymous]
    public async Task<IActionResult> Quotation(string code)
    {
        var data = await (from a in db.TblQuotations.AsNoTracking()
                          where a.QuotationCode == code
                          select new QuotationFormViewModel
                          {
                              quotation_code = a.QuotationCode,
                              start_rate = a.StartRate,
                              customer_name = a.CustomerName,
                              container1 = a.ContainerSize1,
                              container2 = a.ContainerSize2,
                              container3 = a.ContainerSize3,
                              contact = a.ContactName,
                              contact_line = a.ContactLine,
                              start = a.StartDate,
                              expire = a.ExpireDate,
                              route = a.Route,
                              scope = a.Scope,
                              commodity = a.Commodity,
                              quotation_date = a.CreateDate,
                              condition_hour = a.ConditionHour,
                              condition_charged = a.ConditionCharged,
                              minimum_rate = a.MinimumRate,
                              price_increase = a.PriceIncrease,
                              price_length = a.PriceLength,
                              charge_cutchassis = a.ChargeCutchassis,
                              insurance_rate = a.InsuranceRate,
                              remark = a.Remark
                          }).FirstOrDefaultAsync();

        var detail = await db.TblQuotationDetails.AsNoTracking().Where(w => w.QuotationCode == code).OrderBy(o => o.FuelRate).ToListAsync();
        data.detail = detail;

        var condition = await db.TblQuotationConditions.AsNoTracking().Where(w => w.QuotationCode == code).OrderBy(o => o.Id).ToListAsync();
        data.condition = condition;

        return new ViewAsPdf("_Quotation", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }
}