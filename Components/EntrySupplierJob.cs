using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Components;
public class EntrySupplierJob : ViewComponent
{
    public IViewComponentResult Invoke(List<JobViewModel> model)
    {
        return View(model);
    }
}