using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;

namespace TMSWeb.Components;

[Authorize]
public class EntryUpdateCashing : ViewComponent
{
    private string LoggedInUser => User.Identity.Name;
    private readonly JobRepository jobRepo;

    public EntryUpdateCashing(JobRepository _jobRepo)
    {
        jobRepo = _jobRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(string code)
    {
        var cashing = await jobRepo.GetCashingByJob(code);
        if (cashing != null)
        {
            var cashing_other = await jobRepo.GetCashingOtherByJob(cashing.CashingCode);
            cashing.CashingFuelup = cashing.CashingFuelup;
            cashing.CashingExpresswayup = cashing.CashingExpresswayup == 0 ? cashing.CashingExpressway : cashing.CashingExpresswayup;

            ViewBag.Other = cashing_other;
            return View(cashing);
        }
        else
        {
            return View(cashing);
        }
    }
}