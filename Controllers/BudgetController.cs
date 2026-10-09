using Microsoft.AspNetCore.Mvc;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;
[Authorize(Roles = "Administrator, Accounting, Super")]
public class BudgetController : Controller
{
    private readonly BudgetRepository budgetRepo;
    private string LoggedInUser => User.Identity.Name;

    public BudgetController(BudgetRepository _budgetRepo)
    {
        budgetRepo = _budgetRepo;
    }

    #region Transport Budget
    public async Task<IActionResult> Index()
    {
        var model = await budgetRepo.GetTrBudgetData();

        return View(model);
    }

    public IActionResult _AddBalance()
    {
        return PartialView();
    }

    public IActionResult _AddPayment()
    {
        return PartialView();
    }

    public async Task<IActionResult> GetTransportTransactionDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await budgetRepo.GetTransportBudgetTransaction(iFilter.filterDateRangeStart, iFilter.filterDateRangeEnd);
        var data = query.Where(w => (iFilter.filterCustomer == null || w.customer_name.ToUpper().Contains(iFilter.filterCustomer.ToUpper())) &&
        (iFilter.filterMission == null || w.mission.ToUpper().Contains(iFilter.filterMission.ToUpper())) &&
        (iFilter.filterLicense == null || w.transaction_type.Contains(iFilter.filterLicense)));

        Func<TransportBudgetTransactionViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.transaction_date : c.transaction_date);

        IEnumerable<TransportBudgetTransactionViewModel> listQuery;
        if (param.iSortCol_0 == 2)
        {
            if (param.sSortDir_0 == "asc")
            {
                listQuery = data.OrderBy(o => o.transaction_date)
                                .Skip(param.iDisplayStart)
                                .Take(param.iDisplayLength);
            }
            else
            {
                listQuery = data.OrderByDescending(o => o.transaction_date)
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

    public async Task<IActionResult> TransactionDetail(string code)
    {
        var list = await budgetRepo.GetBudgetTransactionDetail(code);
        var data = await budgetRepo.GetBudgetTransactionDataByCode(code);

        ViewBag.List = list;
        return View(data);
    }

    public async Task<IActionResult> SetTrBalance()
    {
        var data = await budgetRepo.GetTrBudget();

        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> SetBalanceData(decimal amount, DateTime budget_date, string remark)
    {
        var _notif = new LineNotifRepository();

        ResponseViewModel response = await budgetRepo.UpdateBalance(amount, budget_date, remark, this.LoggedInUser);
        if (response.result)
            await _notif.NotifGroup(response.notifMessage);

        return Json(response);
    }

    [HttpPost]
    public async Task<IActionResult> AddPaymentData(decimal amount, DateTime budget_date, string remark)
    {
        var _notif = new LineNotifRepository();

        ResponseViewModel response = await budgetRepo.AddPayment(amount, budget_date, remark, this.LoggedInUser);
        if (response.result)
            await _notif.NotifGroup(response.notifMessage);

        return Json(response);
    }
    #endregion

    #region General Budget
    public async Task<IActionResult> SetGeBalance()
    {
        var data = await budgetRepo.GetGeBudget();

        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> SetGeneralBalanceData(decimal amount, DateTime budget_date, string remark)
    {
        var _notif = new LineNotifRepository();

        ResponseViewModel response = await budgetRepo.UpdateGeneralBalance(amount, budget_date, remark, this.LoggedInUser);
        if (response.result)
            await _notif.NotifGroup(response.notifMessage);

        return Json(response);
    }

    [HttpPost]
    public async Task<IActionResult> AddGeneralPaymentData(decimal amount, DateTime budget_date, string remark)
    {
        var _notif = new LineNotifRepository();

        ResponseViewModel response = await budgetRepo.AddGeneralPayment(amount, budget_date, remark, this.LoggedInUser);
        if (response.result)
            await _notif.NotifGroup(response.notifMessage);

        return Json(response);
    }
    #endregion
}