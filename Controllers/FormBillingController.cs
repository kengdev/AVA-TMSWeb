using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;

public class FormBillingController : Controller
{
    private readonly FormBillingRepository frmRepo;

    public FormBillingController(FormBillingRepository _frmRepo)
    {
        frmRepo = _frmRepo;
    }

    #region Billing for all cost
    public async Task<IActionResult> AllPdf(string code)
    {
        var model = new BillingPdfFormViewModel();
        var list = await frmRepo.GetBilling(code);
        var data = await frmRepo.GetBillingDataByCode(code);
        var customer = await frmRepo.GetCustomerByCode(data.CustomerCode);

        model.special_no = data.SpecialNo != null ? "FLBACTH" + data.SpecialNo : "";
        model.billing_code = data.BillingCode;
        model.billing_date = data.BillingDate;
        model.transport_total = data.TransportTotal;
        model.advance_total = data.AdvanceTotal;
        model.vat = data.BillingVat;
        model.net_transport_total = data.IsVat == true ? model.transport_total - data.BillingVat : model.transport_total;
        model.net_total = data.BillingNetTotal;
        model.s_port = list.Sum(s => s.advance_port);
        model.s_lifton = list.Sum(s => s.advance_lifton + s.advance_yard);
        model.s_liftoff = list.Sum(s => s.advance_liftoff);
        model.s_transport = list.Sum(s => s.cost_value);
        model.remark = data.BillingRemark;
        model.tax_id = data.TaxId;
        model.tax_name = data.TaxName;
        model.tax_address = data.TaxAddress;

        model.col = customer.DocType;
        model.billing = list.OrderBy(o => o.job_date).ToList();
        model.customer = customer;

        return new Rotativa.AspNetCore.ViewAsPdf("_AllPdf", model)
        {
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            IsLowQuality = true,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageMargins = { Left = 2, Bottom = 2, Right = 2, Top = 8 },
            CustomSwitches = "--disable-smart-shrinking --no-outline"
        };
    }
    #endregion


    #region Billing for transport cost
    public async Task<IActionResult> TransportPdf(string code)
    {
        var model = new BillingPdfFormViewModel();
        var list = await frmRepo.GetBilling(code);
        var data = await frmRepo.GetBillingDataByCode(code);
        var customer = await frmRepo.GetCustomerByCode(data.CustomerCode);

        model.special_no = data.SpecialNo != null ? "FLBACTH" + data.SpecialNo : "";
        model.billing_code = data.BillingCode;
        model.billing_date = data.BillingDate;
        model.transport_total = data.TransportTotal;
        model.advance_total = data.AdvanceTotal;
        model.vat = data.BillingVat;
        model.net_transport_total = data.IsVat == true ? model.transport_total - data.BillingVat : model.transport_total;
        model.net_total = data.BillingNetTotal;
        model.remark = data.BillingRemark;
        model.tax_id = data.TaxId;
        model.tax_name = data.TaxName;
        model.tax_address = data.TaxAddress;

        model.col = customer.DocType;
        model.billing = list.OrderBy(o => o.job_date).ToList();
        model.customer = customer;

        return new Rotativa.AspNetCore.ViewAsPdf("_TransportPdf", model)
        {
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            IsLowQuality = true,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageMargins = { Left = 2, Bottom = 2, Right = 2, Top = 8 },
            CustomSwitches = "--disable-smart-shrinking --no-outline"
        };
    }
    #endregion


    #region Billing for advance
    public async Task<IActionResult> AdvancePdf(string code)
    {
        var model = new BillingPdfFormViewModel();
        var list = await frmRepo.GetBilling(code);
        var data = await frmRepo.GetBillingDataByCode(code);
        var customer = await frmRepo.GetCustomerByCode(data.CustomerCode);

        model.special_no = data.SpecialNo != null ? "FLBACTH" + data.SpecialNo : "";
        model.billing_code = data.BillingCode;
        model.billing_date = data.BillingDate;
        model.advance_total = data.AdvanceTotal;
        model.net_total = data.BillingNetTotal;
        model.s_port = list.Sum(s => s.advance_port);
        model.s_lifton = list.Sum(s => s.advance_lifton + s.advance_yard);
        model.s_liftoff = list.Sum(s => s.advance_liftoff);
        model.remark = data.BillingRemark;
        model.tax_id = data.TaxId;
        model.tax_name = data.TaxName;
        model.tax_address = data.TaxAddress;

        model.col = customer.DocType;
        model.billing = list.OrderBy(o => o.job_date).ToList();
        model.customer = customer;
        return new Rotativa.AspNetCore.ViewAsPdf("_AdvancePdf", model)
        {
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            IsLowQuality = true,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageMargins = { Left = 2, Bottom = 2, Right = 2, Top = 8 },
            CustomSwitches = "--disable-smart-shrinking --no-outline"
        };
    }
    #endregion


    #region Billing for other
    public async Task<IActionResult> OtherPdf(string code)
    {
        var model = new BillingPdfFormViewModel();
        var list = await frmRepo.GetAdvanceOther(code);
        var data = await frmRepo.GetBillingDataByCode(code);
        var customer = await frmRepo.GetCustomerByCode(data.CustomerCode);

        model.billing_code = code;
        model.billing_date = data.BillingDate;
        model.net_total = data.BillingNetTotal;
        model.col = customer.DocType;
        model.advance = list.OrderBy(o => o.job_date).ToList();
        model.customer = customer;
        model.remark = data.BillingRemark;
        model.tax_id = data.TaxId;
        model.tax_name = data.TaxName;
        model.tax_address = data.TaxAddress;

        return new ViewAsPdf("_OtherPdf", model)
        {
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            IsLowQuality = true,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageMargins = { Left = 2, Bottom = 2, Right = 2, Top = 8 },
            CustomSwitches = "--disable-smart-shrinking --no-outline"
        };
    }
    #endregion
}