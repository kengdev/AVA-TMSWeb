using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using TMSWeb.Models.ViewModels;
using TMSWeb.Models;
using TMSWeb.Repositories;
using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;
namespace TMSWeb.Controllers;

[Authorize(Roles = "Administrator")]
public class SalaryController : Controller
{
    private readonly DropListRepository dropListRepo;
    private readonly SalaryRepository salaryRepo;
    private readonly CodeRepository codeRepo;
    private readonly EmployeeRepository empRepo;

    private string LoggedInUser => User.Identity.Name;

    public SalaryController(DropListRepository _dropListRepo,
        SalaryRepository _salaryRepo,
        CodeRepository _codeRepo,
        EmployeeRepository _empRepo)
    {
        dropListRepo = _dropListRepo;
        salaryRepo = _salaryRepo;
        codeRepo = _codeRepo;
        empRepo = _empRepo;
    }

    public IActionResult Driver()
    {
        string year = DateTime.Now.ToString("yyyy");

        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };
        return View();
    }

    public IActionResult Office()
    {
        string year = DateTime.Now.ToString("yyyy");

        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };
        return View();
    }

    public async Task<IActionResult> ListDriverItem(string code)
    {
        var _salary = await salaryRepo.GetSalaryList(code);

        return View(_salary);
    }

    public async Task<IActionResult> ListOfficeItem(string code)
    {
        var _salary = await salaryRepo.GetSalaryList(code);

        ViewBag.Code = code;
        return View(_salary);
    }

    public IActionResult AddDriverSalary()
    {
        return View();
    }

    public IActionResult AddOfficeSalary()
    {
        string month = DateTime.Now.ToString("MM");
        string year = DateTime.Now.ToString("yyyy");

        ViewData["PaymentMonth"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == month };
        ViewData["PaymentYear"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label, Selected = a.key == year };

        return View();
    }

    public async Task<IActionResult> AddDriverSalaryData(TblPaymentSalary model)
    {
        var _employeeData = await salaryRepo.GetTblDriverListActive();
        _employeeData = _employeeData.OrderBy(o => o.emp_fullname).ToList();
        model.PaymentCode = await codeRepo.GetSalaryPaymentCode();
        model.PaymentDate = model.PaymentDate;
        foreach (var item in _employeeData)
        {
            decimal last_trip = 0;
            TblPaymentSalaryDetail _salaryDetail = new TblPaymentSalaryDetail();
            List<TblPaymentSalaryDetail> _salaryDetailList = new List<TblPaymentSalaryDetail>();
            List<TblPaymentSalaryAddition> _salaryAdditionList = new List<TblPaymentSalaryAddition>();
            _salaryDetail.PaymentCode = model.PaymentCode;

            var _tripPayment = await salaryRepo.GetPaymentTripByDriver(item.driver_code, model.PaymentStart, model.PaymentEnd);
            foreach (var itemTrip in _tripPayment)
            {
                TblPaymentSalaryAddition _addition = new TblPaymentSalaryAddition();
                _addition.EmpCode = item.emp_code;
                _addition.AllowanceCode = itemTrip.AllowanceCode;
                _addition.PaymentAdditionName = "ค่าเที่ยว " + itemTrip.StartDate.ToString("dd-MM-yyyy") + " - " + itemTrip.EndDate.ToString("dd-MM-yyyy");
                _addition.PaymentValue = itemTrip.AllowanceTotal;
                _addition.AdditionType = "T";
                _addition.PaymentCode = model.PaymentCode;
                _salaryAdditionList.Add(_addition);
                model.TblPaymentSalaryAdditions.Add(_addition);
            }
            var countAdditionList = _salaryAdditionList.Count();
            last_trip = countAdditionList > 0 ? _salaryAdditionList[countAdditionList - 1].PaymentValue : 0;


            var _fund = await salaryRepo.GetFundByEmp(item.emp_code);
            _salaryDetail.EmpCode = item.emp_code;
            _salaryDetail.SalaryType = "W";
            _salaryDetail.IncomingSalary = await salaryRepo.GetEmployeeSalaryByEmp(item.emp_code);
            _salaryDetail.IncomingTotal = _salaryDetail.IncomingSalary + _salaryDetail.IncomingDiligence + _salaryAdditionList.Sum(s => s.PaymentValue);

            _salaryDetail.DedictionSso = _fund.SsoDeductValue;
            _salaryDetail.DedictionLate = 0;
            _salaryDetail.DedictionAbsent = 0;
            _salaryDetail.IncomingOt = 0;
            _salaryDetail.IncomingPosition = 0;
            _salaryDetail.DedictionSecure = _fund.SecureDeductValue;
            _salaryDetail.DedictionTotal = _salaryDetail.DedictionSso + _salaryDetail.DedictionLate + _salaryDetail.DedictionAbsent + _salaryDetail.DedictionSecure;
            _salaryDetail.PaymentTransfer = _salaryDetail.IncomingDiligence + last_trip - _salaryDetail.DedictionTotal;

            _salaryDetail.PaymentNetTotal = _salaryDetail.IncomingTotal - _salaryDetail.DedictionTotal;
            _salaryDetail.IsEnabled = true;
            _salaryDetail.CreateDate = DateTime.Now;
            _salaryDetail.CreateBy = this.LoggedInUser;
            _salaryDetail.UpdateDate = DateTime.Now;
            _salaryDetail.UpdateBy = this.LoggedInUser;
            _salaryDetailList.Add(_salaryDetail);
            model.TblPaymentSalaryDetails.Add(_salaryDetail);
        }

        model.PaymentType = "01";
        model.PaymentDate = DateOnly.FromDateTime(DateTime.Now);
        model.PaymentTotal = model.TblPaymentSalaryDetails.Sum(s => s.PaymentNetTotal);
        model.PaymentAmount = model.TblPaymentSalaryDetails.Count();
        model.PaymentMonth = model.PaymentEnd.ToString("MM");
        model.PaymentYear = model.PaymentEnd.ToString("yyyy");
        model.IsActived = false;
        model.IsEnabled = true;
        model.CreateDate = DateTime.Now;
        model.CreateBy = this.LoggedInUser;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;

        var response = await salaryRepo.AddSalary(model);
        return Json(response);
    }

    public async Task<IActionResult> GetDriverSalaryList(string year)
    {
        var _salary = await salaryRepo.GetDriverSalaryListByYear(year);

        return Json(new
        {
            iTotalRecords = _salary.Count(),
            iTotalDisplayRecords = _salary.Count(),
            aaData = _salary
        });
    }

    public async Task<IActionResult> GetOfficeSalaryList(string year)
    {
        var _salary = await salaryRepo.GetOfficeSalaryListByYear(year);

        return Json(new
        {
            iTotalRecords = _salary.Count(),
            iTotalDisplayRecords = _salary.Count(),
            aaData = _salary
        });
    }

    public async Task<IActionResult> IssueDriverSalary(string payment_code, string emp_code)
    {
        var model = await salaryRepo.GetTblPaymentSalaryDetailByCode(payment_code, emp_code);
        var additionData = await salaryRepo.GetTblPaymentSalaryAdditionsByCode(payment_code, emp_code);
        var dedictionData = await salaryRepo.GetTblPaymentSalaryDedictionsByCode(payment_code, emp_code);
        var attendance = await salaryRepo.GetWorkAttendance(payment_code, emp_code);
        var _empData = await empRepo.GetEmployeeByCode(emp_code);

        ViewBag.Addition = additionData;
        ViewBag.Dediction = dedictionData;
        ViewBag.Attendance = attendance;
        ViewBag.Profile = _empData;
        return View(model);
    }

    public async Task<IActionResult> IssueOfficeSalary(string payroll, string emp)
    {
        var model = await salaryRepo.GetTblPaymentSalaryDetailByCode(payroll, emp);
        var additionData = await salaryRepo.GetTblPaymentSalaryAdditionsByCode(payroll, emp);
        var dedictionData = await salaryRepo.GetTblPaymentSalaryDedictionsByCode(payroll, emp);
        var attendance = await salaryRepo.GetWorkAttendance(payroll, emp);
        var _empData = await empRepo.GetEmployeeByCode(emp);

        ViewBag.Addition = additionData;
        ViewBag.Dediction = dedictionData;
        ViewBag.Attendance = attendance;
        ViewBag.Profile = _empData;
        return View(model);
    }

    public async Task<IActionResult> IssueData(TblPaymentSalaryDetail model, string[] tripList, string[] additionName, decimal[] additionValue, string[] dedictionName, decimal[] dedictionValue)
    {
        List<TblPaymentSalaryAddition> listAddition = new List<TblPaymentSalaryAddition>();
        for (int i = 0; i < tripList.Count(); i++)
        {
            TblPaymentSalaryAddition addition = new TblPaymentSalaryAddition();
            var paymentTrip = await salaryRepo.GetDriverAllowance(tripList[i]);

            addition.EmpCode = model.EmpCode;
            addition.AllowanceCode = tripList[i];
            addition.PaymentAdditionName = "ค่าเที่ยว " + paymentTrip.StartDate.ToString("dd-MM-yyyy") + " - " + paymentTrip.EndDate.ToString("dd-MM-yyyy");
            addition.PaymentValue = paymentTrip.AllowanceTotal;
            addition.AdditionType = "T";
            addition.PaymentCode = model.PaymentCode;
            listAddition.Add(addition);
        }

        for (int i = 0; i < additionValue.Count(); i++)
        {
            TblPaymentSalaryAddition addition = new TblPaymentSalaryAddition();
            addition.EmpCode = model.EmpCode;
            addition.PaymentCode = model.PaymentCode;
            addition.PaymentAdditionName = additionName[i];
            addition.PaymentValue = additionValue[i];
            addition.AdditionType = "O";
            listAddition.Add(addition);
        }

        List<TblPaymentSalaryDediction> listDediction = new List<TblPaymentSalaryDediction>();
        for (int i = 0; i < dedictionValue.Count(); i++)
        {
            TblPaymentSalaryDediction dediction = new TblPaymentSalaryDediction();
            dediction.EmpCode = model.EmpCode;
            dediction.PaymentCode = model.PaymentCode;
            dediction.PaymentDedictionName = dedictionName[i];
            dediction.PaymentValue = dedictionValue[i];
            listDediction.Add(dediction);
        }

        decimal last_trip = 0;
        decimal total_trip = 0;
        var payment = await salaryRepo.GetTblPaymentSalary(model.PaymentCode);
        var _driverData = await salaryRepo.GetTblEmployeeDriver(model.EmpCode);
        if (_driverData != null)
        {
            var list_trip = await salaryRepo.GetPaymentTripByDriver(_driverData.DriverCode, payment.PaymentStart, payment.PaymentEnd);
            total_trip = list_trip.Sum(s => s.AllowanceTotal);
            var countTrip = list_trip.Count;
            if (countTrip > 0)
                last_trip = list_trip[list_trip.Count - 1].AllowanceTotal;
        }

        model.IncomingTotal = model.IncomingPosition + model.IncomingOt + model.IncomingSalary + model.IncomingDiligence + listAddition.Sum(s => s.PaymentValue);


        model.DedictionTotal = model.DedictionAbsent + model.DedictionLate + model.DedictionSecure + model.DedictionSso + listDediction.Sum(s => s.PaymentValue);


        model.PaymentNetTotal = model.IncomingTotal - model.DedictionTotal;
        if (model.SalaryType == "W")
        {
            model.PaymentTransfer = (model.IncomingPosition + model.IncomingOt + model.IncomingSalary + model.IncomingDiligence + last_trip + listAddition.Where(w => w.AdditionType == "O").Sum(s => s.PaymentValue)) - model.DedictionTotal;
        }
        else
        {
            model.PaymentTransfer = (model.IncomingPosition + model.IncomingOt + model.IncomingSalary + model.IncomingDiligence + listAddition.Sum(s => s.PaymentValue)) - model.DedictionTotal;
        }

        model.IsApprove = true;
        model.UpdateDate = DateTime.Now;
        model.UpdateBy = this.LoggedInUser;

        ResponseViewModel response = await salaryRepo.EditSalaryItem(model, listAddition, listDediction);
        return Json(response);
    }

    public async Task<IActionResult> RemoveItem(string payment_code, string emp_code)
    {
        var response = await salaryRepo.RemoveSalaryItem(payment_code, emp_code, this.LoggedInUser);
        return Json(response);
    }

    public async Task<IActionResult> RemoveAllItem(string payment_code, string emp_code)
    {
        var response = await salaryRepo.RemoveSalaryAll(payment_code, this.LoggedInUser);
        return Json(response);
    }

    public async Task<IActionResult> ActiveAllItem(string payment_code)
    {
        var response = await salaryRepo.ActiveSalaryAll(payment_code, this.LoggedInUser);
        return Json(response);
    }

    public async Task<IActionResult> InactiveAllItem(string payment_code)
    {
        var response = await salaryRepo.InactiveSalaryAll(payment_code, this.LoggedInUser);
        return Json(response);
    }
}