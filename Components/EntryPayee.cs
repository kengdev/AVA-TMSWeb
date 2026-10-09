using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Repositories;
using TMSWeb.Models.Entities.LOG;
using Microsoft.EntityFrameworkCore;

namespace TMSWeb.Components;

public class EntryPayee : ViewComponent
{
    private readonly DbavalogContext db;

    public EntryPayee(DbavalogContext context)
    {
        db = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(string action_type, int id)
    {
        var model = new TblReceivePaymentDetail();
        if (action_type == "edit")
        {
            model = await db.TblReceivePaymentDetails.Where(w => w.Id == id).FirstOrDefaultAsync();
        }

        ViewBag.Type = action_type;
        return View(model);
    }
}