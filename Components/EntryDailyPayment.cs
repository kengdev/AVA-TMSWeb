using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Repositories;
using TMSWeb.Models.Entities.LOG;
using Microsoft.EntityFrameworkCore;

namespace TMSWeb.Components;

public class EntryDailyPayment : ViewComponent
{
    private readonly DbavalogContext db;

    public EntryDailyPayment(DbavalogContext context)
    {
        db = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(string action_type, int id)
    {
        var model = new TblDailypaymentDetail();
        if (action_type == "edit")
        {
            model = await db.TblDailypaymentDetails.Where(w => w.Id == id).FirstOrDefaultAsync();
        }

        ViewBag.Type = action_type;
        return View(model);
    }
}