using Microsoft.AspNetCore.Mvc;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace AVAWeb.Controllers;
[Authorize(Roles = "Administrator, Transport, Super")]
public class ZoneController : Controller
{
    private readonly ZoneRepository _zoneRepo;
    private string LoggedInUser => User.Identity.Name;

    public ZoneController(ZoneRepository zoneRepo)
    {
        _zoneRepo = zoneRepo;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Add()
    {
        var rate = await _zoneRepo.GetFuelRate();
        ViewBag.Rate = rate;
        return View();
    }

    public async Task<IActionResult> Edit(string code)
    {
        var data = await _zoneRepo.GetZoneByCode(code);
        return View(data);
    }

    public async Task<IActionResult> GetZoneDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await _zoneRepo.GetZoneDataList();
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

    public async Task<IActionResult> EditFuelRate()
    {
        var zoneData = await _zoneRepo.GetZoneDataList();
        ViewBag.Data = zoneData.OrderBy(o => o.zone_name).ToList();
        return View();
    }

    public async Task<IActionResult> EditRate(string[] zone, int[] tr4, int[] tr6, int[] tr10)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await _zoneRepo.EditRate(zone, tr4, tr6, tr10, username);
        return Json(result);
    }

    public async Task<IActionResult> EditZoneData(TblZone zoneToEdit)
    {
        string username = LoggedInUser;
        zoneToEdit.UpdateDate = DateTime.Now;
        zoneToEdit.UpdateBy = username;

        ResponseViewModel result = await _zoneRepo.EditZoneData(zoneToEdit);
        return Json(result);
    }

    public async Task<IActionResult> AddZoneData(TblZone zoneToAdd, int tr4, int tr6, int tr10)
    {
        string username = LoggedInUser;

        zoneToAdd.IsActived = true;
        zoneToAdd.IsEnabled = true;
        zoneToAdd.CreateDate = DateTime.Now;
        zoneToAdd.CreateBy = username;
        zoneToAdd.UpdateDate = DateTime.Now;
        zoneToAdd.UpdateBy = username;

        ResponseViewModel result = await _zoneRepo.AddZoneData(zoneToAdd, tr4, tr6, tr10);
        return Json(result);
    }

    public async Task<IActionResult> Remove(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await _zoneRepo.Remove(code, username);
        return Json(result);
    }

    public async Task<IActionResult> Active(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await _zoneRepo.Active(code, username);
        return Json(result);
    }

    public async Task<IActionResult> Inactive(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await _zoneRepo.InActive(code, username);
        return Json(result);
    }
}