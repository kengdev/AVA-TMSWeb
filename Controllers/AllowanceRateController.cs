using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;
[Authorize(Roles = "Administrator")]
public class AllowanceRateController : Controller
{
    private readonly AllowanceRateRepository rateRepo;
    private readonly DropListRepository dropListRepo;
    private string LoggedInUser => User.Identity.Name;

    public AllowanceRateController(AllowanceRateRepository _rateRepo, DropListRepository _dropListRepo)
    {
        this.rateRepo = _rateRepo;
        this.dropListRepo = _dropListRepo;
    }

    public IActionResult DriverRate()
    {
        return View();
    }

    public async Task<IActionResult> GetDriverAllowanceDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await rateRepo.GetDriverRate();
        var data = query.Where(w => (iFilter.filterMission == null || w.name.ToUpper().Contains(iFilter.filterMission.ToUpper())) &&
        (iFilter.filterZone == null || w.zone_name.ToUpper().Contains(iFilter.filterZone.ToUpper())));

        Func<DriverRateViewModel, string> orderingFunction = (c => param.iSortCol_0 == 0 ? c.name :
        param.iSortCol_0 == 1 ? c.zone_name : c.name);

        IEnumerable<DriverRateViewModel> listQuery;
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

    public async Task<IActionResult> EditDriverRate(decimal value, string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await rateRepo.EditDriverRate(value, code, username);
        return Json(result);
    }

    public async Task<IActionResult> SupplyerRate()
    {
        ViewData["supplyer_code"] = from a in await dropListRepo.GetSupplierTransport() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        return View();
    }

    public async Task<IActionResult> GetSupplierRateDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var data = await rateRepo.GetSupplyerRate(iFilter.filterSupplier);
        data = data.Where(w => (iFilter.filterMission == null || w.place_name.ToLower().Contains(iFilter.filterMission.ToLower())) &&
                                    (iFilter.filterZone == null || w.zone_name.ToLower().Contains(iFilter.filterZone.ToLower()))).ToList();

        Func<SupplyerRateViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.place_name :
        param.iSortCol_0 == 2 ? c.zone_name : c.place_name);

        IEnumerable<SupplyerRateViewModel> listQuery;
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

    public async Task<IActionResult> EditSupplierRate(decimal value, string supplier_code, string place_code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await rateRepo.EditSupplyerRate(value, supplier_code, place_code, username);
        return Json(result);
    }

    public async Task<IActionResult> FleetRate()
    {
        ViewData["supplyer_code"] = from a in await dropListRepo.GetFleet() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        return View();
    }

    public async Task<IActionResult> GetFleetRateDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var data = await rateRepo.GetFleetRate(iFilter.filterSupplier);
        data = data.Where(w => (iFilter.filterMission == null || w.place_name.ToLower().Contains(iFilter.filterMission.ToLower())) &&
                                    (iFilter.filterZone == null || w.zone_name.ToLower().Contains(iFilter.filterZone.ToLower()))).ToList();

        Func<SupplyerRateViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.place_name :
        param.iSortCol_0 == 2 ? c.zone_name : c.supplyer_name);

        IEnumerable<SupplyerRateViewModel> listQuery;
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

    public async Task<IActionResult> EditFleetRate(decimal value, string supplier_code, string place_code, string type)
    {
        string username = LoggedInUser;
        ResponseViewModel result = await rateRepo.EditFleetRate(value, supplier_code, place_code, type, username);
        return Json(result);
    }
}