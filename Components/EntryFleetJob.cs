using Microsoft.AspNetCore.Mvc;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Components;

public class EntryFleetJob : ViewComponent
{
    public IViewComponentResult Invoke(List<JobViewModel> model)
    {
        return View(model);
    }
}