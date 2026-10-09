using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using Microsoft.AspNetCore.Authorization;
using TMSWeb.Models.Entities.HR;

namespace TMSWeb.Controllers;

public class FormAVAController : Controller
{
    private readonly FormAVARepository formRepo;

    public FormAVAController(FormAVARepository _formRepo)
    {
        formRepo = _formRepo;
    }

    public async Task<IActionResult> ReceiptContainerHandling(string code)
    {
        var data = await formRepo.GetContainerHandlingData(code);

        return new ViewAsPdf("_ReceiptContainerHandling", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> ReceiptContainerHandlingList(string month, string year)
    {
        var data = await formRepo.GetContainerHandlingDataList(month, year);

        return new ViewAsPdf("_ReceiptContainerHandling", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> DriverSalarySlip(string payment, string emp)
    {
        var model = await formRepo.GetTblPaymentSalaryByCode(payment, emp);

        return new ViewAsPdf("_DriverSalarySlip", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A5,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 2, Bottom = 2, Right = 2, Top = 7 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> OfficeSalarySlip(string payment, string emp)
    {
        var model = await formRepo.GetTblPaymentSalaryByCode(payment, emp);

        return new ViewAsPdf("_OfficeSalarySlip", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A5,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 2, Bottom = 2, Right = 2, Top = 7 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> DailyPayment(string code)
    {
        var model = await formRepo.GetTblDailyPayments(code);

        return new ViewAsPdf("_DailyPayment", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 2, Right = 5, Top = 7 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> MonthlyPayment(string code)
    {
        var model = await formRepo.GetTblMonthlyPayments(code);

        return new ViewAsPdf("_MonthlyPayment", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 2, Right = 5, Top = 7 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> ReceivePayment(string code)
    {
        var model = await formRepo.GetReceivePayments(code);

        return new ViewAsPdf("_ReceivePayment", model)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 2, Right = 5, Top = 7 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> PayeeReceipt(string code)
    {
        var data = await formRepo.GetReceivePayments(code);

        return new ViewAsPdf("_PayeeReceipt", data)
        {
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }
}