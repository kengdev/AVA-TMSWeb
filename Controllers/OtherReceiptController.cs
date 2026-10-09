using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace TMSWeb.Controllers;
[Authorize(Roles = "Administrator, Accounting, Super")]
public class OtherReceiptController : Controller
{
    private readonly OtherReceiptRepository receiptRepo;
    private readonly DropListRepository dropListRepo;
    private readonly CodeRepository codeRepo;
    private string LoggedInUser => User.Identity.Name;

    public OtherReceiptController(OtherReceiptRepository _receiptRepo, 
        DropListRepository _dropListRepo, 
        CodeRepository _codeRepo)
    {
        receiptRepo = _receiptRepo;
        dropListRepo = _dropListRepo;
        codeRepo = _codeRepo;
    }
    public IActionResult ReceiptHandling()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    public async Task<IActionResult> AddReceiptHandling()
    {
        ViewData["PaymentType"] = from a in dropListRepo.GetReceptType() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewData["BankName"] = from a in await dropListRepo.GetBank() select new SelectListItem { Value = a.label.ToString(), Text = a.label };
        ViewData["ContainerSize"] = from a in await dropListRepo.GetContainerSize() select new SelectListItem { Value = a.label.ToString(), Text = a.label };
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddReceiptHandling(TblReceiptOther model, TblReceiptOtherDetail detail)
    {
        if (model.ReceiptOtherCode == null)
        {
            model.ReceiptOtherCode = await codeRepo.GetOtherReceipt();
        }

        model.ReceiptOtherType = "01";
        model.Total = detail.Price * detail.Quantity;
        model.Vat = (model.Total * 7) / 100;
        model.NetTotal = model.Total + model.Vat;
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = this.LoggedInUser;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;

        detail.ReceiptOtherCode = model.ReceiptOtherCode;
        detail.ReceiptOtherName = detail.ReceiptOtherName != null ? detail.ReceiptOtherName : "-";
        detail.Description = "CONTAINER HANDLING";
        detail.Total = detail.Quantity * detail.Price;

        model.TblReceiptOtherDetails.Add(detail);
        var result = await receiptRepo.AddReceiptHandling(model);

        return Json(result);
    }

    public async Task<IActionResult> GetReceiptHandlingDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await receiptRepo.GetContainerHandlingDataList(iFilter.month, iFilter.year);
        var data = query.Where(w => (iFilter.filterNameTH == null || w.customer_name.ToUpper().Contains(iFilter.filterNameTH.ToUpper())) &&
                                    (iFilter.filterContainerNo == null || w.container_no.Contains(iFilter.filterContainerNo.ToUpper())));

        Func<ContainerHandlingViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.code :
                                                                param.iSortCol_0 == 2 ? c.receipt_date :
                                                                param.iSortCol_0 == 3 ? c.container_no :
                                                                param.iSortCol_0 == 4 ? c.customer_name : c.code);

        IEnumerable<ContainerHandlingViewModel> listQuery;
        if (param.sSortDir_0 == "asc")
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

    [Authorize(Roles = "Administrator")]
    [HttpGet]
    public async Task<IActionResult> EditReceiptHandling(string code)
    {
        var _receipt = await receiptRepo.GetOtherReceiptDataByCode(code);
        _receipt.TblReceiptOtherDetails = await receiptRepo.GetOtherReceiptDetailDataByCode(code);

        ViewData["PaymentType"] = from a in dropListRepo.GetReceptType() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == _receipt.PaymentType };
        ViewData["BankName"] = from a in await dropListRepo.GetBank() select new SelectListItem { Value = a.label.ToString(), Text = a.label, Selected = a.label == _receipt.BankName };
        ViewData["ContainerSize"] = from a in await dropListRepo.GetContainerSize() select new SelectListItem { Value = a.label.ToString(), Text = a.label, Selected = a.label == _receipt.TblReceiptOtherDetails.FirstOrDefault().ContainerSize };
        return View(_receipt);
    }

    [HttpPost]
    public async Task<IActionResult> EditReceiptHandling(TblReceiptOther model, TblReceiptOtherDetail detail)
    {
        model.Total = detail.Price * detail.Quantity;
        model.Vat = (model.Total * 7) / 100;
        model.NetTotal = model.Total + model.Vat;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;

        detail.ReceiptOtherCode = model.ReceiptOtherCode;
        detail.Description = "CONTAINER HANDLING";
        detail.Total = detail.Quantity * detail.Price;

        model.TblReceiptOtherDetails.Add(detail);
        var result = await receiptRepo.EditReceiptHandling(model);

        return Json(result);
    }

    public async Task<IActionResult> RemoveReceiptHandling(string code)
    {
        var username = LoggedInUser;

        ResponseViewModel result = await receiptRepo.RemoveReceiptHandling(code, LoggedInUser);
        return Json(result);
    }

    private string GetLoggedCode()
    {
        var data = User.Claims.Where(w => w.Type == "code").FirstOrDefault();
        return data.Value;
    }
}