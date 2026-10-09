using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;
[Authorize(Roles = "Administrator")]
public class FuelRateController : Controller
{
    private readonly FuelRateRepository rateRepo;
    private readonly DropListRepository dropListRepo;
    private readonly ZoneRepository zoneRepo;
    private string LoggedInUser => User.Identity.Name;

    public FuelRateController(FuelRateRepository _rateRepo, DropListRepository _dropListRepo, ZoneRepository _zoneRepo)
    {
        rateRepo = _rateRepo;
        dropListRepo = _dropListRepo;
        zoneRepo = _zoneRepo;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["supplyer_code"] = from a in await dropListRepo.GetFleet() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        return View();
    }

    public async Task<IActionResult> GetFuelRateDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await zoneRepo.GetZoneDataList();
        var data = query.Where(w => (iFilter.filterNameTH == null || w.zone_name.ToUpper().Contains(iFilter.filterNameTH.ToUpper())));

        Func<ZoneViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.zone_name : c.zone_name);

        IEnumerable<ZoneViewModel> listQuery;
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

    public async Task<IActionResult> EditFuelRate(string zone_code, string truck_type, int value)
    {
        ResponseViewModel result = await rateRepo.EditFuelRate(zone_code, truck_type, value, this.LoggedInUser);

        return Json(result);
    }
}