using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Repositories;
using TMSWeb.Models.Entities.LOG;

namespace TMSWeb.Components;

public class EntryFloorJob : ViewComponent
{
    private readonly DropListRepository dropListRepo;

    public EntryFloorJob(DropListRepository _dropListRepo)
    {
        dropListRepo = _dropListRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var truckGroups = await dropListRepo.GetTruckGroup();
        ViewData["TruckTypegroupCode"] = from a in truckGroups select new SelectListItem { Value = a.key.ToString(), Text = a.label };

        return View();
    }
}