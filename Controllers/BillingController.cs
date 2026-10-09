using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TMSWeb.Controllers;

[Authorize(Roles = "Administrator")]
public class BillingController : Controller
{
    private readonly BillingRepository billingRepo;
    private readonly CodeRepository codeRepo;
    private readonly DropListRepository dropListRepo;
    private string LoggedInUser => User.Identity.Name;

    public BillingController(BillingRepository _billingRepo, DropListRepository _dropListRepo, CodeRepository _codeRepo)
    {
        billingRepo = _billingRepo;
        dropListRepo = _dropListRepo;
        codeRepo = _codeRepo;
    }

    public async Task<IActionResult> Index()
    {
        var data = await billingRepo.GetTotalUnBilling();
        return View();
    }

    public async Task<IActionResult> GetBillingDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await billingRepo.GetListBillingData();
        var data = query.Where(w => (iFilter.filterCode == null || w.billing_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterCode == null || w.billing_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterMission == null || w.mission_name.Contains(iFilter.filterMission.ToUpper())) &&
        (iFilter.filterCustomer == null || w.customer_name.Contains(iFilter.filterCustomer.ToUpper())));

        Func<BillingViewModel, string> orderingFunction = (c => param.iSortCol_0 == 2 ? c.billing_code :
                                                                param.iSortCol_0 == 3 ? c.customer_name :
                                                                param.iSortCol_0 == 4 ? c.billing_date : c.billing_code);

        IEnumerable<BillingViewModel> listQuery;
        if (param.iSortCol_0 == 2)
        {
            if (param.sSortDir_0 == "asc")
            {
                listQuery = data.OrderBy(o => o.billing_code)
                                .Skip(param.iDisplayStart)
                                .Take(param.iDisplayLength);
            }
            else
            {
                listQuery = data.OrderByDescending(o => o.billing_code)
                                .Skip(param.iDisplayStart)
                                .Take(param.iDisplayLength);
            }
        }
        else
        {
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
        }

        return Json(new
        {
            sEcho = param.sEcho,
            iTotalRecords = data.Count(),
            iTotalDisplayRecords = data.Count(),
            aaData = listQuery
        });
    }

    public async Task<IActionResult> Transport()
    {
        var data = await billingRepo.GetListBilling();
        var model = new TblBillingMaster();
        model.IsAdvance = true;
        model.IsVat = true;
        model.BillingDate = DateOnly.FromDateTime(DateTime.Now);

        ViewBag.Data = data.OrderBy(o => o.job_date_value).ToList();
        return View(model);
    }

    public async Task<IActionResult> AddTransport(TblBillingMaster billingToAdd, string[] listChecked, decimal[] listValue)
    {
        string username = LoggedInUser;

        billingToAdd.BillingCode = await codeRepo.GetBillingCode();
        billingToAdd.CreateDate = DateTime.Now;
        billingToAdd.CreateBy = username;
        billingToAdd.UpdateDate = DateTime.Now;
        billingToAdd.UpdateBy = username;

        ResponseViewModel result = await billingRepo.AddBilling(billingToAdd, listChecked, listValue);
        return Json(result);
    }

    public async Task<IActionResult> Advance()
    {
        var data = await billingRepo.GetListAdvance();
        var model = new TblBillingMaster();
        model.BillingDate = DateOnly.FromDateTime(DateTime.Now);

        ViewBag.Data = data.OrderBy(o => o.job_date).ToList();
        return View(model);
    }

    public async Task<IActionResult> AddAdvance(TblBillingMaster billingToAdd, string[] listChecked, decimal[] listValue)
    {
        string username = LoggedInUser;
        billingToAdd.BillingCode = await codeRepo.GetBillingCode();
        billingToAdd.CreateDate = DateTime.Now;
        billingToAdd.CreateBy = username;
        billingToAdd.UpdateDate = DateTime.Now;
        billingToAdd.UpdateBy = username;
        billingToAdd.IsAdvance = true;

        ResponseViewModel result = await billingRepo.AddAdvance(billingToAdd, listChecked, listValue);
        return Json(result);
    }

    public async Task<IActionResult> Other()
    {
        var data = await billingRepo.GetListOther();
        var model = new TblBillingMaster();
        model.BillingDate = DateOnly.FromDateTime(DateTime.Now);

        ViewBag.Data = data.OrderBy(o => o.job_date).ToList();
        return View(model);
    }

    public async Task<IActionResult> AddOther(TblBillingMaster billingToAdd, int[] listChecked, decimal[] listValue)
    {
        string username = LoggedInUser;
        billingToAdd.BillingCode = await codeRepo.GetBillingCode();
        billingToAdd.CreateDate = DateTime.Now;
        billingToAdd.CreateBy = username;
        billingToAdd.UpdateDate = DateTime.Now;
        billingToAdd.UpdateBy = username;
        billingToAdd.IsAdvance = true;

        ResponseViewModel result = await billingRepo.AddOther(billingToAdd, listChecked, listValue);
        return Json(result);
    }

    public async Task<IActionResult> EditTransport(string code)
    {
        var model = await billingRepo.GetBillingDataByCode(code);
        var data = await billingRepo.GetListBilling();
        var check = await billingRepo.GetListBillingByCode(code);

        model.BillingTotal = model.BillingTotal - model.AdvanceTotal;
        ViewBag.Data = data.OrderBy(o => o.job_date_value).ToList();
        ViewBag.Check = check;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditTransportData(TblBillingMaster billingToEdit, string[] listChecked, decimal[] listValue)
    {
        string username = LoggedInUser;
        billingToEdit.UpdateDate = DateTime.Now;
        billingToEdit.UpdateBy = username;

        ResponseViewModel result = await billingRepo.EditTransport(billingToEdit, listChecked, listValue);
        return Json(result);
    }


    public async Task<IActionResult> EditAdvance(string code)
    {
        var model = await billingRepo.GetBillingDataByCode(code);
        var data = await billingRepo.GetListAdvance();
        var check = await billingRepo.GetListAdvanceByBillingCode(code);
        ViewBag.Data = data.OrderBy(o => o.job_date).ToList();
        ViewBag.Check = check.OrderBy(o => o.job_date).ToList();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditAdvanceData(TblBillingMaster model, string[] listChecked, decimal[] listValue)
    {
        string username = LoggedInUser;

        model.UpdateDate = DateTime.Now;
        model.UpdateBy = username;

        ResponseViewModel result = await billingRepo.EditAdvance(model, listChecked, listValue);
        return Json(result);
    }

    public async Task<IActionResult> EditAdvanceOther(string code)
    {
        var model = await billingRepo.GetBillingDataByCode(code);
        var data = await billingRepo.GetListAdvanceOther();
        var check = await billingRepo.GetListAdvanceOtherByBillingCode(code);

        ViewBag.Data = data.OrderBy(o => o.job_date).ToList();
        ViewBag.Check = check.OrderBy(o => o.job_date).ToList();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditAdvanceOtherData(TblBillingMaster billingToEdit, int[] listChecked, decimal[] listValue)
    {
        string username = LoggedInUser;

        billingToEdit.UpdateDate = DateTime.Now;
        billingToEdit.UpdateBy = username;

        ResponseViewModel result = await billingRepo.EditAdvanceOther(billingToEdit, listChecked, listValue);
        return Json(result);
    }

    public async Task<IActionResult> Detail(string code)
    {
        var data = await billingRepo.GetListBillingByCode(code);
        return View(data);
    }

    public async Task<IActionResult> DetailOther(string code)
    {
        var data = await billingRepo.GetListBillingOtherByCode(code);
        return View(data);
    }

    public async Task<IActionResult> RemoveBilling(string billing_code)
    {
        string username = LoggedInUser;
        ResponseViewModel result = await billingRepo.RemoveBilling(billing_code, username);
        return Json(result);
    }

    public async Task<IActionResult> GetAdvanceByCode(string code)
    {
        var result = await billingRepo.GetJobAdvanceByCode(code);

        return Json(result);
    }

    public async Task<IActionResult> EditBookingData(string job_detail_code, string value)
    {
        ResponseViewModel result = await billingRepo.EditBookingData(value, job_detail_code);
        return Json(result);
    }

    public async Task<IActionResult> EditInvoiceData(string job_detail_code, string value)
    {
        ResponseViewModel result = await billingRepo.EditInvoiceData(value, job_detail_code);
        return Json(result);
    }

    public async Task<IActionResult> EditContainerData(string job_detail_code, string value)
    {
        ResponseViewModel result = await billingRepo.EditContainerData(value, job_detail_code);
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> EditAdvanceDetailData(TblJobadvance model, string billing_code)
    {
        var username = LoggedInUser;
        model.UpdateBy = username;

        ResponseViewModel result = await billingRepo.EditAdvanceDataDetail(model, billing_code);
        return Json(result);
    }

    public async Task<IActionResult> EditTransportDetailData(decimal value, string job_detail_code, string billing_code)
    {
        var username = LoggedInUser;

        ResponseViewModel result = await billingRepo.EditTransportDataDetail(value, job_detail_code, billing_code, username);
        return Json(result);
    }

    public async Task<IActionResult> EditAdvanceOtherDetailData(decimal value, int id)
    {
        var username = LoggedInUser;

        ResponseViewModel result = await billingRepo.EditOtherDataDetail(id, value, username);
        return Json(result);
    }

    public IActionResult List()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    public async Task<JsonResult> GetAllBillingDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await billingRepo.GetListAllBillingData(iFilter.month, iFilter.year);
        var data = query.Where(w => ((iFilter.filterCode == null || w.billing_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterCustomer == null || w.customer_name.Contains(iFilter.filterCustomer.ToUpper())) &&
        (iFilter.filterMission == null || w.mission_name.Contains(iFilter.filterMission.ToUpper()))));

        Func<BillingViewModel, string> orderingFunction = (c => param.iSortCol_0 == 2 ? c.billing_code :
                                                                param.iSortCol_0 == 3 ? c.customer_name :
                                                                param.iSortCol_0 == 4 ? c.billing_date :
                                                                param.iSortCol_0 == 5 ? c.clearing_date : c.billing_code);

        IEnumerable<BillingViewModel> listQuery;
        if (param.sSortDir_0 == "desc")
        {
            listQuery = data.OrderBy(orderingFunction)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength);
            if (param.iSortCol_0 == 5)
            {
                listQuery = data.OrderByDescending(o => o.billing_amount).ToList();
            }
            else if (param.iSortCol_0 == 6)
            {
                listQuery = data.OrderByDescending(o => o.billing_amount).ToList();
            }
        }
        else
        {
            listQuery = data.OrderByDescending(orderingFunction)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength);
            if (param.iSortCol_0 == 5)
            {
                listQuery = data.OrderBy(o => o.billing_amount).ToList();
            }
            else if (param.iSortCol_0 == 6)
            {
                listQuery = data.OrderBy(o => o.billing_amount).ToList();
            }
        }

        return Json(new
        {
            sEcho = param.sEcho,
            iTotalRecords = data.Count(),
            iTotalDisplayRecords = data.Count(),
            aaData = listQuery
        });
    }

    public IActionResult PendingPayment()
    {
        string year = DateTime.Now.ToString("yyyy");
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    public IActionResult PendingDetail(string code, string month, string year)
    {
        ViewBag.Code = code;
        ViewBag.Month = month;
        ViewBag.Year = year;
        return PartialView();
    }

    public async Task<IActionResult> _PendingData(string year)
    {
        var customer = await billingRepo.GetListCustomer(year);
        List<PendingBillViewModel> list = new List<PendingBillViewModel>();
        foreach (var cs in customer)
        {
            var item = new PendingBillViewModel();
            var data = await billingRepo.GetListPending(cs.customer_code, year);
            item.customer_code = cs.customer_code;
            item.customer_name = cs.customer_name;
            foreach (var pd in data)
            {
                switch (pd.month)
                {
                    case "01":
                        item.month_01 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                    case "02":
                        item.month_02 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                    case "03":
                        item.month_03 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                    case "04":
                        item.month_04 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                    case "05":
                        item.month_05 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                    case "06":
                        item.month_06 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                    case "07":
                        item.month_07 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                    case "08":
                        item.month_08 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                    case "09":
                        item.month_09 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                    case "10":
                        item.month_10 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                    case "11":
                        item.month_11 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                    case "12":
                        item.month_12 = (pd.count_pending == null ? 0 : pd.count_pending.Value);
                        break;
                }
            }

            list.Add(item);
        }
        return PartialView(list);
    }

    public async Task<JsonResult> GetAllPendingDataList(jQueryDataTableParamModel param, string month, string year, string code)
    {
        var data = await billingRepo.GetListPendingDataByCustomer(month, year, code);
        if (!(string.IsNullOrEmpty(param.sSearch)))
        {
            data = data.Where(w => w.billing_code.ToUpper().Contains(param.sSearch.ToUpper()) ||
                                w.customer_name.Contains(param.sSearch)).ToList();
        }
        Func<BillingViewModel, string> orderingFunction = (c => param.iSortCol_0 == 2 ? c.billing_code :
                                                                param.iSortCol_0 == 3 ? c.customer_name :
                                                                param.iSortCol_0 == 4 ? c.billing_date :
                                                                param.iSortCol_0 == 5 ? c.clearing_date : c.billing_code);

        IEnumerable<BillingViewModel> listQuery;
        if (param.sSortDir_0 == "desc")
        {
            listQuery = data.OrderBy(orderingFunction)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength);
            if (param.iSortCol_0 == 5)
            {
                listQuery = data.OrderByDescending(o => o.billing_amount).ToList();
            }
            else if (param.iSortCol_0 == 6)
            {
                listQuery = data.OrderByDescending(o => o.billing_amount).ToList();
            }
        }
        else
        {
            listQuery = data.OrderByDescending(orderingFunction)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength);
            if (param.iSortCol_0 == 5)
            {
                listQuery = data.OrderBy(o => o.billing_amount).ToList();
            }
            else if (param.iSortCol_0 == 6)
            {
                listQuery = data.OrderBy(o => o.billing_amount).ToList();
            }
        }

        return Json(new
        {
            sEcho = param.sEcho,
            iTotalRecords = data.Count(),
            iTotalDisplayRecords = data.Count(),
            aaData = listQuery
        });
    }
}