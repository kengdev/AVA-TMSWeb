using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;
using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Rotativa.AspNetCore;

namespace TMSWeb.Controllers;

[Authorize]
public class ExportController : Controller
{
    private readonly AttendanceExcelService attendanceExcelService;
    private readonly ExportRepository exportRepo;
    private readonly JobRepository jobRepo;
    private readonly OtherReceiptRepository otherReceiptRepo;
    private readonly BudgetRepository budgetRepo;
    private readonly BillingRepository billingRepo;
    private readonly FormBillingRepository frmRepo;
    private readonly DbavalogContext db;
    private string connStr;
    public ExportController(DbavalogContext context,
        AttendanceExcelService _attendanceExcelService,
        JobRepository _jobRepo,
        OtherReceiptRepository _otherReceiptRepo,
        BudgetRepository _budgetRepo,
        BillingRepository _billingRepo,
        FormBillingRepository _frmRepo,
        ExportRepository _exportRepo)
    {
        db = context;
        connStr = db.Database.GetConnectionString();
        attendanceExcelService = _attendanceExcelService;
        exportRepo = _exportRepo;
        budgetRepo = _budgetRepo;
        jobRepo = _jobRepo;
        billingRepo = _billingRepo;
        frmRepo = _frmRepo;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> MonthlyAttendance(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken)
    {
        if (!startDate.HasValue ||
            !endDate.HasValue)
        {
            TempData["ErrorMessage"] =
                "กรุณาระบุวันที่เริ่มต้นและวันที่สิ้นสุด";

            return RedirectToAction(nameof(Index));
        }

        if (endDate.Value.Date <
            startDate.Value.Date)
        {
            TempData["ErrorMessage"] =
                "วันที่สิ้นสุดต้องไม่น้อยกว่าวันที่เริ่มต้น";

            return RedirectToAction(nameof(Index));
        }

        try
        {
            var excelBytes =
                await attendanceExcelService
                    .ExportAttendanceAsync(
                        startDate.Value,
                        endDate.Value,
                        cancellationToken);

            var fileName =
                $"Attendance-" +
                $"{startDate.Value:yyyyMMdd}-" +
                $"{endDate.Value:yyyyMMdd}.xlsx";

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(nameof(Index));
        }
        catch (OperationCanceledException)
        {
            return StatusCode(
                StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] =
                $"ไม่สามารถสร้างรายงานได้: {ex.Message}";

            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> TrackingReportExcel(avaDataTableParamModel iFilter)
    {
        var query = await jobRepo.GetJobDataList(iFilter.filterDateRangeStart, iFilter.filterDateRangeEnd);
        var data = query.Where(w => ((iFilter.filterCustomer == null || w.customer_name.Contains(iFilter.filterCustomer.ToUpper())) &&
        (iFilter.filterMission == null || w.mission_name.Contains(iFilter.filterMission.ToUpper())) &&
        (iFilter.filterBookingNo == null || w.booking_no.Contains(iFilter.filterBookingNo.ToUpper())) &&
        (iFilter.filterContainerSize == null || w.container_size.Contains(iFilter.filterContainerSize)) &&
        (iFilter.filterContainerNo == null || w.container_no.Contains(iFilter.filterContainerNo.ToUpper())) &&
        (iFilter.filterLicense == null || w.truck_license.Contains(iFilter.filterLicense)))).OrderBy(o => o.job_date).ToList();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("list");
            var colA = worksheet.Column("A");
            colA.Width = 15;
            colA.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colB = worksheet.Column("B");
            colB.Width = 10;
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
            colF.Width = 10;
            colF.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colG = worksheet.Column("G");
            colG.Width = 20;
            colG.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colH = worksheet.Column("H");
            colH.Width = 12;
            colH.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colI = worksheet.Column("I");
            colI.Width = 15;
            colI.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colJ = worksheet.Column("J");
            colJ.Width = 30;

            var colK = worksheet.Column("K");
            colK.Width = 20;

            var colL = worksheet.Column("L");
            colL.Width = 15;
            colL.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colM = worksheet.Column("M");
            colM.Width = 40;


            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "Date";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 2).Value = "Time";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 3).Value = "B/L No.";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 4).Value = "Industrial";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 5).Value = "Container";
            worksheet.Cell(currentRow, 5).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 6).Value = "Size";
            worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 7).Value = "Seal";
            worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 8).Value = "Tare";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 9).Value = "License";
            worksheet.Cell(currentRow, 9).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 10).Value = "Driver Name";
            worksheet.Cell(currentRow, 10).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 11).Value = "Mobile";
            worksheet.Cell(currentRow, 11).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 12).Value = "Supplier";
            worksheet.Cell(currentRow, 12).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 13).Value = "Remark";
            worksheet.Cell(currentRow, 13).Style.Font.Bold = true;

            foreach (var item in data)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = item.job_date_value.ToString("dd-MM-yyyy");
                worksheet.Cell(currentRow, 2).Value = item.job_date_value.ToString("HH:mm");
                worksheet.Cell(currentRow, 3).Value = item.booking_no;
                worksheet.Cell(currentRow, 4).Value = item.mission_name;
                worksheet.Cell(currentRow, 5).Value = item.container_no;
                worksheet.Cell(currentRow, 6).Value = item.container_size;
                worksheet.Cell(currentRow, 7).Value = item.seal_no;
                worksheet.Cell(currentRow, 8).Value = item.tare_weight;
                worksheet.Cell(currentRow, 9).Value = item.truck_license;
                worksheet.Cell(currentRow, 10).Value = item.driver_fullname;
                worksheet.Cell(currentRow, 11).Value = item.driver_phone;
                worksheet.Cell(currentRow, 12).Value = item.supplier_name;
                worksheet.Cell(currentRow, 13).Value = item.job_remark;

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

    public async Task<IActionResult> ReceiptHandlingExcel(jQueryDataTableParamModel param, avaDataTableParamModel iFilter)
    {
        var query = await otherReceiptRepo.GetContainerHandlingDataList(iFilter.month, iFilter.year);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("list");

            var colA = worksheet.Column("A");
            colA.Width = 15;
            colA.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colB = worksheet.Column("B");
            colB.Width = 15;
            colB.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colC = worksheet.Column("C");
            colC.Style.NumberFormat.Format = "@";
            colC.Width = 20;
            colC.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colD = worksheet.Column("D");
            colD.Width = 50;
            colD.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            var colE = worksheet.Column("E");
            colE.Width = 100;
            colE.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            var colF = worksheet.Column("F");
            colF.Width = 30;
            colF.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colG = worksheet.Column("G");
            colG.Width = 15;
            colG.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colH = worksheet.Column("H");
            colH.Width = 15;
            colH.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colI = worksheet.Column("I");
            colI.Width = 15;
            colI.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "INV";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 2).Value = "Date";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 3).Value = "Tax Id";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 4).Value = "Customer";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 5).Value = "Address";
            worksheet.Cell(currentRow, 5).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 6).Value = "Container No.";
            worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 7).Value = "Total";
            worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 8).Value = "Vat";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 9).Value = "NetTotal";
            worksheet.Cell(currentRow, 9).Style.Font.Bold = true;

            foreach (var item in query)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = item.code;
                worksheet.Cell(currentRow, 2).Value = item.receipt_date;
                worksheet.Cell(currentRow, 3).Value = item.customer_tax.ToString();
                worksheet.Cell(currentRow, 4).Value = item.customer_name;
                worksheet.Cell(currentRow, 5).Value = item.customer_address;
                worksheet.Cell(currentRow, 6).Value = item.container_no;
                worksheet.Cell(currentRow, 7).Value = item.total;
                worksheet.Cell(currentRow, 8).Value = item.vat;
                worksheet.Cell(currentRow, 9).Value = item.net_total;

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

    public async Task<IActionResult> TrackingReportPdf(avaDataTableParamModel iFilter)
    {
        var query = await exportRepo.GetCustomerTrackingList(iFilter.filterDateRangeStart, iFilter.filterDateRangeEnd);
        var data = query.Where(w => ((iFilter.filterCustomer == null || w.customer_name.Contains(iFilter.filterCustomer.ToUpper())) &&
        (iFilter.filterMission == null || w.mission_name.Contains(iFilter.filterMission.ToUpper())) &&
        (iFilter.filterBookingNo == null || w.booking_no.Contains(iFilter.filterBookingNo.ToUpper())) &&
        (iFilter.filterContainerSize == null || w.container_size.Contains(iFilter.filterContainerSize)) &&
        (iFilter.filterContainerNo == null || w.container_no.Contains(iFilter.filterContainerNo.ToUpper())) &&
        (iFilter.filterLicense == null || w.truck_license.Contains(iFilter.filterLicense)))).OrderBy(o => o.job_date).ToList();

        var filename = "tracking-" + DateTime.Now.ToString("ddMMyy") + ".pdf";
        return new ViewAsPdf("_TrackingInvoicePdf", data)
        {
            FileName = filename,
            PageSize = Rotativa.AspNetCore.Options.Size.Legal,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> PaymentReportExcel(avaDataTableParamModel iFilter)
    {
        var query = await exportRepo.GetPaymentList(iFilter.filterDateRangeStart, iFilter.filterDateRangeEnd);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("list");
            var colA = worksheet.Column("A");
            colA.Width = 15;
            colA.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colB = worksheet.Column("B");
            colB.Width = 10;
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
            colG.Width = 12;
            colG.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colH = worksheet.Column("H");
            colH.Width = 12;
            colH.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colI = worksheet.Column("I");
            colI.Width = 30;

            var colJ = worksheet.Column("J");
            colJ.Width = 12;
            colJ.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colK = worksheet.Column("K");
            colK.Width = 30;

            var colL = worksheet.Column("L");
            colL.Width = 12;
            colL.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colM = worksheet.Column("M");
            colM.Width = 12;

            var colN = worksheet.Column("N");
            colN.Width = 12;

            var colO = worksheet.Column("O");
            colO.Width = 12;

            var colP = worksheet.Column("P");
            colP.Width = 12;

            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "Date";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 2).Value = "Time";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 3).Value = "Type";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 4).Value = "B/L No.";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 5).Value = "Industrial";
            worksheet.Cell(currentRow, 5).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 6).Value = "Container";
            worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 7).Value = "Size";
            worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 8).Value = "License";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 9).Value = "Driver Name";
            worksheet.Cell(currentRow, 9).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 10).Value = "Supplier";
            worksheet.Cell(currentRow, 10).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 11).Value = "สถานที่รับตู้";
            worksheet.Cell(currentRow, 11).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 12).Value = "น้ำมัน";
            worksheet.Cell(currentRow, 12).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 13).Value = "ค่าผ่านท่า";
            worksheet.Cell(currentRow, 13).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 14).Value = "ค่ายกตู้";
            worksheet.Cell(currentRow, 14).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 15).Value = "ค่าทางด่วน";
            worksheet.Cell(currentRow, 15).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 16).Value = "ค่า ช.";
            worksheet.Cell(currentRow, 16).Style.Font.Bold = true;

            foreach (var item in query)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = item.job_date.ToString("dd-MM-yyyy");
                worksheet.Cell(currentRow, 2).Value = item.job_date.ToString("HH:mm");
                worksheet.Cell(currentRow, 3).Value = item.booking_no;
                worksheet.Cell(currentRow, 4).Value = item.booking_no;
                worksheet.Cell(currentRow, 5).Value = item.industrial;
                worksheet.Cell(currentRow, 6).Value = item.container_no;
                worksheet.Cell(currentRow, 7).Value = item.container_size;
                worksheet.Cell(currentRow, 8).Value = item.truck_license;
                worksheet.Cell(currentRow, 9).Value = item.driver_fullname;
                worksheet.Cell(currentRow, 10).Value = item.supplier;
                worksheet.Cell(currentRow, 11).Value = item.start_name;
                worksheet.Cell(currentRow, 12).Value = item.fuel;
                worksheet.Cell(currentRow, 13).Value = item.port;
                worksheet.Cell(currentRow, 14).Value = item.lift;
                worksheet.Cell(currentRow, 15).Value = item.express;
                worksheet.Cell(currentRow, 16).Value = item.sh;
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

    public async Task<IActionResult> CustomerReportExcel(avaDataTableParamModel iFilter)
    {
        var data = await exportRepo.GetJobByCustomer(iFilter.filterCode, iFilter.month, iFilter.year);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("list");
            var colA = worksheet.Column("A");
            colA.Width = 7;
            colA.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colB = worksheet.Column("B");
            colB.Width = 15;
            colB.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colC = worksheet.Column("C");
            colC.Width = 10;
            colC.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colD = worksheet.Column("D");
            colD.Width = 10;
            colD.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colE = worksheet.Column("E");
            colE.Width = 20;
            colE.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colF = worksheet.Column("F");
            colF.Width = 10;
            colF.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colG = worksheet.Column("G");
            colG.Width = 50;
            colG.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            var colH = worksheet.Column("H");
            colH.Width = 20;
            colH.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            var colI = worksheet.Column("I");
            colI.Width = 20;
            colI.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colJ = worksheet.Column("J");
            colJ.Width = 10;
            colJ.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colK = worksheet.Column("K");
            colK.Width = 10;
            colK.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colL = worksheet.Column("L");
            colL.Width = 15;
            colL.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "ลำดับ";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 2).Value = "รหัส";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 3).Value = "วัน/เวลา";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 4).Value = "เวลา";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 5).Value = "Job/Invoice";
            worksheet.Cell(currentRow, 5).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 6).Value = "ลูกค้า";
            worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 7).Value = "สถานที่";
            worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 8).Value = "B/L No.";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 9).Value = "Container";
            worksheet.Cell(currentRow, 9).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 10).Value = "ค่าเที่ยว";
            worksheet.Cell(currentRow, 10).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 11).Value = "สำรองจ่าย";
            worksheet.Cell(currentRow, 11).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 12).Value = "การวางบิล";
            worksheet.Cell(currentRow, 12).Style.Font.Bold = true;

            int i = 0;
            foreach (var item in data)
            {
                currentRow++;
                i++;
                worksheet.Cell(currentRow, 1).Value = i.ToString();
                worksheet.Cell(currentRow, 2).Value = item.job_code;
                worksheet.Cell(currentRow, 3).Value = item.job_date.Date;
                worksheet.Cell(currentRow, 4).Value = item.job_date.TimeOfDay;
                worksheet.Cell(currentRow, 5).Value = item.job_invoice;
                worksheet.Cell(currentRow, 6).Value = item.customer_name;
                worksheet.Cell(currentRow, 7).Value = item.mission_name;
                worksheet.Cell(currentRow, 8).Value = item.booking_no;
                worksheet.Cell(currentRow, 9).Value = item.container_no;
                worksheet.Cell(currentRow, 10).Value = item.trip_cost;
                worksheet.Cell(currentRow, 11).Value = item.advance_cost;
                if (item.is_billing)
                {
                    worksheet.Cell(currentRow, 12).Value = "COMPLETED";
                }
                else
                {
                    worksheet.Cell(currentRow, 12).Value = "PENDING";
                }
            }

            var filename = "cus" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xlsx";
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

    public async Task<IActionResult> CustomerReportPdf(avaDataTableParamModel iFilter)
    {
        var data = await exportRepo.GetJobByCustomer(iFilter.filterCode, iFilter.month, iFilter.year);

        var filename = "cus-" + DateTime.Now.ToString("ddMMyyHHMM") + ".pdf";
        return new ViewAsPdf("_CustomerReportPdf", data)
        {
            FileName = filename,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> OverdueReportExcel(avaDataTableParamModel iFilter)
    {
        var data = await exportRepo.GetOverdueList(iFilter.filterCustomer, iFilter.month, iFilter.year);
        var total_trip = data.Sum(s => s.transport_value);
        var total_advance = data.Sum(s => s.advance_value);
        var net_total = data.Sum(s => s.billing_total);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("list");
            var colA = worksheet.Column("A");
            colA.Width = 7;
            colA.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colB = worksheet.Column("B");
            colB.Width = 15;
            colB.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colC = worksheet.Column("C");
            colC.Width = 13;
            colC.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colD = worksheet.Column("D");
            colD.Width = 15;
            colD.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colE = worksheet.Column("E");
            colE.Width = 20;
            colE.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colF = worksheet.Column("F");
            colF.Width = 10;
            colF.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colG = worksheet.Column("G");
            colG.Width = 15;
            colG.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colH = worksheet.Column("H");
            colH.Width = 15;
            colH.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colI = worksheet.Column("I");
            colI.Width = 15;
            colI.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "ลำดับ";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 2).Value = "รหัส";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 3).Value = "วันที่";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 4).Value = "ลูกค้า";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 5).Value = "สถานที่";
            worksheet.Cell(currentRow, 5).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 6).Value = "ประเภท";
            worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 7).Value = "ค่าเที่ยว";
            worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 8).Value = "สำรองจ่าย";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 9).Value = "ยอดรวม";
            worksheet.Cell(currentRow, 9).Style.Font.Bold = true;

            int i = 0;
            foreach (var item in data)
            {
                currentRow++;
                i++;
                worksheet.Cell(currentRow, 1).Value = i.ToString();
                worksheet.Cell(currentRow, 2).Value = item.billing_code;
                worksheet.Cell(currentRow, 3).Value = item.billing_date.ToString("dd-MM-yyyy");
                worksheet.Cell(currentRow, 4).Value = item.customer_name;
                worksheet.Cell(currentRow, 5).Value = item.mission_name;
                if (item.billing_type == "M")
                {
                    worksheet.Cell(currentRow, 6).Value = "รวม";
                }
                else if (item.billing_type == "T")
                {
                    worksheet.Cell(currentRow, 6).Value = "ค่าเที่ยว";
                }
                else if (item.billing_type == "A")
                {
                    worksheet.Cell(currentRow, 6).Value = "สำรองจ่าย";
                }
                else if (item.billing_type == "O")
                {
                    worksheet.Cell(currentRow, 6).Value = "อื่น ๆ";
                }
                worksheet.Cell(currentRow, 7).Value = item.transport_value;
                worksheet.Cell(currentRow, 8).Value = item.advance_value;
                worksheet.Cell(currentRow, 9).Value = item.billing_total;
            }

            currentRow = currentRow + 2;
            worksheet.Cell(currentRow, 6).Value = "ยอดรวม";
            worksheet.Cell(currentRow, 7).Value = total_trip;
            worksheet.Cell(currentRow, 8).Value = total_advance;
            worksheet.Cell(currentRow, 9).Value = net_total;

            var filename = "overdue-" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xlsx";
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

    public async Task<IActionResult> OverdueReportPdf(avaDataTableParamModel iFilter)
    {
        var data = await exportRepo.GetOverdueList(iFilter.filterCustomer, iFilter.month, iFilter.year);

        var filename = "overdue-" + DateTime.Now.ToString("ddMMyyHHMM") + ".pdf";
        return new ViewAsPdf("_OverdueReportPdf", data)
        {
            FileName = filename,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> BudgetTransactionExcel(avaDataTableParamModel iFilter)
    {
        var query = await budgetRepo.GetTransportBudgetTransaction(iFilter.filterDateRangeStart, iFilter.filterDateRangeEnd);
        var data = query.Where(w => (iFilter.filterCustomer == null || w.customer_name.ToUpper().Contains(iFilter.filterCustomer.ToUpper())) &&
        (iFilter.filterMission == null || w.mission.ToUpper().Contains(iFilter.filterMission.ToUpper())) &&
        (iFilter.filterLicense == null || w.transaction_type.Contains(iFilter.filterLicense)));

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("list");
            var colA = worksheet.Column("A");
            colA.Width = 15;
            colA.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colB = worksheet.Column("B");
            colB.Width = 20;
            colB.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            var colC = worksheet.Column("C");
            colC.Width = 20;
            colC.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colD = worksheet.Column("D");
            colD.Width = 20;
            colD.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colE = worksheet.Column("E");
            colE.Width = 20;
            colE.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colF = worksheet.Column("F");
            colF.Width = 20;
            colF.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colG = worksheet.Column("G");
            colG.Width = 20;
            colG.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colH = worksheet.Column("H");
            colH.Width = 20;
            colH.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colI = worksheet.Column("I");
            colI.Width = 15;
            colI.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colJ = worksheet.Column("J");
            colJ.Width = 15;
            colJ.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colK = worksheet.Column("K");
            colK.Width = 15;
            colK.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "Date";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 2).Value = "Job No.";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 3).Value = "Customer.";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 4).Value = "Booking No.";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 5).Value = "Container No";
            worksheet.Cell(currentRow, 5).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 6).Value = "Job Type";
            worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 7).Value = "Start";
            worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 8).Value = "To";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 9).Value = "Transaction";
            worksheet.Cell(currentRow, 9).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 10).Value = "Get";
            worksheet.Cell(currentRow, 10).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 11).Value = "Paid";
            worksheet.Cell(currentRow, 11).Style.Font.Bold = true;


            foreach (var item in data)
            {
                if (item.transaction_type == "GET")
                {
                    currentRow++;
                    string star_m = "B" + currentRow;
                    string end_m = "H" + currentRow;
                    var range = worksheet.Range(star_m + ":" + end_m);
                    range.Merge();
                    worksheet.Cell(currentRow, 1).Value = item.transaction_date;
                    worksheet.Cell(currentRow, 2).Value = "รับยอดเข้า";
                    worksheet.Cell(currentRow, 9).Value = "รับเข้า";

                    worksheet.Cell(currentRow, 10).Style.Font.SetFontColor(XLColor.Green);
                    worksheet.Cell(currentRow, 10).Value = item.amount;
                    worksheet.Cell(currentRow, 10).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                }
                else if (item.transaction_type == "JOB")
                {
                    if (item.payment_type == "MIN")
                    {
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = item.transaction_date;

                        worksheet.Cell(currentRow, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Cell(currentRow, 2).Value = item.job_detail_code;

                        worksheet.Cell(currentRow, 3).Value = item.customer_name;
                        worksheet.Cell(currentRow, 4).Value = item.booking;
                        worksheet.Cell(currentRow, 5).Value = item.container;
                        worksheet.Cell(currentRow, 6).Value = item.job_type;
                        worksheet.Cell(currentRow, 7).Value = item.start;
                        worksheet.Cell(currentRow, 8).Value = item.mission;
                        worksheet.Cell(currentRow, 9).Value = "จ่ายออก";

                        worksheet.Cell(currentRow, 11).Style.Font.SetFontColor(XLColor.Red);
                        worksheet.Cell(currentRow, 11).Value = item.amount;
                        worksheet.Cell(currentRow, 11).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                    }
                    else
                    {
                        if (item.amount_diff > 0)
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = item.transaction_date;

                            worksheet.Cell(currentRow, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            worksheet.Cell(currentRow, 2).Value = item.job_detail_code;

                            worksheet.Cell(currentRow, 3).Value = item.customer_name;
                            worksheet.Cell(currentRow, 4).Value = item.booking;
                            worksheet.Cell(currentRow, 5).Value = item.container;
                            worksheet.Cell(currentRow, 6).Value = item.job_type;
                            worksheet.Cell(currentRow, 7).Value = item.start;
                            worksheet.Cell(currentRow, 8).Value = item.mission;
                            worksheet.Cell(currentRow, 9).Value = "แก้ไข";

                            worksheet.Cell(currentRow, 10).Style.Font.SetFontColor(XLColor.Green);
                            worksheet.Cell(currentRow, 10).Value = item.amount_diff;
                            worksheet.Cell(currentRow, 10).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                        }
                        else if (item.amount_diff < 0)
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = item.transaction_date;

                            worksheet.Cell(currentRow, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            worksheet.Cell(currentRow, 2).Value = item.job_detail_code;

                            worksheet.Cell(currentRow, 3).Value = item.customer_name;
                            worksheet.Cell(currentRow, 4).Value = item.booking;
                            worksheet.Cell(currentRow, 5).Value = item.container;
                            worksheet.Cell(currentRow, 6).Value = item.job_type;
                            worksheet.Cell(currentRow, 7).Value = item.start;
                            worksheet.Cell(currentRow, 8).Value = item.mission;
                            worksheet.Cell(currentRow, 9).Value = "แก้ไข";

                            worksheet.Cell(currentRow, 11).Style.Font.SetFontColor(XLColor.Red);
                            worksheet.Cell(currentRow, 11).Value = Math.Abs(item.amount_diff);
                            worksheet.Cell(currentRow, 11).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                        }
                        else
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = item.transaction_date;

                            worksheet.Cell(currentRow, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            worksheet.Cell(currentRow, 2).Value = item.job_detail_code;

                            worksheet.Cell(currentRow, 3).Value = item.customer_name;
                            worksheet.Cell(currentRow, 4).Value = item.booking;
                            worksheet.Cell(currentRow, 5).Value = item.container;
                            worksheet.Cell(currentRow, 6).Value = item.job_type;
                            worksheet.Cell(currentRow, 7).Value = item.start;
                            worksheet.Cell(currentRow, 8).Value = item.mission;
                            worksheet.Cell(currentRow, 9).Value = "แก้ไข";
                        }
                    }
                }
                else
                {
                    currentRow++;
                    string star_m = "B" + currentRow;
                    string end_m = "H" + currentRow;
                    var range = worksheet.Range(star_m + ":" + end_m);
                    range.Merge().Style.Font.SetFontColor(XLColor.Red);
                    worksheet.Cell(currentRow, 1).Value = item.transaction_date;
                    worksheet.Cell(currentRow, 2).Value = "จ่าย" + item.remark;
                    worksheet.Cell(currentRow, 9).Value = "จ่ายออก";
                    worksheet.Cell(currentRow, 11).Value = item.amount;
                    worksheet.Cell(currentRow, 11).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
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

    public IActionResult TrackingReportExcelForCustomer(avaDataTableParamModel iFilter)
    {
        var customer = this.GetLoggedCode;
        var start = iFilter.filterDateRangeStart;
        var end = iFilter.filterDateRangeEnd;

        List<JobViewModel> query = new List<JobViewModel>();
        using (var connection = new SqlConnection(connStr))
        {
            connection.Open();
            var sql = "SELECT tbl_job_master.job_code, CASE WHEN tbl_job_detail.job_detail_code IS NULL THEN '-' ELSE " +
            "tbl_job_detail.job_detail_code END AS job_detail_code, CONVERT(varchar, tbl_job_master.job_date, 105) AS job_date,  " +
            "tbl_place.place_name As mission_name, tbl_job_master.booking_no, " +
            "CASE WHEN tbl_job_detail.job_detail_code IS NULL THEN '-' ELSE tbl_job_detail.container_no END AS container_no,  " +
            "CASE WHEN tbl_job_detail.job_detail_code IS NULL THEN '-' ELSE tbl_truck.truck_license END AS truck_license, " +
            "CASE WHEN tbl_job_detail.job_detail_code IS NULL THEN '-' ELSE tbl_job_detail.tracking_status END AS tracking_status, " +
            "tbl_job_master.job_type  " +
            "FROM tbl_job_master INNER JOIN " +
            "tbl_job_place ON tbl_job_master.job_code = tbl_job_place.job_code INNER JOIN " +
            "tbl_place ON tbl_job_place.mission_code = tbl_place.place_code INNER JOIN " +
            "tbl_customer ON tbl_job_master.customer_code = tbl_customer.customer_code LEFT OUTER JOIN " +
            "tbl_truck INNER JOIN " +
            "tbl_job_detail ON tbl_truck.truck_code = tbl_job_detail.truck_code ON tbl_job_master.job_code = tbl_job_detail.job_code " +
            "WHERE (tbl_job_detail.is_enabled IS NULL OR  " +
            "tbl_job_detail.is_enabled = 1) AND (tbl_job_master.job_date BETWEEN @start AND @end) AND (tbl_customer.customer_code = @customer)  " +
            "ORDER BY tbl_job_master.job_date DESC, tbl_job_detail.job_detail_code DESC";

            query = connection.Query<JobViewModel>(sql, new { customer, start, end }).ToList();
        }


        var data = query.Where(w => (iFilter.filterCode == null || w.job_detail_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterMission == null || w.mission_name.Contains(iFilter.filterMission.ToUpper())) &&
        (iFilter.filterBookingNo == null || w.booking_no.Contains(iFilter.filterBookingNo.ToUpper())) &&
        (iFilter.filterStatusCode == "--ทั้งหมด--" || w.tracking_status.Contains(iFilter.filterStatusCode.ToUpper())) &&
        (iFilter.filterLicense == null || w.truck_license.Contains(iFilter.filterLicense.ToUpper())) &&
        (iFilter.filterContainerNo == null || w.container_no.Contains(iFilter.filterContainerNo.ToUpper())));

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("list");
            var colA = worksheet.Column("A");
            colA.Width = 15;
            colA.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colB = worksheet.Column("B");
            colB.Width = 25;
            colB.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colC = worksheet.Column("C");
            colC.Width = 30;
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
            colG.Width = 20;
            colG.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "Date";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 2).Value = "Job";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 3).Value = "Industrial";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 4).Value = "Booking";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 5).Value = "Container";
            worksheet.Cell(currentRow, 5).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 6).Value = "License";
            worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 7).Value = "Status";
            worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

            foreach (var item in data)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = item.job_date;
                worksheet.Cell(currentRow, 2).Value = item.job_detail_code;
                worksheet.Cell(currentRow, 3).Value = item.mission_name;
                worksheet.Cell(currentRow, 4).Value = item.booking_no;
                worksheet.Cell(currentRow, 5).Value = item.container_no;
                worksheet.Cell(currentRow, 6).Value = item.truck_license;
                worksheet.Cell(currentRow, 7).Value = this.TrackingStatus(item.tracking_status);

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

    public IActionResult TrackingReportPdfForCustomer(avaDataTableParamModel iFilter)
    {
        var customer = this.GetLoggedCode;
        var start = iFilter.filterDateRangeStart;
        var end = iFilter.filterDateRangeEnd;
        List<JobViewModel> query = new List<JobViewModel>();
        using (var connection = new SqlConnection(connStr))
        {
            connection.Open();
            var sql = "SELECT tbl_job_master.job_code, " +
            "CASE WHEN tbl_job_detail.job_detail_code IS NULL THEN '-' ELSE tbl_job_detail.job_detail_code END AS job_detail_code, " +
            "CONVERT(varchar, tbl_job_master.job_date, 105) AS job_date, tbl_place.place_name As mission_name, tbl_job_master.booking_no, " +
            "CASE WHEN tbl_job_detail.job_detail_code IS NULL THEN '-' ELSE tbl_job_detail.container_no END AS container_no,  " +
            "CASE WHEN tbl_job_detail.job_detail_code IS NULL THEN '-' ELSE tbl_truck.truck_license END AS truck_license, " +
            "CASE WHEN tbl_job_detail.job_detail_code IS NULL THEN '-' ELSE tbl_job_detail.tracking_status END AS tracking_status, " +
            "tbl_job_master.job_type  " +
            "FROM tbl_job_master INNER JOIN " +
            "tbl_job_place ON tbl_job_master.job_code = tbl_job_place.job_code INNER JOIN " +
            "tbl_place ON tbl_job_place.mission_code = tbl_place.place_code INNER JOIN " +
            "tbl_customer ON tbl_job_master.customer_code = tbl_customer.customer_code LEFT OUTER JOIN " +
            "tbl_truck INNER JOIN " +
            "tbl_job_detail ON tbl_truck.truck_code = tbl_job_detail.truck_code ON tbl_job_master.job_code = tbl_job_detail.job_code " +
            "WHERE (tbl_job_detail.is_enabled IS NULL OR  " +
            "tbl_job_detail.is_enabled = 1) AND (tbl_job_master.job_date BETWEEN @start AND @end) AND (tbl_customer.customer_code = @customer)  " +
            "ORDER BY tbl_job_master.job_date DESC, tbl_job_detail.job_detail_code DESC";

            query = connection.Query<JobViewModel>(sql, new { customer, start, end }).ToList();
        }

        var data = query.Where(w => (iFilter.filterCode == null || w.job_detail_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterMission == null || w.mission_name.Contains(iFilter.filterMission.ToUpper())) &&
        (iFilter.filterBookingNo == null || w.booking_no.Contains(iFilter.filterBookingNo.ToUpper())) &&
        (iFilter.filterStatusCode == "--ทั้งหมด--" || w.tracking_status.Contains(iFilter.filterStatusCode.ToUpper())) &&
        (iFilter.filterLicense == null || w.truck_license.Contains(iFilter.filterLicense.ToUpper())) &&
        (iFilter.filterContainerNo == null || w.container_no.Contains(iFilter.filterContainerNo.ToUpper())));

        var filename = "tracking-" + DateTime.Now.ToString("ddMMyy") + ".pdf";
        return new ViewAsPdf("_TrackingReportPdfForCustomer", data.ToList())
        {
            FileName = filename,
            PageSize = Rotativa.AspNetCore.Options.Size.Legal,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
    }

    public async Task<IActionResult> AllTruckCheckListPdf(DateTime start_date)
    {
        var modelData = await exportRepo.GetAllCheckListByMonth(start_date);

        var filename = "checking-all-" + start_date.ToString("MMMyyyy") + ".pdf";
        return new ViewAsPdf("_AllTruckCheckListPdf", modelData)
        {
            FileName = filename,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
        //return View("_TruckCheckListPdf", modelData);
    }

    public async Task<IActionResult> TruckCheckListPdf(string id)
    {
        var modelData = await exportRepo.GetCheckListValue(id);

        var filename = "checking-" + modelData.checklist_date.ToString("MMMyyyy") + ".pdf";
        return new ViewAsPdf("_TruckCheckListPdf", modelData)
        {
            FileName = filename,
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            PageMargins = { Left = 5, Bottom = 5, Right = 5, Top = 5 },
            CustomSwitches = "--disable-smart-shrinking"
        };
        //return View("_TruckCheckListPdf", modelData);
    }

    public async Task<IActionResult> GetBillingPDFList(avaDataTableParamModel iFilter)
    {
        var query = await billingRepo.GetListAllBillingData(iFilter.month, iFilter.year);
        var data = query.Where(w => ((iFilter.filterCode == null || w.billing_code.Contains(iFilter.filterCode.ToUpper())) &&
        (iFilter.filterCustomer == null || w.customer_name.Contains(iFilter.filterCustomer.ToUpper())) &&
        (iFilter.filterMission == null || w.mission_name.Contains(iFilter.filterMission.ToUpper())))).ToList();

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
            colF.Width = 10;
            colF.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colG = worksheet.Column("G");
            colG.Width = 20;
            colG.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colH = worksheet.Column("H");
            colH.Width = 12;
            colH.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "Date";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 2).Value = "Code";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 3).Value = "Customer";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 4).Value = "Place";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 5).Value = "Amount";
            worksheet.Cell(currentRow, 5).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 6).Value = "Total";
            worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 7).Value = "Type";
            worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 8).Value = "Status";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

            foreach (var item in data)
            {
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = item.billing_date;
                worksheet.Cell(currentRow, 2).Value = item.billing_code;
                worksheet.Cell(currentRow, 3).Value = item.customer_name;
                worksheet.Cell(currentRow, 4).Value = item.mission_name;
                worksheet.Cell(currentRow, 5).Value = item.billing_amount;
                worksheet.Cell(currentRow, 6).Value = item.billing_total;

                if (item.billing_type == "M")
                {
                    worksheet.Cell(currentRow, 7).Value = "รวม";
                }
                else if (item.billing_type == "A")
                {
                    worksheet.Cell(currentRow, 7).Value = "ค่าใช้จ่าย";
                }
                else if (item.billing_type == "T")
                {
                    worksheet.Cell(currentRow, 7).Value = "ค่าขนส่ง";
                }
                else if (item.billing_type == "O")
                {
                    worksheet.Cell(currentRow, 7).Value = "อื่น ๆ";
                }


                if (item.is_clearing)
                {
                    worksheet.Cell(currentRow, 8).Value = "Complete";
                }
                else
                {
                    worksheet.Cell(currentRow, 8).Value = "Pending";
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

    public async Task<IActionResult> All(string code)
    {
        var model = new BillingPdfFormViewModel();
        var list = await frmRepo.GetBilling(code);
        var data = await frmRepo.GetBillingDataByCode(code);
        var customer = await frmRepo.GetCustomerByCode(data.CustomerCode);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("list");
            var colA = worksheet.Column("A");
            colA.Width = 7;
            colA.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colB = worksheet.Column("B");
            colB.Width = 15;
            colB.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colC = worksheet.Column("C");
            colC.Width = 13;
            colC.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colD = worksheet.Column("D");
            colD.Width = 15;
            colD.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colE = worksheet.Column("E");
            colE.Width = 20;
            colE.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colF = worksheet.Column("F");
            colF.Width = 10;
            colF.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var colG = worksheet.Column("G");
            colG.Width = 15;
            colG.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colH = worksheet.Column("H");
            colH.Width = 15;
            colH.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colI = worksheet.Column("I");
            colI.Width = 15;
            colI.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var colJ = worksheet.Column("J");
            colI.Width = 15;
            colI.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "No.";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 2).Value = "Date";
            worksheet.Cell(currentRow, 2).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 3).Value = "Job No";
            worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 4).Value = "Container";
            worksheet.Cell(currentRow, 4).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 5).Value = "Type";
            worksheet.Cell(currentRow, 5).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 6).Value = "Description";
            worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 7).Value = "Gate";
            worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 8).Value = "Lift On";
            worksheet.Cell(currentRow, 8).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 9).Value = "Lift Off";
            worksheet.Cell(currentRow, 9).Style.Font.Bold = true;

            worksheet.Cell(currentRow, 10).Value = "Transport";
            worksheet.Cell(currentRow, 10).Style.Font.Bold = true;

            int i = 0;
            foreach (var item in list)
            {
                currentRow++;
                i++;
                worksheet.Cell(currentRow, 1).Value = i.ToString();
                worksheet.Cell(currentRow, 2).Value = item.job_date;
                if (customer.DocType == "JN")
                {
                    worksheet.Cell(currentRow, 3).Value = item.job_invoice;
                }
                else
                {
                    worksheet.Cell(currentRow, 3).Value = item.booking_no;
                }

                var lifton = item.advance_lifton + item.advance_yard;
                worksheet.Cell(currentRow, 4).Value = item.container_no;
                worksheet.Cell(currentRow, 5).Value = item.container_size;
                worksheet.Cell(currentRow, 6).Value = item.mission_name;
                worksheet.Cell(currentRow, 7).Value = item.advance_port.ToString("#,##0.00");
                worksheet.Cell(currentRow, 8).Value = lifton.ToString("#,##0.00");
                worksheet.Cell(currentRow, 9).Value = item.advance_liftoff.ToString("#,##0.00");
                worksheet.Cell(currentRow, 10).Value = item.cost_value.ToString("#,##0.00");
            }

            currentRow = currentRow + 3;
            int r = currentRow;
            string noteStart = "A", noteEnd = "D";
            string labelStart = "E", labelEnd = "F";
            string n1 = "G", n2 = "H", n3 = "I", n4 = "J";

            var noteRange = worksheet.Range($"{noteStart}{r}:{noteEnd}{r + 3}");
            noteRange.Merge();
            worksheet.Cell($"{noteStart}{r}").Value = "หมายเหตุ";

            noteRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            noteRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            noteRange.Style.Font.Bold = true;

            // -------------------------
            // 2) Label (G..H) + ตัวเลข (I..L)
            // -------------------------
            void SetLabel(int row, string text)
            {
                var lr = worksheet.Range($"{labelStart}{row}:{labelEnd}{row}");
                lr.Merge();
                lr.Value = text;
                lr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                lr.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                lr.Style.Font.Bold = true;
            }

            SetLabel(r, "รวม");
            SetLabel(r + 1, "ภาษี ณ ที่จ่าย 1%");
            SetLabel(r + 2, "รวมทั้งสิ้น");
            SetLabel(r + 3, "ยอดเงินสุทธิ");

            // ตัวเลขตัวอย่าง (เปลี่ยนเป็นค่าจริง/สูตรได้)
            worksheet.Cell($"{n1}{r}").Value = list.Sum(s => s.advance_port).ToString("#,##0.00");
            worksheet.Cell($"{n2}{r}").Value = list.Sum(s => s.advance_lifton + s.advance_yard).ToString("#,##0.00");
            worksheet.Cell($"{n3}{r}").Value = list.Sum(s => s.advance_liftoff).ToString("#,##0.00");
            worksheet.Cell($"{n4}{r}").Value = list.Sum(s => s.cost_value).ToString("#,##0.00");

            // แถวภาษี ใส่แค่ช่องสุดท้าย (ตามรูป)
            worksheet.Cell($"{n4}{r + 1}").Value = data.BillingVat.ToString("#,##0.00");

            worksheet.Cell($"{n1}{r + 2}").Value = list.Sum(s => s.advance_port).ToString("#,##0.00");
            worksheet.Cell($"{n2}{r + 2}").Value = list.Sum(s => s.advance_lifton + s.advance_yard).ToString("#,##0.00");
            worksheet.Cell($"{n3}{r + 2}").Value = list.Sum(s => s.advance_liftoff).ToString("#,##0.00");
            worksheet.Cell($"{n4}{r + 2}").Value = data.IsVat == true ? data.TransportTotal - data.BillingVat : data.TransportTotal;

            worksheet.Cell($"{n4}{r + 3}").Value = data.BillingNetTotal.ToString("#,##0.00");

            // ฟอร์แมตตัวเลข + ชิดขวา
            var numRange = worksheet.Range($"{n1}{r}:{n4}{r + 3}");
            numRange.Style.NumberFormat.Format = "#,##0.00";
            numRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            numRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // ทำตัวหนาให้ยอดสุทธิ (ช่อง L แถว r+3)
            worksheet.Cell($"{n4}{r + 3}").Style.Font.Bold = true;

            // -------------------------
            // 4) แถวตัวอักษรจำนวนเงิน (A..F) พื้นหลังเทา
            // -------------------------
            var textRange = worksheet.Range($"{noteStart}{r + 3}:{noteEnd}{r + 3}");
            textRange.Merge();
            textRange.Value = this.ChangeCurrencyToText(data.BillingNetTotal);
            textRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            textRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            textRange.Style.Font.Bold = true;
            textRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            var filename = "billing-" + DateTime.Now.ToString("ddMMyyyyHHmm") + ".xlsx";
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

    private string TrackingStatus(string code)
    {
        string status_name = "";
        switch (code)
        {
            case "-":
                status_name = "งานเข้าระบบ";
                break;
            case "PN":
                status_name = "รอ";
                break;
            case "ST":
                status_name = "พขร. รับงาน";
                break;
            case "WP":
                status_name = "รอรับตู้";
                break;
            case "AR":
                status_name = "รอเปิด/บรรจุ";
                break;
            case "FN":
                status_name = "เปิด/บรรจุเรียบร้อย";
                break;
            case "DO":
                status_name = "เสร็จสิ้น";
                break;
            case "BO":
                status_name = "คืนตู้";
                break;
        }

        return status_name;
    }

    private string GetLoggedCode()
    {
        var data = User.Claims.Where(w => w.Type == "code").FirstOrDefault();
        return data.Value;
    }

    private string ChangeCurrencyToText(decimal val)
    {
        string bahtTxt, n, bahtTH = "";
        double amount;
        try { amount = Convert.ToDouble(val); }
        catch { amount = 0; }
        bahtTxt = amount.ToString("####.00");
        string[] num = { "ศูนย์", "หนึ่ง", "สอง", "สาม", "สี่", "ห้า", "หก", "เจ็ด", "แปด", "เก้า", "สิบ" };
        string[] rank = { "", "สิบ", "ร้อย", "พัน", "หมื่น", "แสน", "ล้าน" };
        string[] temp = bahtTxt.Split('.');
        string intVal = temp[0];
        string decVal = temp[1];
        if (Convert.ToDouble(bahtTxt) == 0)
            bahtTH = "ศูนย์บาทถ้วน";
        else
        {
            for (int i = 0; i < intVal.Length; i++)
            {
                n = intVal.Substring(i, 1);
                if (n != "0")
                {
                    if ((i == (intVal.Length - 1)) && (n == "1"))
                        bahtTH += "เอ็ด";
                    else if ((i == (intVal.Length - 2)) && (n == "2"))
                        bahtTH += "ยี่";
                    else if ((i == (intVal.Length - 2)) && (n == "1"))
                        bahtTH += "";
                    else
                        bahtTH += num[Convert.ToInt32(n)];
                    bahtTH += rank[(intVal.Length - i) - 1];
                }
            }
            bahtTH += "บาท";
            if (decVal == "00")
                bahtTH += "ถ้วน";
            else
            {
                for (int i = 0; i < decVal.Length; i++)
                {
                    n = decVal.Substring(i, 1);
                    if (n != "0")
                    {
                        if ((i == decVal.Length - 1) && (n == "1"))
                            bahtTH += "เอ็ด";
                        else if ((i == (decVal.Length - 2)) && (n == "2"))
                            bahtTH += "ยี่";
                        else if ((i == (decVal.Length - 2)) && (n == "1"))
                            bahtTH += "";
                        else
                            bahtTH += num[Convert.ToInt32(n)];
                        bahtTH += rank[(decVal.Length - i) - 1];
                    }
                }
                bahtTH += "สตางค์";
            }
        }
        return bahtTH;
    }
}