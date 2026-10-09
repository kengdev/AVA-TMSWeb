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
public class EntrySupplierDetail : ViewComponent
{
    private readonly PaymentRepository paymentRepo;
    private string LoggedInUser => User.Identity.Name;
    public EntrySupplierDetail(PaymentRepository _paymentRepo)
    {
        paymentRepo = _paymentRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(string trip)
    {
        var model = new TblSupplierpaymentDetail();
        var data = await paymentRepo.GetJobByCode(trip);

        model.JobDetailCode = trip;
        model.SupplierpaymentValue = data.cost_value.Value;
        ViewBag.Data = data;
        return View(model);
    }
}