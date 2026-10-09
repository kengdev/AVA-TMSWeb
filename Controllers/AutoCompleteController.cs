using Microsoft.AspNetCore.Mvc;
using TMSWeb.Models.ViewModels;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;
[Authorize]
public class AutoCompleteController : Controller
{
    private readonly AutoCompleteRepository autoComRepo;

    public AutoCompleteController(AutoCompleteRepository _autoComRepo)
    {
        autoComRepo = _autoComRepo;
    }

    public async Task<IActionResult> Truck(string term)
    {
        var query = (from a in await autoComRepo.GetTruck(term)
                     select new DefaultAutoCompleteViewModel
                     {
                         value = a.truck_code,
                         name = a.truck_license,
                         title = a.truck_license + " [" + a.truck_no + "]"
                     }).ToList();

        return Json(query);
    }

    public async Task<IActionResult> Trailer(string term)
    {
        var query = (from a in await autoComRepo.GetTrailer(term)
                     select new DefaultAutoCompleteViewModel
                     {
                         value = a.TrailerCode,
                         name = a.TrailerLicense,
                         title = a.TrailerLicense + " [" + a.TrailerNo + "]"
                     }).ToList();

        return Json(query);
    }

    public async Task<IActionResult> Zone(string term)
    {
        var query = (from a in await autoComRepo.GetZone(term)
                     select new DefaultAutoCompleteViewModel
                     {
                         code = a.ZoneCode,
                         value = a.ZoneCode,
                         title = a.ZoneName
                     }).ToList();

        return Json(query);
    }

    public async Task<IActionResult> Driver(string term)
    {
        var query = (from a in await autoComRepo.GetDriver(term)
                     select new DefaultAutoCompleteViewModel
                     {
                         value = a.DriverCode,
                         title = a.DriverCode + " [" + a.DriverFullname + "]",
                         name = a.DriverFullname
                     }).ToList();

        return Json(query);
    }

    public async Task<IActionResult> Place(string term)
    {
        var query = (from a in await autoComRepo.GetPlace(term)
                     select new DefaultAutoCompleteViewModel
                     {
                         value = a.PlaceCode,
                         title = a.PlaceCode + " [" + a.PlaceName + "]",
                         name = a.PlaceName
                     }).ToList();

        return Json(query);
    }

    public async Task<IActionResult> Customer(string term)
    {
        var query = (from a in await autoComRepo.GetCustomer(term)
                     select new DefaultAutoCompleteViewModel
                     {
                         code = a.customer_code,
                         value = a.customer_code,
                         title = a.customer_code + " [" + a.customer_name_th + "]",
                         name = a.customer_name_th
                     }).ToList();

        return Json(query);
    }

    /*public IActionResult RouteByCustomer(string customer, string term)
    {
        var auto = new AutoCompleteRepository(_context);
        var query = (from a in auto.GetRouteByCustomer(customer, term)
                     select new DefaultAutoCompleteViewModel
                     {
                         code = a.RouteCode,
                         value = a.RouteCode,
                         title = a.RouteName
                     }).ToList();

        return Json(query);
    }*/

    public async Task<IActionResult> TransportRate(string term)
    {
        var query = (from a in await autoComRepo.GetCustomerCost(term)
                     select new DefaultAutoCompleteViewModel
                     {
                         value = a.cost_code,
                         title = a.cost_code + " [" + a.cost_name + "]",
                         name = a.customer_name_th
                     }).ToList();

        return Json(query);
    }

    public async Task<IActionResult> Booking(string term)
    {
        var query = (from a in await autoComRepo.GetBooking(term)
                     select new DefaultAutoCompleteViewModel
                     {
                         value = a,
                         title = a
                     }).ToList();

        return Json(query);
    }

    /*public IActionResult Spareparts(string term)
    {
        var auto = new AutoCompleteRepository(_context);
        var query = (from a in auto.GetSpareparts(term)
                     select new SparepartsAutoCompleteViewModel
                     {
                         value = a.SparepartCode,
                         title = " [" + a.SparepartCode + "]" + a.SparepartName,
                         name = a.SparepartName,
                         quantity = a.Id != null ? a.Quantity.Value : 0,
                         price = a.Price != null ? a.Price.Value : 0,
                         type = a.SparepartType
                     }).OrderBy(o => o.name).ToList();

        return Json(query);
    }*/

    public async Task<IActionResult> PlaceByType(string term, string jobtype, string action)
    {
        string type = "";
        if (action == "S")
        {
            type = jobtype == "IMPORT" ? "PORT" : "YARD";
        }
        else if (action == "F")
        {
            type = jobtype == "IMPORT" ? "YARD" : "PORT";
        }

        var query = (from a in await autoComRepo.GetPlaceByType(term, type)
                     select new DefaultAutoCompleteViewModel
                     {
                         value = a.PlaceCode,
                         title = a.PlaceCode + " [" + a.PlaceName + "]",
                         name = a.PlaceName
                     }).ToList();

        return Json(query);
    }

    public async Task<IActionResult> Supplier(string term)
    {
        var query = (from a in await autoComRepo.GetSupplier(term)
                     where a.IsEnabled == true
                     select new DefaultAutoCompleteViewModel
                     {
                         value = a.SupplierCode,
                         name = a.SupplierName,
                         title = a.SupplierFullname
                     }).ToList();

        return Json(query);
    }
}