using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Repositories;
using TMSWeb.Models.Entities.LOG;
using Microsoft.EntityFrameworkCore;

namespace AVAWeb.Components;

public class EntryMonthlyPayment : ViewComponent
{
    private readonly DbavalogContext db;

    public EntryMonthlyPayment(DbavalogContext context)
    {
        db = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(string action_type, int id)
    {
        var model = new TblMonthlypaymentDetail();
        if (action_type == "edit")
        {
            model = await db.TblMonthlypaymentDetails.Where(w => w.Id == id).FirstOrDefaultAsync();
        }

        ViewBag.Type = action_type;
        return View(model);
    }
}