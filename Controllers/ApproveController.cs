using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;
[Authorize(Roles = "Administrator")]
public class ApproveController : Controller
{
    private readonly ApproveRepository approveRepo;
    private readonly OrderRepository orderRepo;
    private readonly DropListRepository dropListRepo;
    private string LoggedInUser => User.Identity.Name;

    public ApproveController(DropListRepository _dropListRepo, OrderRepository _orderRepo, ApproveRepository _approveRepo)
    {
        dropListRepo = _dropListRepo;
        orderRepo = _orderRepo;
        approveRepo = _approveRepo;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["supplier"] = from a in await dropListRepo.GetSupplierTransport() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        return View();
    }

    public async Task<IActionResult> SendBack(string job_detail_code)
    {
        string username = LoggedInUser;
        ResponseViewModel result = await approveRepo.SendBack(job_detail_code, username);

        return Json(result);
    }

    public async Task<IActionResult> Checked(string job_detail_code)
    {
        var data = await orderRepo.GetOrderByCode(job_detail_code);
        var driver = await approveRepo.GetDriverCost(data.DriverCode, data.JobDetailCode);
        var supplyer = await approveRepo.GetSupplyerCost(data.SupplierCode, data.JobDetailCode);
        ViewBag.Data = data;
        ViewBag.Driver = driver;
        ViewBag.Supplyer = supplyer;
        return View();
    }

    public async Task<IActionResult> Confirm(FinishJobModel jobToFinish, TblJobadvance jobAdvance, TblJobcashing jobCashing)
    {
        string username = LoggedInUser;
        jobToFinish.username = username;

        ResponseViewModel result = await approveRepo.ConfirmData(jobToFinish, jobAdvance, jobCashing);
        return Json(result);
    }

    public async Task<IActionResult> Grant(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await approveRepo.Grant(code, this.LoggedInUser);
        return Json(result);
    }

    public async Task<IActionResult> GetJobApproveDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await approveRepo.GetJobApproveList();
        var data = query.Where(w => (iFilter.filterCode == null || w.job_detail_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterMission == null || w.mission_name.Contains(iFilter.filterMission.ToUpper())) &&
        (iFilter.filterBookingNo == null || w.booking_no.Contains(iFilter.filterBookingNo.ToUpper())) &&
        (iFilter.filterSupplier == null || w.supplier_code.Contains(iFilter.filterSupplier.ToUpper())) &&
        (iFilter.filterContainerSize == null || w.container_size.Contains(iFilter.filterContainerSize.ToUpper())) &&
        (iFilter.filterLicense == null || w.truck_license.Contains(iFilter.filterLicense.ToUpper())) &&
        (iFilter.filterStatus == null || w.is_approve == iFilter.filterStatus) &&
        (iFilter.filterContainerNo == null || w.container_no.Contains(iFilter.filterContainerNo.ToUpper())));

        Func<JobViewModel, string> orderingFunction = (c => param.iSortCol_0 == 2 ? c.job_detail_code :
                                                                param.iSortCol_0 == 3 ? c.mission_name :
                                                                param.iSortCol_0 == 4 ? c.booking_no :
                                                                param.iSortCol_0 == 5 ? c.container_no :
                                                                param.iSortCol_0 == 5 ? c.container_size :
                                                                param.iSortCol_0 == 5 ? c.truck_license :
                                                                param.iSortCol_0 == 6 ? c.supplier_name : c.job_detail_code);

        IEnumerable<JobViewModel> listQuery;
        if (param.iSortCol_0 == 1)
        {
            if (param.sSortDir_0 == "asc")
            {
                listQuery = data.OrderBy(o => o.job_date_value)
                                .Skip(param.iDisplayStart)
                                .Take(param.iDisplayLength);
            }
            else
            {
                listQuery = data.OrderByDescending(o => o.job_date_value)
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

    public IActionResult ClearShippingAdvance()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    public async Task<JsonResult> GetShippingAdvanceDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await approveRepo.GetClearingList(iFilter.month, iFilter.year);
        var data = query.Where(w => (iFilter.filterCode == null || w.payment_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterContainerNo == null || w.customer_name.Contains(iFilter.filterContainerNo.ToUpper())));

        Func<ShippingPaymentViewModel, string> orderingFunction = (c => param.iSortCol_0 == 2 ? c.payment_code :
                                                                param.iSortCol_0 == 3 ? c.payment_date :
                                                                param.iSortCol_0 == 6 ? c.customer_name : c.payment_code);

        IEnumerable<ShippingPaymentViewModel> listQuery;
        if (param.iSortCol_0 == 1)
        {
            if (param.sSortDir_0 == "asc")
            {
                listQuery = data.OrderBy(o => o.payment_date)
                                .Skip(param.iDisplayStart)
                                .Take(param.iDisplayLength);
            }
            else
            {
                listQuery = data.OrderByDescending(o => o.payment_date)
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

    public async Task<IActionResult> Clearing()
    {
        var query = await approveRepo.GetAdvanceJobList();

        ViewBag.List = query;
        return PartialView();
    }

    public async Task<IActionResult> ClearingData(TblShippingpayment model, string[] code)
    {
        model.ReceiveDate = DateOnly.FromDateTime(DateTime.Now);
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = this.LoggedInUser;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;
        
        ResponseViewModel result = await approveRepo.Clearing(model, code);
        return Json(result);
    }

    public async Task<IActionResult> EditClearing(string code)
    {
        var query = await approveRepo.GetAdvanceJobList();
        var selected = await approveRepo.GetClearingDataListByCode(code);
        var model = await approveRepo.GetClearingDataByCode(code);

        ViewBag.Selected = selected;
        ViewBag.List = query;

        return PartialView(model);
    }

    public async Task<IActionResult> EditClearingData(TblShippingpayment model, string[] code)
    {
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;

        ResponseViewModel result = await approveRepo.EditClearing(model, code);
        return Json(result);
    }

    public async Task<IActionResult> RemoveClearing(string code)
    {
        ResponseViewModel result = await approveRepo.RemoveClearing(code, this.LoggedInUser);
        return Json(result);
    }
}