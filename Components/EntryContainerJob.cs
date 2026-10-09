using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Repositories;
using TMSWeb.Models.Entities.LOG;

namespace TMSWeb.Components;

public class EntryContainerJob : ViewComponent
{
    private readonly DropListRepository dropListRepo;

    public EntryContainerJob(DropListRepository _dropListRepo)
    {
        dropListRepo = _dropListRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var containerSizes = await dropListRepo.GetContainerSize();
        var agents = await dropListRepo.GetAgent();
        var truckGroups = await dropListRepo.GetTruckGroup();

        ViewData["ContainerSizeCode"] = containerSizes
            .Select(a => new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label
            })
            .ToList();

        ViewData["AgentCode"] = agents
            .Select(a => new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label
            })
            .ToList();

        ViewData["TruckTypegroupCode"] = truckGroups
            .Select(a => new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label
            })
            .ToList();

        return View();
    }
}