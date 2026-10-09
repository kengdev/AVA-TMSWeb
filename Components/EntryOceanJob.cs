using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;

namespace TMSWeb.Components;

public class EntryOceanJob : ViewComponent
{
    private readonly DropListRepository dropListRepo;

    public EntryOceanJob(DropListRepository _dropListRepo)
    {
        dropListRepo = _dropListRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {

        var consignee = await dropListRepo.GetConsignee();
        var shipper = await dropListRepo.GetShipper();
        var requestBy = await dropListRepo.GetRequestBy();
        var shippingContact = await dropListRepo.GetShippingContact();
        var containerSize = await dropListRepo.GetContainerSize();
        var agent = await dropListRepo.GetAgent();
        var truckGroup = await dropListRepo.GetTruckGroup();

        ViewData["consignee_name"] = consignee
            .Select(a => new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label
            })
            .ToList();

        ViewData["shipper_name"] = shipper
            .Select(a => new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label
            })
            .ToList();

        ViewData["request_name"] = requestBy
            .Select(a => new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label
            })
            .ToList();

        ViewData["shipping_name"] = shippingContact
            .Select(a => new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label
            })
            .ToList();

        ViewData["ContainerSizeCode"] = containerSize
            .Select(a => new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label
            })
            .ToList();

        ViewData["AgentCode"] = agent
            .Select(a => new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label
            })
            .ToList();

        ViewData["TruckTypegroupCode"] = truckGroup
            .Select(a => new SelectListItem
            {
                Value = a.key.ToString(),
                Text = a.label
            })
            .ToList();

        return View();
    }
}