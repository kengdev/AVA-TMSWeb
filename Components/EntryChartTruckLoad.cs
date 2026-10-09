using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using TMSWeb.Repositories;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.Entities.HR;

namespace TMSWeb.Components;
public class EntryChartTruckLoad : ViewComponent
{
    private readonly DashboardRepository dashboardRepo;

    public EntryChartTruckLoad(DashboardRepository _dashboardRepo)
    {
        dashboardRepo = _dashboardRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var dayofweek = (int)DateTime.Now.DayOfWeek;
        var last_sat = this.GetLastThurs(dayofweek);
        DateTime start_date = DateTime.Now.AddDays(last_sat).Date;

        var data = await dashboardRepo.GetTruckWorkingData(start_date);
        return View(data);
    }

    private int GetLastSaturn(int val)
    {
        int load = 0;
        switch (val)
        {
            case 0:
                load = -1;
                break;
            case 1:
                load = -2;
                break;
            case 2:
                load = -3;
                break;
            case 3:
                load = -4;
                break;
            case 4:
                load = -5;
                break;
            case 5:
                load = -6;
                break;
        }

        return load;
    }

    private int GetLastThurs(int val)
    {
        int load = 0;
        switch (val)
        {
            case 0:
                load = -3;
                break;
            case 1:
                load = -4;
                break;
            case 2:
                load = -5;
                break;
            case 3:
                load = -6;
                break;
            case 4:
                load = -7;
                break;
            case 5:
                load = -8;
                break;
        }

        return load;
    }
}