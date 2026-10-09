using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;

[Authorize(Roles = "Administrator, Accounting, Super")]
public class CoverSheetController : Controller
{
    private readonly DropListRepository dropListRepo;
    private readonly CoverSheetRepository coverSheetRepo;
    private readonly FormRepository formRepo;
    private string LoggedInUser => User.Identity.Name;

    public CoverSheetController(DropListRepository _dropListRepo, CoverSheetRepository _coverSheetRepo, FormRepository _formRepo)
    {
        dropListRepo = _dropListRepo;
        coverSheetRepo = _coverSheetRepo;
        formRepo = _formRepo;
    }

    public IActionResult Index()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    public async Task<IActionResult> Add()
    {
        var data = await coverSheetRepo.GetBillingDataList();
        ViewBag.Data = data;
        return View();
    }

    public async Task<IActionResult> Edit(string code)
    {
        var model = await coverSheetRepo.GetCoverSheetData(code);
        var detail = await coverSheetRepo.GetCoverSheetDetailListData(code);
        var data = await coverSheetRepo.GetBillingDataList();
        ViewBag.Data = data;
        ViewBag.Detail = detail;
        return View(model);
    }

    public async Task<IActionResult> EntryEditor(string code)
    {
        var data = await coverSheetRepo.GetBillingBycode(code);
        var model = await coverSheetRepo.GetCoverSheetDetailData(data.billing_code);
        ViewBag.Data = data;
        return PartialView("_EntryCoverDetail", model);
    }

    public async Task<IActionResult> Detail(string code)
    {
        var data = await formRepo.GetCoverSheetByCode(code);
        var customer = await formRepo.GetCustomerByCode(data.CustomerCode);
        var rule = await formRepo.GetCustomerRule(data.CustomerCode);
        var detail = await formRepo.GetCoverSheetValue(code);

        ViewBag.Customer = customer;
        ViewBag.Detail = detail;
        ViewBag.Rule = rule;
        return View(data);
    }

    public async Task<IActionResult> Remove(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await coverSheetRepo.RemoveCoverSheet(code, username);
        return Json(result);
    }

    public async Task<IActionResult> AddCoverSheet(TblCoversheet coverToAdd)
    {
        string username = LoggedInUser;

        coverToAdd.IsEnabled = true;
        coverToAdd.CreateDate = DateTime.Now;
        coverToAdd.CreateBy = username;
        coverToAdd.UpdateDate = DateTime.Now;
        coverToAdd.UpdateBy = username;

        ResponseViewModel result = await coverSheetRepo.AddCoverSheet(coverToAdd);
        return Json(result);
    }

    public IActionResult _EntryCoverDetail(string code)
    {
        return ViewComponent("EntryCoverSheet", code);
    }

    public async Task<IActionResult> EditCoverSheet(TblCoversheet coverToEdit)
    {
        string username = LoggedInUser;

        coverToEdit.UpdateDate = DateTime.Now;
        coverToEdit.UpdateBy = username;

        ResponseViewModel result = await coverSheetRepo.EditCoverSheet(coverToEdit);
        return Json(result);
    }

    public IActionResult Export(string code, string customer)
    {
        ViewBag.Code = code;
        ViewBag.Customer = customer;
        return View();
    }

    public async Task<IActionResult> GetCoverSheetDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await coverSheetRepo.GetCoverSheetDataList(iFilter.month, iFilter.year);
        var data = query.Where(w => (iFilter.filterCode == null || w.coversheet_code.ToUpper().Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterCustomer == null || w.customer_name.Contains(iFilter.filterCustomer.ToUpper())));

        Func<CoverSheetViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.coversheet_date :
                                                                param.iSortCol_0 == 2 ? c.coversheet_code :
                                                                param.iSortCol_0 == 3 ? c.customer_name : c.customer_name);

        IEnumerable<CoverSheetViewModel> listQuery;
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

    public IActionResult List()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    public async Task<IActionResult> GetAllCoverSheetDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await coverSheetRepo.GetAllCoverSheetDataList(iFilter.month, iFilter.year);
        var data = query.Where(w => (iFilter.filterCode == null || w.coversheet_code.ToUpper().Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterCustomer == null || w.customer_name.Contains(iFilter.filterCustomer.ToUpper())));
        Func<CoverSheetViewModel, string> orderingFunction = (c => param.iSortCol_0 == 2 ? c.coversheet_code :
                                                                param.iSortCol_0 == 3 ? c.customer_name :
                                                                param.iSortCol_0 == 4 ? c.coversheet_code : c.customer_name);

        IEnumerable<CoverSheetViewModel> listQuery;
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

    public async Task<IActionResult> ProveData(string billing_code)
    {
        ResponseViewModel result = await coverSheetRepo.ProveBilling(billing_code);
        return Json(result);
    }
}