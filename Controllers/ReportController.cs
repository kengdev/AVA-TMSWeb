using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Repositories;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;

namespace TMSWeb.Controllers;
[Authorize(Roles = "Administrator")]
public class ReportController : Controller
{
    private readonly ReportRepository reportRepo;
    private readonly DropListRepository dropListRepo;
    private string LoggedInUser => User.Identity.Name;

    public ReportController(ReportRepository _reportRepo, DropListRepository _dropListRepo)
    {
        reportRepo = _reportRepo;
        dropListRepo = _dropListRepo;
    }
    public IActionResult Index()
    {
        ViewData["month"] = from a in dropListRepo.GetMonth() select new SelectListItem { Value = a.key.ToString(), Text = a.label };
        ViewData["year"] = from a in dropListRepo.GetYear() select new SelectListItem { Value = a.key.ToString(), Text = a.label };

        return View();
    }

    public async Task<IActionResult> GetCostData(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        iFilter.month = iFilter.month == null ? "0" : iFilter.month;
        iFilter.year = iFilter.year == null ? "0" : iFilter.year;
        var data = await reportRepo.GetCostData(iFilter.year, iFilter.month);
        /*List<ReportJobViewModel> data_report = new List<ReportJobViewModel>();
        List<ReportAdvanceViewModel> data_advance = new List<ReportAdvanceViewModel>();
        foreach (var item in data)
        {
            var item_advance = _service.GetOtherAdvanceData(item.code);
            item.advance_list = item_advance;

            data_report.Add(item);
        }*/

        return Json(new
        {
            sEcho = param.sEcho,
            iTotalRecords = data.Count(),
            iTotalDisplayRecords = data.Count(),
            aaData = data
        });
    }

    public async Task<IActionResult> CostReportExcel(avaDataTableParamModel iFilter)
    {
        iFilter.month = iFilter.month == null ? "0" : iFilter.month;
        iFilter.year = iFilter.year == null ? "0" : iFilter.year;
        var data = await reportRepo.GetCostData(iFilter.year, iFilter.month);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("list");
            var colA = worksheet.Column("A");
            colA.Width = 15;
            colA.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colB = worksheet.Column("B");
            colB.Width = 20;
            colB.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colC = worksheet.Column("C");
            colC.Width = 20;
            colC.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colD = worksheet.Column("D");
            colD.Width = 25;
            colD.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colE = worksheet.Column("E");
            colE.Width = 20;
            colE.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colF = worksheet.Column("F");
            colF.Width = 20;
            colF.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colG = worksheet.Column("G");
            colG.Width = 10;
            colG.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colH = worksheet.Column("H");
            colH.Width = 30;
            colH.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colI = worksheet.Column("I");
            colI.Width = 20;
            colI.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colJ = worksheet.Column("J");
            colJ.Width = 15;
            colJ.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colK = worksheet.Column("K");
            colK.Width = 20;
            colK.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colL = worksheet.Column("L");
            colL.Width = 30;
            colL.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            var colM = worksheet.Column("M");
            colM.Width = 15;
            colM.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colN = worksheet.Column("N");
            colN.Width = 15;
            colN.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colO = worksheet.Column("O");
            colO.Width = 20;
            colO.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);


            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "Date";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 2).Value = "Job Code";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 3).Value = "Customer";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 4).Value = "Booking";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 5).Value = "Invoice";
            worksheet.Cell(currentRow, 5).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 6).Value = "Container";
            worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 7).Value = "Size";
            worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 8).Value = "Industrial";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 9).Value = "Zone";
            worksheet.Cell(currentRow, 9).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 10).Value = "Supplier";
            worksheet.Cell(currentRow, 10).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 11).Value = "Truck";
            worksheet.Cell(currentRow, 11).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 12).Value = "Driver";
            worksheet.Cell(currentRow, 12).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 13).Value = "Supplier Billing";
            worksheet.Cell(currentRow, 13).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 14).Value = "Customer Billing";
            worksheet.Cell(currentRow, 14).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 15).Value = "Billing Code";
            worksheet.Cell(currentRow, 15).Style.Font.Bold = true;

            foreach (var item in data)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = item.job_date_value.ToString("dd-MM-yyyy");
                worksheet.Cell(currentRow, 2).Value = item.code;
                worksheet.Cell(currentRow, 3).Value = item.customer_name;
                worksheet.Cell(currentRow, 4).Value = item.booking_no;
                worksheet.Cell(currentRow, 5).Value = item.invoice_no;
                worksheet.Cell(currentRow, 6).Value = item.container_no;
                worksheet.Cell(currentRow, 7).Value = item.container_size;
                worksheet.Cell(currentRow, 8).Value = item.mission_name;
                worksheet.Cell(currentRow, 9).Value = item.zone_name;
                worksheet.Cell(currentRow, 10).Value = item.supplier_name;
                worksheet.Cell(currentRow, 11).Value = item.truck_license + " [" + item.truck_no + "]";
                worksheet.Cell(currentRow, 12).Value = item.driver_name;
                worksheet.Cell(currentRow, 13).Value = item.supplier_charge;
                worksheet.Cell(currentRow, 14).Value = item.customer_charge;
                worksheet.Cell(currentRow, 15).Value = item.billing_code;

                var advance_list = await reportRepo.GetOtherAdvanceData(item.code);
                if (advance_list != null)
                {
                    foreach (var item_advance in advance_list)
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 12).Style.Font.SetFontColor(XLColor.Red);
                        worksheet.Cell(currentRow, 13).Style.Font.SetFontColor(XLColor.Red);
                        worksheet.Cell(currentRow, 14).Style.Font.SetFontColor(XLColor.Red);

                        worksheet.Cell(currentRow, 12).Value = item_advance.advance_name;
                        worksheet.Cell(currentRow, 13).Value = item_advance.supplier;
                        worksheet.Cell(currentRow, 14).Value = item_advance.customer;
                    }
                }
            }
            var filename = DateTime.Now.ToString("ddMMyyyyHHmm") + ".xlsx";
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            filename);
            }
        }
    }
}