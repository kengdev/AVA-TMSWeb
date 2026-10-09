using Microsoft.AspNetCore.Mvc;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;

[Authorize(Roles = "Administrator, Transport, Super")]
public class YardPortController : Controller
{
    private readonly YardPortRepository yardportRepo;
    private string LoggedInUser => User.Identity.Name;

    public YardPortController(YardPortRepository _yardportRepo)
    {
        _yardportRepo = yardportRepo;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> GetYardPortDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await yardportRepo.GetYardPortDataList();
        var data = query.Where(w => (iFilter.filterNameTH == null || w.yardport_name.ToUpper().Contains(iFilter.filterNameTH.ToUpper())) &&
        (iFilter.filterNameEN == null || w.yardport_type.ToUpper().Contains(iFilter.filterNameEN.ToUpper())));

        Func<YardPortViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.yardport_type :
        param.iSortCol_0 == 3 ? c.yardport_type : c.yardport_type);

        IEnumerable<YardPortViewModel> listQuery;
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

    public async Task<IActionResult> AddPort()
    {
        var container = await yardportRepo.GetContainerData();

        ViewBag.Container = container;
        return View();
    }

    public async Task<IActionResult> AddPortData(PortModel portToAdd)
    {
        string username = LoggedInUser;

        portToAdd.place_remark = portToAdd.place_remark == null ? "-" : portToAdd.place_remark;
        portToAdd.place_type = "PORT";
        portToAdd.create_by = username;

        ResponseViewModel result = await yardportRepo.AddPortData(portToAdd);
        return Json(result);
    }

    public async Task<IActionResult> EditPort(string code)
    {
        var data = await yardportRepo.GetPortData(code);
        var cashing = await yardportRepo.GetListPortCashing(code);

        ViewBag.Cashing = cashing;
        return View(data);
    }

    public async Task<IActionResult> EditPortData(PortModel portToEdit)
    {
        string username = LoggedInUser;
        portToEdit.update_by = username;

        ResponseViewModel result = await yardportRepo.EditPortData(portToEdit);
        return Json(result);
    }

    public IActionResult AddYard()
    {
        return View();
    }

    public async Task<IActionResult> AddYardData(YardModel yardToAdd)
    {
        string username = LoggedInUser;

        yardToAdd.place_remark = yardToAdd.place_remark == null ? "-" : yardToAdd.place_remark;
        yardToAdd.place_type = "YARD";
        yardToAdd.create_by = username;

        ResponseViewModel result = await yardportRepo.AddYardData(yardToAdd);
        return Json(result);
    }

    public async Task<IActionResult> EditYard(string code)
    {
        var data = await yardportRepo.GetYardData(code);
        var cashing = await yardportRepo.GetYardCashingData(code);
        ViewBag.Cashing = cashing;

        return View(data);
    }

    public async Task<IActionResult> EditYardData(YardModel yardToEdit)
    {
        string username = LoggedInUser;

        yardToEdit.place_remark = yardToEdit.place_remark == null ? "-" : yardToEdit.place_remark;
        yardToEdit.update_by = username;

        ResponseViewModel result = await yardportRepo.EditYardData(yardToEdit);
        return Json(result);
    }

    public async Task<IActionResult> Remove(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await yardportRepo.Remove(code, username);
        return Json(result);
    }

    public async Task<IActionResult> Active(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await yardportRepo.Active(code, username);
        return Json(result);
    }

    public async Task<IActionResult> Inactive(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await yardportRepo.InActive(code, username);
        return Json(result);
    }
}