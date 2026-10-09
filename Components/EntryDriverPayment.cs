using Microsoft.AspNetCore.Mvc;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;

namespace TMSWeb.Components;

public class EntryDriverPayment : ViewComponent
{
    public IViewComponentResult Invoke(ViewDriverPaymentListActive data)
    {
        var hasJobDetail =
            !string.IsNullOrWhiteSpace(data.JobDetailCode);

        var model = new TblAllowanceDetail
        {
            AllowanceCode = data.JobCode,

            JobDetailCode = hasJobDetail
                ? data.JobDetailCode
                : "-",

            AllowanceDate =
                DateOnly.FromDateTime(data.JobDate),

            TripValue = hasJobDetail
                ? data.TripValue
                : 0,

            FuelValue =
                hasJobDetail &&
                data.TruckTypegroupCode == "02"
                    ? data.FuelBalance
                    : 0,

            WasteValue = hasJobDetail
                ? data.WasteValue
                : 0,

            OtherValue = hasJobDetail
                ? data.OtherValue
                : 0,

            TripType = hasJobDetail
                ? data.CostType
                : "A"
        };

        ViewBag.MissionName =
            data.MissionName ?? "-";

        ViewBag.ContainerNo =
            data.ContainerNo ?? "-";

        ViewBag.Code = hasJobDetail
            ? data.HashCode
            : data.JobDate.ToString("ddMMyyyy");

        return View(model);
    }
}