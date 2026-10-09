using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Models;
using TMSWeb.Models.ViewModels;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;

namespace TMSWeb.Controllers;
[Authorize(Roles = "Administrator, Transport, Super")]
public class TrailerController : Controller
{
    private readonly TrailerRepository trailerRepo;
    private readonly DropListRepository dropListRepo;
    private string LoggedInUser => User.Identity.Name;

    public TrailerController(TrailerRepository _trailerRepo, DropListRepository _dropListRepo)
    {
        trailerRepo = _trailerRepo;
        dropListRepo = _dropListRepo;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Add()
    {
        ViewData["TrailerTypeCode"] = from a in await dropListRepo.GetTrailerType() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewData["TruckCode"] = from a in await dropListRepo.GetTruck() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewData["TrailerFloorCode"] = from a in await dropListRepo.GetTrailerFloor() select new SelectListItem { Value = a.key.ToString(), Text = a.label };

        return View();
    }

    public async Task<IActionResult> AddTrailer(TblTrailer trailerToAdd, string truck_code)
    {
        string username = LoggedInUser;

        trailerToAdd.IsEnabled = true;
        trailerToAdd.CreateDate = DateTime.Now;
        trailerToAdd.CreateBy = username;
        trailerToAdd.UpdateDate = DateTime.Now;
        trailerToAdd.UpdateBy = username;

        ResponseViewModel result = await trailerRepo.AddTrailer(trailerToAdd, truck_code);
        return Json(result);
    }

    public async Task<IActionResult> Edit(string code)
    {
        var data = await trailerRepo.GetTrailerDataByCode(code);
        var truck = await trailerRepo.GetTruckByTrailer(code);
        ViewData["TruckCode"] = from a in await dropListRepo.GetTruck() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == truck.TruckCode };
        ViewData["TrailerTypeCode"] = from a in await dropListRepo.GetTrailerType() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.TrailerTypeCode };
        ViewData["TrailerFloorCode"] = from a in await dropListRepo.GetTrailerFloor() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == data.TrailerFloorCode };

        return View(data);
    }

    public async Task<IActionResult> EditTrailer(TblTrailer trailerToEdit, string TruckCode)
    {
        string username = LoggedInUser;

        trailerToEdit.UpdateDate = DateTime.Now;
        trailerToEdit.UpdateBy = username;


        ResponseViewModel result = await trailerRepo.EditTrailer(trailerToEdit, TruckCode);
        return Json(result);
    }

    public async Task<IActionResult> _TrailerDetail(string license)
    {
        string username = LoggedInUser;

        var truck = await trailerRepo.GetTrailerByLicense(license);
        var insurance = await trailerRepo.GetInsuranceByLicense(license);

        ViewBag.Truck = truck;
        ViewBag.InsuranceList = insurance;
        return View();
    }

    public async Task<IActionResult> _EditTrailerInsurance(string trailer_license_insurance)
    {
        ViewData["insurance"] = from a in await dropListRepo.GetInsurance() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewData["insurance_type"] = from a in await dropListRepo.GetInsuranceType() select new SelectListItem { Value = a.key.ToString(), Text = a.label };

        var truck = await trailerRepo.GetTrailerByLicense(trailer_license_insurance);
        var insurance = await trailerRepo.GetInsuranceByLicense(trailer_license_insurance);

        ViewBag.Truck = truck;
        ViewBag.InsuranceList = insurance;
        return View();
    }

    public IActionResult _EntryEditInsurance(string insurance_code, string insurance_type_code, decimal? budget, decimal? increase, DateTime? started, DateTime? expired)
    {
        ViewTrailerInsuranceList data = new ViewTrailerInsuranceList();

        data.InsuranceCode = insurance_code;
        data.InsuranceTypeCode = insurance_type_code;
        data.InsuranceBudget = budget.Value;
        data.InsuranceIncrease = increase.Value;
        data.StartedDate = started.Value;
        data.ExpiredDate = expired.Value;

        return ViewComponent("EditInsuranceComponent", data);
    }

    public async Task<IActionResult> EditTrailerInsuranceData(TblTrailer trailerInsuranceToEdit)
    {
        string username = LoggedInUser;
        ResponseViewModel result = await trailerRepo.EditTrailerInsurance(trailerInsuranceToEdit, username);
        return Json(result);
    }

    public async Task<IActionResult> GetTrailerDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await trailerRepo.GetTruckDataList();
        var data = query.Where(w => (iFilter.filterLicense == null || w.truck_license.ToUpper().Contains(iFilter.filterLicense.ToUpper())) &&
                    (iFilter.filterTrailer == null || w.trailer_license.ToUpper().Contains(iFilter.filterTrailer.ToUpper())) &&
                    (iFilter.filterTruck == null || w.truck_license.ToUpper().Contains(iFilter.filterTruck.ToUpper())));

        Func<TrailerViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.trailer_no :
                                                                param.iSortCol_0 == 2 ? c.trailer_license :
                                                                param.iSortCol_0 == 3 ? c.truck_license :
                                                                param.iSortCol_0 == 4 ? c.trailer_floor_name :
                                                                param.iSortCol_0 == 5 ? c.trailer_type_name : c.trailer_no);

        IEnumerable<TrailerViewModel> listQuery;
        if (param.sSortDir_0 == "asc")
        {
            listQuery = data.OrderBy(orderingFunction)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength);
        }
        else
        {
            listQuery = data.OrderByDescending(orderingFunction)
                            .Skip(param.iDisplayStart)
                            .Take(param.iDisplayLength);
        }

        return Json(new
        {
            sEcho = param.sEcho,
            iTotalRecords = data.Count(),
            iTotalDisplayRecords = data.Count(),
            aaData = listQuery
        });
    }

    public async Task<IActionResult> Remove(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await trailerRepo.Remove(code, username);
        return Json(result);
    }

    public async Task<IActionResult> Active(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await trailerRepo.Active(code, username);
        return Json(result);
    }

    public async Task<IActionResult> Inactive(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await trailerRepo.Inactive(code, username);
        return Json(result);
    }
}