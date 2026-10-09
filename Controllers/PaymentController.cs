using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using TMSWeb.Models.ViewModels;
using TMSWeb.Models.Entities.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace TMSWeb.Controllers;
public class PaymentController : Controller
{
    private readonly PaymentRepository paymentRepo;
    private readonly DropListRepository dropListRepo;
    private readonly BudgetRepository budgetRepo;
    private readonly CodeRepository codeRepo;
    private string LoggedInUser => User.Identity.Name;
    public PaymentController(PaymentRepository _paymentRepo, 
        DropListRepository _dropListRepo, 
        BudgetRepository _budgetRepo,
        CodeRepository _codeRepo)
    {
        paymentRepo = _paymentRepo;
        dropListRepo = _dropListRepo;
        budgetRepo = _budgetRepo;
        codeRepo = _codeRepo;
    }


    #region Driver payment
    [Authorize(Roles = "Administrator")]
    public IActionResult DriverPayment()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");

        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };
        return View();
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetDriverList(jQueryDataTableParamModel param, string month, string year)
    {
        var data = await paymentRepo.GetDriverPayment(month, year);
        if (!(string.IsNullOrEmpty(param.sSearch)))
        {
            data = data.Where(w => w.start_date_text.ToLower().Contains(param.sSearch.ToLower()) ||
                                w.end_date_text.ToLower().Contains(param.sSearch.ToLower())).ToList();
        }

        Func<DriverPaymentViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.start_date_text :
        param.iSortCol_0 == 2 ? c.end_date_text : c.start_date_text);

        IEnumerable<DriverPaymentViewModel> listQuery;
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

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> RemoveDriverPaymentItem(int id, string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await paymentRepo.RemoveDriverPaymentItem(id, code, username);
        return Json(result);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetDriverPaymentByDate(DateOnly start_date)
    {
        var query = await paymentRepo.GetDriverPaymentByDate(start_date);
        return Json(new
        {
            query
        });
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DriverPaymentDetail(string allowance_code, string driver_code)
    {
        var allowance = await paymentRepo.GetDriverPaymentListByCode(allowance_code);
        var substract = await paymentRepo.GetDriverSubtractByCode(allowance_code);
        var driver = await paymentRepo.GetTruckDriverByDriver(driver_code);

        ViewBag.Driver = driver;
        ViewBag.Substract = substract;
        ViewBag.Allowance = allowance;
        return View();
    }

    public async Task<IActionResult> EditDriverPayment(DriverPaymentModel model)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await paymentRepo.EditDriverPayment(model, username);
        return Json(result);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AddDriverPayment()
    {
        ViewData["driver"] = from a in await dropListRepo.GetDriver() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        return View();
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DriverPaymentEntry(string job_detail_code, DateTime working_date, string code, string driver_code)
    {
        ViewDriverPaymentListActive model = new ViewDriverPaymentListActive();
        if (job_detail_code != null)
        {
            model = await paymentRepo.GetDriverJobByCode(job_detail_code, driver_code);
            model.JobCode = code;
        }
        else
        {
            model.JobDate = working_date;
            model.JobCode = code;
        }

        return ViewComponent("EntryDriverPayment", model);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetDriverJob(string driver_code, DateTime start, DateTime end)
    {
        var data = await paymentRepo.GetOrderList(driver_code, start, end);
        return Json(data);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
    public async Task<IActionResult> AddDriverPaymentData(TblAllowance model, OtherPaymentViewModel other)
    {
        model.Remark = null;
        model.IsPayment = false;
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = this.LoggedInUser;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;

        ResponseViewModel result = await paymentRepo.AddDriverPayment(model, other);
        return Json(result);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> RemoveDriverPayment(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await paymentRepo.RemoveDriverPayment(code, username);
        return Json(result);
    }
    #endregion


    #region Supplier payment
    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> SupplierPayment()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["supplyer_code"] = from a in await dropListRepo.GetSupplierTransport() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> GetSupplierList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await paymentRepo.GetSupplierList(iFilter.month, iFilter.year);
        var data = query.Where(w => ((iFilter.filterCode == null || w.supplierpayment_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterSupplier == null || w.supplier_code.Contains(iFilter.filterSupplier.ToUpper()))));

        Func<SupplierPaymentViewModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.supplierpayment_code :
        param.iSortCol_0 == 3 ? c.supplierpayment_date :
        param.iSortCol_0 == 4 ? c.supplier_name : c.supplierpayment_code);

        IEnumerable<SupplierPaymentViewModel> listQuery;
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

    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> AddSupplierPayment()
    {
        TblSupplierpayment model = new TblSupplierpayment();
        model.SupplierpaymentDate = DateOnly.FromDateTime(DateTime.Now);

        ViewData["PaymentType"] = from a in dropListRepo.GetReceptType() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == "02" };
        ViewData["filterSupplyer"] = from a in await dropListRepo.GetSupplierTransport() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        return View(model);
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> EntrySupplierJob(DateTime start, DateTime end, string supplyer)
    {
        var data = await paymentRepo.GetSupplierJob(start, end, supplyer);

        return ViewComponent("EntrySupplierJob", data);
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public IActionResult EntrySupplierDetail(string code)
    {
        return ViewComponent("EntrySupplierDetail", code);
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> AddSupplierPaymentData(TblSupplierpayment model)
    {
        string username = LoggedInUser;

        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = username;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = username;

        ResponseViewModel result = await paymentRepo.AddSupplierPaymentData(model);
        return Json(result);
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> EditSupplierPayment(string code)
    {
        var model = await paymentRepo.GetSupplyerPaymentDataByCode(code);
        var list = await paymentRepo.GetSupplierJobByCode(code);

        ViewBag.List = list;
        ViewData["PaymentType"] = from a in dropListRepo.GetReceptType() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == model.PaymentType };
        ViewData["filterSupplyer"] = from a in await dropListRepo.GetSupplierTransport() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == model.SupplierCode };
        return View(model);
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> EditSupplierPaymentData(TblSupplierpayment model)
    {
        string username = LoggedInUser;

        model.UpdateDate = DateTime.Now;
        model.UpdateBy = username;

        ResponseViewModel result = await paymentRepo.EditSupplierPaymentData(model);
        return Json(result);
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> SupplierPaymentDetail(string code)
    {
        var list = await paymentRepo.GetSupplierJobByCode(code);
        var model = await paymentRepo.GetSupplyerPaymentDataByCode(code);
        ViewBag.List = list;
        return View(model);
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> EditSupplierTripCost(string job_detail_code, decimal value)
    {
        var username = LoggedInUser;

        var result = await paymentRepo.EditSupplierTripCost(job_detail_code, value, username);
        return Json(result);
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> EditSupplierInvoiceJob(string job_detail_code, string value)
    {
        var username = LoggedInUser;

        var result = await paymentRepo.EditSupplierInvoiceJob(job_detail_code, value, username);
        return Json(result);
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> EditSupplierAdvanceCost(string job_detail_code, decimal value)
    {
        var username = LoggedInUser;

        var result = await paymentRepo.EditSupplierAdvanceCost(job_detail_code, value, username);
        return Json(result);
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public async Task<IActionResult> RemoveSupplierPayment(string code)
    {
        string username = LoggedInUser;

        ResponseViewModel result = await paymentRepo.RemoveSupplierPayment(code, username);
        return Json(result);
    }
    #endregion


    #region fleet payment
    [Authorize(Roles = "Administrator")]
    public IActionResult FleetPayment()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetFleetList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await paymentRepo.GetFleetAllowanceList(iFilter.month, iFilter.year);
        var data = query.Where(w => ((iFilter.filterCode == null || w.supplierpayment_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterNameEN == null || w.supplier_name.Contains(iFilter.filterNameEN.ToUpper())) &&
        (iFilter.filterLicense == null || w.truck_license.Contains(iFilter.filterLicense.ToUpper()))));

        Func<SupplierPaymentViewModel, string> orderingFunction = (c => param.iSortCol_0 == 2 ? c.supplierpayment_date :
                                        param.iSortCol_0 == 3 ? c.supplierpayment_code :
                                        param.iSortCol_0 == 4 ? c.truck_license :
                                        param.iSortCol_0 == 5 ? c.supplier_name : c.supplierpayment_date);

        IEnumerable<SupplierPaymentViewModel> listQuery;
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

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AddFleetPayment()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["filterTruck"] = from a in await dropListRepo.GetTruck() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };
        return View();
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EntryFleetJob(DateTime start, DateTime end, string truck)
    {
        end = end.AddDays(1).AddSeconds(-1);
        var data = await paymentRepo.GetFleetJob(start, end, truck);

        return ViewComponent("EntryFleetJob", data);
    }

    [Authorize(Roles = "Administrator")]
    public IActionResult EntryFleetDetail(string code)
    {
        return ViewComponent("EntryFleetDetail", code);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
    public async Task<IActionResult> AddFleetPaymentData(TblFleetpayment model, string month, string year)
    {
        string username = LoggedInUser;
        model.FleetpaymentMonth = month;
        model.FleetpaymentYear = year;
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = username;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = username;

        ResponseViewModel result = await paymentRepo.AddFleetPaymentData(model);
        return Json(result);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditFleetPayment(string code)
    {
        var model = await paymentRepo.GetFleetPaymentDataByCode(code);
        var data = await paymentRepo.GetListFleetJobByCode(code);

        ViewBag.Data = data;
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == model.FleetpaymentMonth };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == model.FleetpaymentYear };
        return View(model);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditFleetPaymentData(TblFleetpayment model, string month, string year)
    {
        string username = LoggedInUser;

        model.FleetpaymentMonth = month;
        model.FleetpaymentYear = year;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = username;

        ResponseViewModel result = await paymentRepo.EditFleetPaymentData(model);
        return Json(result);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> FleetPaymentDetail(string code)
    {
        var list = await paymentRepo.GetListFleetJobByCode(code);
        var model = await paymentRepo.GetFleetPaymentByCode(code);
        model.month = this.ConvertMonth(model.month);

        ViewBag.List = list;
        return View(model);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditFleetTransportCost(string job_detail_code, int value)
    {
        var username = LoggedInUser;

        var result = await paymentRepo.EditFleetTransportCost(job_detail_code, value, username);
        return Json(result);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditFleetTripCost(string job_detail_code, int value)
    {
        var username = LoggedInUser;

        var result = await paymentRepo.EditFleetTripCost(job_detail_code, value, username);
        return Json(result);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditFleetFuelCost(string job_detail_code, int value)
    {
        var username = LoggedInUser;

        var result = await paymentRepo.EditFleetFuelCost(job_detail_code, value, username);
        return Json(result);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> EditFleetExpressCost(string job_detail_code, decimal value)
    {
        var username = LoggedInUser;

        var result = await paymentRepo.EditFleetExpressCost(job_detail_code, value, username);
        return Json(result);
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> RemoveFleetPayment(string code)
    {
        var result = await paymentRepo.RemoveFleetPayment(code, this.LoggedInUser);
        return Json(result);
    }
    #endregion


    #region Other Payment
    [Authorize]
    public IActionResult DailyPayment()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ApproveDailyPayment()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };
        var model = await budgetRepo.GetGeBudgetData();
        return View(model);
    }

    [Authorize(Roles = "Administrator, Accounting, Super")]
    public IActionResult MonthlyPayment()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    [Authorize]
    public IActionResult AddDailyPayment()
    {
        return View();
    }

    [Authorize(Roles = "Administrator, Accounting, Super")]
    public async Task<IActionResult> AddMonthlyPayment()
    {
        ViewData["BankName"] = from a in await dropListRepo.GetBank() select new SelectListItem { Value = a.label.ToString(), Text = a.label };
        ViewData["PaymentTypeCode"] = from a in await dropListRepo.GetPaymentType() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        return View();
    }

    public IActionResult _EntryDailyPayment(string action_type)
    {
        return ViewComponent("EntryDailyPayment", action_type);
    }

    public IActionResult _EntryMonthlyPayment(string action_type)
    {
        return ViewComponent("EntryMonthlyPayment", action_type);
    }

    [HttpPost]
    public async Task<IActionResult> AddDailyPayment(TblDailypayment model)
    {
        model.DailypaymentCode = await codeRepo.GetDailyPaymentCode();
        model.PaymentDate = DateOnly.FromDateTime(DateTime.Now);
        model.NetTotal = model.TblDailypaymentDetails.Sum(s => s.Amount);
        model.PaymentMethod = "01";
        model.ApproveStatus = "W";
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = this.LoggedInUser;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;
        var result = await paymentRepo.AddDailyPayment(model);

        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddMonthlyPayment(TblMonthlypayment model)
    {
        model.MonthlyPaymentCode = await codeRepo.GetMonthlyPaymentCode();

        if (model.PaymentMethod == "01")
        {
            model.BankName = null;
            model.AccountNo = null;
            model.BankBranchName = null;
            model.ChequeNo = null;
        }
        else if (model.PaymentMethod == "02")
        {
            model.BankBranchName = null;
            model.ChequeNo = null;
        }
        
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = this.LoggedInUser;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;
        var result = await paymentRepo.AddMonthlyPayment(model);

        return Json(result);
    }

    [Authorize]
    public async Task<IActionResult> EditDailyPayment(string code)
    {
        var model = await paymentRepo.GetDailyPaymentData(code);

        return View(model);
    }

    [Authorize(Roles = "Administrator, Accounting, Super")]
    public async Task<IActionResult> EditMonthlyPayment(string code)
    {
        var model = await paymentRepo.GetMonthlyPaymentData(code);

        ViewData["BankName"] = from a in await dropListRepo.GetBank() select new SelectListItem { Value = a.label.ToString(), Text = a.label, Selected = a.label == model.BankName };
        ViewData["PaymentTypeCode"] = from a in await dropListRepo.GetPaymentType() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == model.PaymentTypeCode };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditDailyPayment(TblDailypayment model)
    {
        var detail = new List<TblDailypaymentDetail>();
        foreach (var item in model.TblDailypaymentDetails)
        {
            if (item.Description != null && item.Amount > 0)
            {
                detail.Add(item);
            }
        }

        model.TblDailypaymentDetails = detail;
        model.ApproveStatus = "W";
        model.PaymentDate = DateOnly.FromDateTime(DateTime.Now);
        model.NetTotal = model.TblDailypaymentDetails.Sum(s => s.Amount);
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;
        var result = await paymentRepo.EditDailyPayment(model);

        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> EditMonthlyPayment(TblMonthlypayment model)
    {
        var detail = new List<TblMonthlypaymentDetail>();
        foreach (var item in model.TblMonthlypaymentDetails)
        {
            if (item.Description != null && item.Amount > 0)
            {
                detail.Add(item);
            }
        }

        model.TblMonthlypaymentDetails = detail;
        if (model.PaymentMethod == "01")
        {
            model.BankName = null;
            model.AccountNo = null;
            model.BankBranchName = null;
            model.ChequeNo = null;
        }
        else if (model.PaymentMethod == "02")
        {
            model.BankBranchName = null;
            model.ChequeNo = null;
        }
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;
        var result = await paymentRepo.EditMonthlyPayment(model);

        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveDailyPayment(string code)
    {
        var result = await paymentRepo.RemoveDailyPayment(code, this.LoggedInUser);

        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> ApproveDailyPayment(string code)
    {
        var result = await paymentRepo.Approve(code, this.LoggedInUser);

        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> RejectDailyPayment(string code)
    {
        var result = await paymentRepo.Reject(code, this.LoggedInUser);

        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveMonthlyPayment(string code)
    {
        var result = await paymentRepo.RemoveMonthlyPayment(code, this.LoggedInUser);

        return Json(result);
    }

    [Authorize(Roles = "Administrator,Accounting")]
    public IActionResult Receive()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    [Authorize(Roles = "Administrator, Accounting, Super")]
    public async Task<IActionResult> AddReceive()
    {
        ViewData["BankName"] = from a in await dropListRepo.GetBank() select new SelectListItem { Value = a.label.ToString(), Text = a.label };
        ViewData["PaymentTypeCode"] = from a in await dropListRepo.GetPaymentType() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        return View();
    }

    public async Task<IActionResult> GetDailyDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await paymentRepo.GetDailyPaymentDataList(iFilter.month, iFilter.year, this.LoggedInUser);
        var data = query.Where(w => (iFilter.filterCode == null || w.payment_id.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterCustomer == null || w.payment_name.Contains(iFilter.filterCustomer.ToUpper())));

        Func<PaymentModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.payment_date :
                                                                param.iSortCol_0 == 2 ? c.payment_id :
                                                                param.iSortCol_0 == 3 ? c.payment_name : c.payment_id);

        IEnumerable<PaymentModel> listQuery;
        if (param.sSortDir_0 == "desc")
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

    public async Task<IActionResult> GetAllDailyDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await paymentRepo.GetAllDailyPaymentDataList(iFilter.month, iFilter.year);
        var data = query.Where(w => (iFilter.filterCode == null || w.payment_id.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterCustomer == null || w.payment_name.Contains(iFilter.filterCustomer.ToUpper())));

        Func<PaymentModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.payment_date :
                                                                param.iSortCol_0 == 2 ? c.payment_id :
                                                                param.iSortCol_0 == 3 ? c.payment_name : c.payment_id);

        IEnumerable<PaymentModel> listQuery;
        if (param.sSortDir_0 == "desc")
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

    public async Task<IActionResult> GetMonthlyDataList(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await paymentRepo.GetMonthlyPaymentDataList(iFilter.month, iFilter.year);
        var data = query.Where(w => (iFilter.filterCode == null || w.payment_id.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterCustomer == null || w.payment_name.Contains(iFilter.filterCustomer.ToUpper())));

        Func<PaymentModel, string> orderingFunction = (c => param.iSortCol_0 == 1 ? c.payment_date :
                                                                param.iSortCol_0 == 2 ? c.payment_id :
                                                                param.iSortCol_0 == 3 ? c.payment_name : c.payment_id);

        IEnumerable<PaymentModel> listQuery;
        if (param.sSortDir_0 == "desc")
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
    #endregion

    private string ConvertMonth(string month)
    {
        switch (month)
        {
            case "01":
                month = "JANUARY";
                break;
            case "02":
                month = "FEBUARY";
                break;
            case "03":
                month = "MARCH";
                break;
            case "04":
                month = "APRIL";
                break;
            case "05":
                month = "MAY";
                break;
            case "06":
                month = "JUNE";
                break;
            case "07":
                month = "JULY";
                break;
            case "08":
                month = "AUGUST";
                break;
            case "09":
                month = "SEPTEMBER";
                break;
            case "10":
                month = "OCTOBER";
                break;
            case "11":
                month = "NOVEMBER";
                break;
            case "12":
                month = "DECEMBER";
                break;
        }

        return month;
    }
}