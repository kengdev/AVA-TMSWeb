using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;

[Authorize(Roles = "Administrator")]
public class QuotationController : Controller
{
    private readonly QuotationRepository quotationRepo;
    private readonly CodeRepository codeRepo;
    private string LoggedInUser => User.Identity.Name;

    public QuotationController(QuotationRepository _quotationRepo, CodeRepository _codeRepo)
    {
        quotationRepo = _quotationRepo;
        codeRepo = _codeRepo;
    }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Add()
    {
        return View();
    }

    public async Task<IActionResult> AddData(TblQuotation model, string[] condition, string[] token, string[] rate, decimal[] cost1, decimal[] cost2, decimal[] cost3, string rdoActive)
    {
        model.QuotationCode = await codeRepo.GetQuotationCode();
        model.ContainerSize1 = model.ContainerSize1 != null ? model.ContainerSize1 : "-";
        model.ContainerSize2 = model.ContainerSize2 != null ? model.ContainerSize2 : "-";
        model.ContainerSize3 = model.ContainerSize3 != null ? model.ContainerSize3 : "-";
        model.Remark = model.Remark == null ? "-" : model.Remark;
        model.IsActived = true;
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = LoggedInUser;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = LoggedInUser;

        for (int i = 0; i < rate.Count(); i++)
        {
            TblQuotationDetail detail = new TblQuotationDetail();
            detail.FuelRate = rate[i];
            detail.Token = token[i];
            detail.CostValue1 = model.ContainerSize1 != "-" ? cost1[i] : 0;
            detail.CostValue2 = model.ContainerSize2 != "-" ? cost2[i] : 0;
            detail.CostValue3 = model.ContainerSize3 != "-" ? cost3[i] : 0;
            detail.IsActive = rdoActive == detail.Token ? true : false;
            detail.QuotationCode = model.QuotationCode;
            model.TblQuotationDetails.Add(detail);
        }

        for (int i = 0; i < condition.Count(); i++)
        {
            if (condition[i] != null && condition[i] != "")
            {
                TblQuotationCondition conditionlist = new TblQuotationCondition();
                conditionlist.Condition = condition[i];
                conditionlist.QuotationCode = model.QuotationCode;
                model.TblQuotationConditions.Add(conditionlist);
            }
        }

        ResponseViewModel result = await quotationRepo.Add(model);
        return Json(result);
    }

    public async Task<IActionResult> Edit(string code)
    {
        var model = await quotationRepo.GetQuotationDataByCode(code);

        return View(model);
    }

    public async Task<IActionResult> Remove(string code)
    {
        var model = await quotationRepo.Remove(code, this.LoggedInUser);

        return Json(model);
    }

    public async Task<IActionResult> Active(string code)
    {
        var model = await quotationRepo.Active(code, this.LoggedInUser);

        return Json(model);
    }

    public async Task<IActionResult> Inactive(string code)
    {
        var model = await quotationRepo.Inactive(code, this.LoggedInUser);

        return Json(model);
    }

    public async Task<IActionResult> EditData(TblQuotation model, string[] condition, string[] token, string[] rate, decimal[] cost1, decimal[] cost2, decimal[] cost3, string rdoActive)
    {
        model.Remark = model.Remark == null ? "-" : model.Remark;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = LoggedInUser;

        for (int i = 0; i < rate.Count(); i++)
        {
            TblQuotationDetail detail = new TblQuotationDetail();
            detail.FuelRate = rate[i];
            detail.Token = token[i];
            detail.CostValue1 = model.ContainerSize1 != "-" ? cost1[i] : 0;
            detail.CostValue2 = model.ContainerSize2 != "-" ? cost2[i] : 0;
            detail.CostValue3 = model.ContainerSize3 != "-" ? cost3[i] : 0;
            detail.IsActive = rdoActive == detail.Token ? true : false;
            detail.QuotationCode = model.QuotationCode;
            model.TblQuotationDetails.Add(detail);
        }

        for (int i = 0; i < condition.Count(); i++)
        {
            if (condition[i] != null && condition[i] != "")
            {
                TblQuotationCondition conditionlist = new TblQuotationCondition();
                conditionlist.Condition = condition[i];
                conditionlist.QuotationCode = model.QuotationCode;
                model.TblQuotationConditions.Add(conditionlist);
            }
        }

        ResponseViewModel result = await quotationRepo.Edit(model);
        return Json(result);
    }

    public async Task<IActionResult> GetQuotationDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await quotationRepo.GetQuotationList();
        var data = query.Where(w =>
                (iFilter.filterNameTH == null || w.customer.ToUpper().Contains(iFilter.filterNameTH.ToUpper())) &&
                (iFilter.filterNameEN == null || w.route.ToUpper().Contains(iFilter.filterNameEN.ToUpper())));

        Func<QuotationViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.customer : c.customer);

        IEnumerable<QuotationViewModel> listQuery;
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
}