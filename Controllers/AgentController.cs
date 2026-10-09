using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using TMSWeb.Services;

namespace TMSWeb.Controllers;

[Authorize]
public class AgentController : Controller
{
    private readonly AgentRepository _agentRepo;
    private readonly IAuditLogService _auditLogService;
    private string LoggedInUser = "admin";

    public AgentController(AgentRepository agentRepo)
    {
        _agentRepo = agentRepo;
    }
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> GetAgentDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await _agentRepo.GetAgentDataList();
        var data = query.Where(w => (iFilter.filterNameTH == null || w.name.ToUpper().Contains(iFilter.filterNameTH.ToUpper())));

        Func<AgentViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.name : c.name);

        IEnumerable<AgentViewModel> listQuery;
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

    public IActionResult Add()
    {
        return View();
    }

    public async Task<IActionResult> AddData(TblAgent model)
    {
        model.IsActived = true;
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = this.LoggedInUser;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;

        ResponseViewModel result = await _agentRepo.AddData(model);
        return Json(result);
    }

    public async Task<IActionResult> Edit(string code)
    {
        var model = await _agentRepo.GetAgentDataByCode(code);
        return View(model);
    }

    public async Task<IActionResult> EditData(TblAgent dataToEdit)
    {
        var username = LoggedInUser;
        dataToEdit.UpdateDate = DateTime.Now;
        dataToEdit.UpdateBy = username;

        ResponseViewModel result = await _agentRepo.EditAgentData(dataToEdit);
        return Json(result);
    }

    public async Task<IActionResult> Remove(string code)
    {
        var username = LoggedInUser;

        ResponseViewModel result = await _agentRepo.Remove(code, LoggedInUser);
        return Json(result);
    }

    public async Task<IActionResult> Active(string code)
    {
        var username = LoggedInUser;

        ResponseViewModel result = await _agentRepo.Active(code, LoggedInUser);
        return Json(result);
    }

    public async Task<IActionResult> Inactive(string code)
    {
        var username = LoggedInUser;

        ResponseViewModel result = await _agentRepo.Inactive(code, LoggedInUser);
        return Json(result);
    }
}