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
public class EntryReceipt : ViewComponent
{
    private string LoggedInUser => User.Identity.Name;
    private readonly ReceiptRepository receiptRepo;

    public EntryReceipt(ReceiptRepository _receiptRepo)
    {
        receiptRepo = _receiptRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(string code)
    {
        var model = new TblReceiptDetail();
        var data = await receiptRepo.GetBillingBycode(code);

        model.ReceiptValue = data.billing_total;
        model.ReceiptAdvanceValue = data.advance_value;
        model.ReceiptTransportValue = data.transport_value;
        model.BillingCode = data.billing_code;
        ViewBag.Data = data;
        return View(model);
    }
}