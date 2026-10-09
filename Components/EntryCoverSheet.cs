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
public class EntryCoverSheet : ViewComponent
{
    private readonly CoverSheetRepository coversheetRepo;
    private string LoggedInUser => User.Identity.Name;

    public EntryCoverSheet(CoverSheetRepository _coversheetRepo)
    {
        coversheetRepo = _coversheetRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(string code)
    {
        var model = new TblCoversheetDetail();
        var data = await coversheetRepo.GetBillingBycode(code);

        model.CoversheetValue = data.billing_total;
        model.BillingCode = data.billing_code;
        ViewBag.Data = data;
        return View(model);
    }
}