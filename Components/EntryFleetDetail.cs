using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.Entities.HR;

namespace TMSWeb.Components;

[Authorize]
public class EntryFleetDetail : ViewComponent
{
    private readonly PaymentRepository paymentRepo;
    private string LoggedInUser => User.Identity.Name;
    public EntryFleetDetail(PaymentRepository _paymentRepo)
    {
        paymentRepo = _paymentRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(string trip)
    {
        var model = new TblFleetpaymentDetail();
        var data = await paymentRepo.GetJobByCode(trip);

        model.JobDetailCode = trip;
        model.TransportValue = data.cost_value.Value;
        ViewBag.Data = data;
        return View(model);
    }
}