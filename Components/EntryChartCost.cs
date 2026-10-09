using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace TMSWeb.Components;
public class EntryChartCost : ViewComponent
{
    private readonly DashboardRepository dashboardRepo;

    public EntryChartCost(DashboardRepository _dashboardRepo)
    {
        dashboardRepo = _dashboardRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        string month = DateTime.Now.Month.ToString();
        string year = DateTime.Now.Year.ToString();
        var data = await dashboardRepo.GetJobCostCustomerBySupplier(month, year);
        return View(data);
    }
}