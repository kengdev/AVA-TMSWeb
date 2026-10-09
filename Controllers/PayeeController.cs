using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using TMSWeb.Models.Entities.HR;

namespace TMSWeb.Controllers;

public class PayeeController : Controller
{
    private readonly PayeeRepository payeeRepo;
    private readonly DropListRepository dropListRepo;
    private readonly CodeRepository codeRepo;
    private string LoggedInUser => User.Identity.Name;
    public PayeeController(PayeeRepository _payeeRepo,
        DropListRepository _dropListRepo,
        CodeRepository _codeRepo)
    {
        payeeRepo = _payeeRepo;
        dropListRepo = _dropListRepo;
        codeRepo = _codeRepo;
    }

    #region Other Payment
    [Authorize(Roles = "Administrator,Accounting")]
    public IActionResult Receive()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    public IActionResult _EntryPayee(string action_type)
    {
        return ViewComponent("EntryPayee", action_type);
    }

    [Authorize(Roles = "Administrator, Accounting, Super")]
    public async Task<IActionResult> AddReceive()
    {
        ViewData["BankName"] = from a in await dropListRepo.GetBank() select new SelectListItem { Value = a.label.ToString(), Text = a.label };
        ViewData["PaymentTypeCode"] = from a in await dropListRepo.GetPaymentType() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddReceivePayment(TblReceivePayment model)
    {
        model.ReceivePaymentCode = await codeRepo.GetReceivePaymentCode();
        model.PaymentTopic = model.TblReceivePaymentDetails.Where(w => w.Description != "" && w.Description != null).ElementAt(0).Description;

        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = this.LoggedInUser;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;
        var result = await payeeRepo.AddReceivePayment(model);

        return Json(result);
    }

    public async Task<IActionResult> EditReceive(string code)
    {
        var model = await payeeRepo.GetReceivePaymentData(code);

        ViewData["PaymentTypeCode"] = from a in await dropListRepo.GetPaymentType() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == model.PaymentTypeCode };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditReceivePayment(TblReceivePayment model)
    {
        var detail = new List<TblReceivePaymentDetail>();
        model.PaymentTopic = model.TblReceivePaymentDetails.Where(w => w.Description != "" && w.Description != null).ElementAt(0).Description;

        foreach (var item in model.TblReceivePaymentDetails)
        {
            if (item.Description != null && item.Amount > 0)
            {
                detail.Add(item);
            }
        }

        model.TblReceivePaymentDetails = detail;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;
        var result = await payeeRepo.EditReceivePayment(model);

        return Json(result);
    }

    public async Task<IActionResult> IsPaid(string code)
    {
        var data = await payeeRepo.GetReceivePaymentData(code);

        if (data.IsPaid == true)
        {
            return Json(true);
        }
        else
        {
            return Json(false);
        }
    }

    public async Task<IActionResult> _Receipt(string code)
    {

        ViewBag.ReceivePaymentCode = code;
        ViewData["ReceiptType"] = from a in dropListRepo.GetReceptType() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewData["BankName"] = from a in await dropListRepo.GetBank() select new SelectListItem { Value = a.label.ToString(), Text = a.label };
        ViewData["CBankName"] = from a in await dropListRepo.GetBank() select new SelectListItem { Value = a.label.ToString(), Text = a.label };
        return PartialView("_Receipt", new { code });
    }

    [HttpPost]
    public async Task<IActionResult> Receipt(TblReceivePaymentReceipt model, string CBankName)
    {
        model.ReceiptCode = await codeRepo.GetPayeeReceipt();
        model.BankName = model.PaymentType == "03" ? CBankName : model.BankName;
        model.CreateDate = DateTime.Now;
        model.CreateBy = this.LoggedInUser;
        var response = await payeeRepo.AddReceipt(model);

        return Json(response);
    }

    [HttpPost]
    public async Task<IActionResult> Remove(string code)
    {
        var response = await payeeRepo.RemoveReceivePayment(code, this.LoggedInUser);

        return Json(response);
    }

    public async Task<IActionResult> GetReceivePaymentDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await payeeRepo.GetReceivePaymentDataList(iFilter.month, iFilter.year);
        var data = query.Where(w => (iFilter.filterCode == null || w.payment_id.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterCustomer == null || w.payment_name.Contains(iFilter.filterCustomer.ToUpper())));

        Func<PaymentModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.payment_date :
                                                                param.iSortCol_0 == 2 ? c.payment_id :
                                                                param.iSortCol_0 == 3 ? c.payment_name : c.payment_id);

        IEnumerable<PaymentModel> listQuery;
        if (param.sSortDir_0 == "desc")
        {
            listQuery = data.OrderBy(orderingFunction)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength);
        }
        else
        {
            listQuery = data.OrderByDescending(orderingFunction)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength);
        }

        return Json(new
        {
            sEcho = param.sEcho,
            iTotalRecords = data.Count(),
            iTotalDisplayRecords = data.Count(),
            aaData = listQuery
        });
    }
    #endregion

    private string ConvertMonth(string month)
    {
        switch (month)
        {
            case "01":
                month = "JANUARY";
                break;
            case "02":
                month = "FEBUARY";
                break;
            case "03":
                month = "MARCH";
                break;
            case "04":
                month = "APRIL";
                break;
            case "05":
                month = "MAY";
                break;
            case "06":
                month = "JUNE";
                break;
            case "07":
                month = "JULY";
                break;
            case "08":
                month = "AUGUST";
                break;
            case "09":
                month = "SEPTEMBER";
                break;
            case "10":
                month = "OCTOBER";
                break;
            case "11":
                month = "NOVEMBER";
                break;
            case "12":
                month = "DECEMBER";
                break;
        }

        return month;
    }
}