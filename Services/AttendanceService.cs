using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TMSWeb.Models.Entities.HR;

public sealed class AttendanceExcelService
{
    private readonly DbavahrContext _db;

    public AttendanceExcelService(DbavahrContext db)
    {
        _db = db;
    }

    public async Task<byte[]> ExportAttendanceAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;

        ValidateDateRange(startDate, endDate);

        /*
         * รวมวันที่สิ้นสุดด้วย
         *
         * ตัวอย่าง:
         * endDate = 31/07/2026
         * endDateExclusive = 01/08/2026
         */
        var endDateExclusive = endDate.AddDays(1);

        /*
         * ดึงข้อมูลต่อไปถึงเวลา 05:00 ของวันถัดไป
         *
         * เพราะการสแกนเวลา 00:00 - 04:59
         * จะนับเป็นเวลาของวันทำงานก่อนหน้า
         */
        var transactionEndDate =
            endDateExclusive.AddHours(5);

        var reportDates = Enumerable
            .Range(
                0,
                (endDate - startDate).Days + 1)
            .Select(offset => startDate.AddDays(offset))
            .ToList();

        /*
         * tbl_employee.emp_code
         *          ↓
         * tbl_employee_iclock.emp_code
         *
         * tbl_employee_iclock.iclock_code
         *          ↓
         * iclock_transaction.emp_code
         */
        var employeeMappings = await (
            from employee in _db.TblEmployees.AsNoTracking()

            join mapping in _db.TblEmployeeIclocks.AsNoTracking()
                on employee.EmpCode equals mapping.EmpCode

            where employee.IsActived == true
                  && employee.IsEnabled == true

            select new EmployeeIclockMap
            {
                EmployeeCode = employee.EmpCode,
                InitialName = employee.InitialName,
                EmployeeName = employee.EmpFullname,
                IclockCode = mapping.IclockCode
            })
            .ToListAsync(cancellationToken);

        /*
         * สร้างรายชื่อพนักงาน
         *
         * รองรับกรณีพนักงานหนึ่งคน
         * มี iclock_code มากกว่าหนึ่งรหัส
         */
        var employees = employeeMappings
            .GroupBy(x => x.EmployeeCode)
            .Select(group =>
            {
                var first = group.First();

                return new EmployeeReportRow
                {
                    EmployeeCode = group.Key,
                    InitialName = first.InitialName,
                    EmployeeName = first.EmployeeName
                };
            })
            .OrderBy(x => x.EmployeeCode)
            .ToList();

        /*
         * ใช้ค้นว่า iclock_code นี้
         * เป็นของพนักงานรหัสใด
         */
        var iclockToEmployee = employeeMappings
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.IclockCode))
            .GroupBy(x => x.IclockCode)
            .ToDictionary(
                group => group.Key,
                group => group.First().EmployeeCode);

        var iclockCodes =
            iclockToEmployee.Keys.ToList();

        List<IclockPunchDto> transactions;

        if (iclockCodes.Count == 0)
        {
            transactions = [];
        }
        else
        {
            transactions = await _db.IclockTransactions
                .AsNoTracking()
                .Where(x =>
                    iclockCodes.Contains(x.EmpCode) &&
                    x.PunchTime >= startDate &&
                    x.PunchTime < transactionEndDate)
                .Select(x => new IclockPunchDto
                {
                    IclockCode = x.EmpCode,
                    PunchTime = x.PunchTime
                })
                .ToListAsync(cancellationToken);
        }

        /*
         * เปลี่ยน iclock_code กลับเป็น emp_code
         * และคำนวณวันทำงานของแต่ละรายการ
         */
        var employeePunches = transactions
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.IclockCode) &&
                iclockToEmployee.ContainsKey(x.IclockCode))
            .Select(x => new EmployeePunchDto
            {
                EmployeeCode =
                    iclockToEmployee[x.IclockCode],

                PunchTime = x.PunchTime,

                WorkDate =
                    GetWorkDate(x.PunchTime)
            })
            .Where(x =>
                x.WorkDate >= startDate &&
                x.WorkDate < endDateExclusive)
            .ToList();

        /*
         * สรุปเวลาแรกและเวลาสุดท้าย
         * ของแต่ละคนในแต่ละวัน
         */
        var punchSummary = employeePunches
            .GroupBy(x => new
            {
                x.EmployeeCode,
                WorkDate = x.WorkDate.Date
            })
            .ToDictionary(
                group => (
                    EmployeeCode: group.Key.EmployeeCode,
                    WorkDate: group.Key.WorkDate
                ),
                group => new DailyPunchSummary
                {
                    PunchCount = group.Count(),

                    FirstPunch =
                        group.Min(x => x.PunchTime),

                    LastPunch =
                        group.Max(x => x.PunchTime)
                });

        using var workbook = new XLWorkbook();

        var worksheetName =
            $"Attendance-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}";

        if (worksheetName.Length > 31)
        {
            worksheetName = worksheetName[..31];
        }

        var worksheet =
            workbook.Worksheets.Add(worksheetName);

        BuildHeader(
            worksheet,
            reportDates);

        BuildEmployeeRows(
            worksheet,
            employees,
            punchSummary,
            reportDates);

        ApplyWorksheetStyle(
            worksheet,
            employees.Count,
            reportDates.Count);

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return stream.ToArray();
    }

    private static void ValidateDateRange(
        DateTime startDate,
        DateTime endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException(
                "วันที่สิ้นสุดต้องไม่น้อยกว่าวันที่เริ่มต้น");
        }

        var totalDays =
            (endDate - startDate).Days + 1;

        if (totalDays > 366)
        {
            throw new ArgumentException(
                "กรุณาเลือกช่วงวันที่ไม่เกิน 366 วัน");
        }
    }

    private static DateTime GetWorkDate(
        DateTime punchTime)
    {
        /*
         * รายการก่อน 05:00
         * นับเป็นของวันก่อนหน้า
         */
        var changeDayTime =
            new TimeSpan(5, 0, 0);

        return punchTime.TimeOfDay < changeDayTime
            ? punchTime.Date.AddDays(-1)
            : punchTime.Date;
    }

    private static void BuildHeader(
        IXLWorksheet worksheet,
        IReadOnlyList<DateTime> reportDates)
    {
        worksheet.Cell(1, 1).Value = "รหัส";
        worksheet.Cell(1, 2).Value = "ชื่อ";

        var isSameMonth = reportDates
            .Select(x => new
            {
                x.Year,
                x.Month
            })
            .Distinct()
            .Count() == 1;

        for (var index = 0;
             index < reportDates.Count;
             index++)
        {
            var date = reportDates[index];
            var column = index + 3;
            var cell = worksheet.Cell(1, column);

            /*
             * ถ้าอยู่เดือนเดียวกัน แสดงเฉพาะวันที่
             * ถ้าข้ามเดือน แสดงวันที่/เดือน
             */
            cell.Value = isSameMonth
                ? date.Day.ToString()
                : date.ToString("dd/MM");

            cell.CreateComment()
                .AddText(
                    date.ToString(
                        "dddd dd/MM/yyyy",
                        new CultureInfo("th-TH")));

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                cell.Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#F4CCCC");
            }
        }
    }

    private static void BuildEmployeeRows(
        IXLWorksheet worksheet,
        IReadOnlyList<EmployeeReportRow> employees,
        IReadOnlyDictionary<
            (string EmployeeCode, DateTime WorkDate),
            DailyPunchSummary> punchSummary,
        IReadOnlyList<DateTime> reportDates)
    {
        /*
         * หลัง 08:00 ถือว่าสาย
         */
        var lateAfter =
            new TimeSpan(8, 0, 0);

        var row = 2;

        foreach (var employee in employees)
        {
            worksheet.Cell(row, 1).Value =
                employee.EmployeeCode;

            worksheet.Cell(row, 2).Value =
                employee.EmployeeName;

            /*
             * หากต้องการแสดงคำนำหน้าชื่อด้วย
             * ใช้บรรทัดนี้แทน
             */
            // worksheet.Cell(row, 2).Value =
            //     $"{employee.InitialName} {employee.EmployeeName}".Trim();

            for (var index = 0;
                 index < reportDates.Count;
                 index++)
            {
                var workDate =
                    reportDates[index].Date;

                var column =
                    index + 3;

                var cell =
                    worksheet.Cell(row, column);

                cell.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                cell.Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;

                cell.Style.Alignment.WrapText = true;

                /*
                 * วันอาทิตย์
                 */
                if (workDate.DayOfWeek ==
                    DayOfWeek.Sunday)
                {
                    SetSundayStyle(cell);
                    continue;
                }

                /*
                 * วันที่ในอนาคต
                 */
                if (workDate > DateTime.Today)
                {
                    SetFutureDateStyle(cell);
                    continue;
                }

                var key = (
                    EmployeeCode:
                        employee.EmployeeCode,

                    WorkDate:
                        workDate
                );

                /*
                 * ไม่พบข้อมูลสแกน
                 */
                if (!punchSummary.TryGetValue(
                        key,
                        out var punch))
                {
                    SetNoPunchStyle(cell);
                    continue;
                }

                var hasCheckOut =
                    punch.PunchCount > 1 &&
                    punch.FirstPunch != punch.LastPunch;

                var checkInText =
                    punch.FirstPunch.ToString("HH:mm");

                var checkOutText = hasCheckOut
                    ? FormatCheckOutTime(
                        punch.LastPunch,
                        workDate)
                    : "-";

                cell.Value =
                    $"เข้า {checkInText}\n" +
                    $"ออก {checkOutText}";

                /*
                 * สีของช่องพิจารณาจากเวลาเข้า
                 */
                if (punch.FirstPunch.TimeOfDay > lateAfter)
                {
                    SetLateTimeStyle(cell);
                }
                else
                {
                    SetNormalTimeStyle(cell);
                }

                /*
                 * รายละเอียดเต็มใน Comment
                 */
                var commentText =
                    $"วันที่ทำงาน: {workDate:dd/MM/yyyy}\n" +
                    $"จำนวนครั้งที่สแกน: {punch.PunchCount}\n" +
                    $"เวลาเข้า: {punch.FirstPunch:dd/MM/yyyy HH:mm:ss}\n" +
                    $"เวลาออก: " +
                    (hasCheckOut
                        ? punch.LastPunch
                            .ToString("dd/MM/yyyy HH:mm:ss")
                        : "ไม่มีข้อมูล");

                cell.CreateComment()
                    .AddText(commentText);
            }

            row++;
        }
    }

    private static string FormatCheckOutTime(
        DateTime checkOut,
        DateTime workDate)
    {
        /*
         * ออกงานเป็นวันถัดไป
         * ตัวอย่าง 02:15 (+1)
         */
        if (checkOut.Date > workDate.Date)
        {
            var dayDifference =
                (checkOut.Date - workDate.Date).Days;

            return $"{checkOut:HH:mm} (+{dayDifference})";
        }

        return checkOut.ToString("HH:mm");
    }

    private static void SetNormalTimeStyle(
        IXLCell cell)
    {
        cell.Style.Fill.BackgroundColor =
            XLColor.FromHtml("#D9EAD3");

        cell.Style.Font.FontColor =
            XLColor.FromHtml("#146B3A");

        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 9;
    }

    private static void SetLateTimeStyle(
        IXLCell cell)
    {
        cell.Style.Fill.BackgroundColor =
            XLColor.FromHtml("#FFF2CC");

        cell.Style.Font.FontColor =
            XLColor.FromHtml("#9C6500");

        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 9;
    }

    private static void SetNoPunchStyle(
        IXLCell cell)
    {
        cell.Value =
            "เข้า -\nออก -";

        cell.Style.Fill.BackgroundColor =
            XLColor.FromHtml("#E7E6E6");

        cell.Style.Font.FontColor =
            XLColor.FromHtml("#7F0000");

        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 9;
    }

    private static void SetSundayStyle(
        IXLCell cell)
    {
        cell.Value = string.Empty;

        cell.Style.Fill.BackgroundColor =
            XLColor.FromHtml("#F4CCCC");
    }

    private static void SetFutureDateStyle(
        IXLCell cell)
    {
        cell.Value = string.Empty;

        cell.Style.Fill.BackgroundColor =
            XLColor.FromHtml("#FCE4EC");
    }

    private static void ApplyWorksheetStyle(
        IXLWorksheet worksheet,
        int employeeCount,
        int dateCount)
    {
        var lastRow =
            employeeCount + 1;

        var lastColumn =
            dateCount + 2;

        var allRange = worksheet.Range(
            1,
            1,
            Math.Max(lastRow, 1),
            lastColumn);

        allRange.Style.Font.FontName = "Tahoma";
        allRange.Style.Font.FontSize = 10;

        allRange.Style.Border.TopBorder =
            XLBorderStyleValues.Thin;

        allRange.Style.Border.BottomBorder =
            XLBorderStyleValues.Thin;

        allRange.Style.Border.LeftBorder =
            XLBorderStyleValues.Thin;

        allRange.Style.Border.RightBorder =
            XLBorderStyleValues.Thin;

        allRange.Style.Border.TopBorderColor =
            XLColor.FromHtml("#D0D0D0");

        allRange.Style.Border.BottomBorderColor =
            XLColor.FromHtml("#D0D0D0");

        allRange.Style.Border.LeftBorderColor =
            XLColor.FromHtml("#D0D0D0");

        allRange.Style.Border.RightBorderColor =
            XLColor.FromHtml("#D0D0D0");

        /*
         * หัวตาราง
         */
        var headerRange = worksheet.Range(
            1,
            1,
            1,
            lastColumn);

        headerRange.Style.Font.Bold = true;

        headerRange.Style.Fill.BackgroundColor =
            XLColor.FromHtml("#F2F2F2");

        headerRange.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        headerRange.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        /*
         * ความกว้างคอลัมน์
         */
        worksheet.Column(1).Width = 16;
        worksheet.Column(2).Width = 28;

        for (var column = 3;
             column <= lastColumn;
             column++)
        {
            worksheet.Column(column).Width = 13;
        }

        /*
         * ความสูงแถว
         */
        worksheet.Row(1).Height = 26;

        for (var row = 2;
             row <= lastRow;
             row++)
        {
            worksheet.Row(row).Height = 34;
        }

        /*
         * ล็อกหัวตารางและชื่อพนักงาน
         */
        worksheet.SheetView.FreezeRows(1);
        worksheet.SheetView.FreezeColumns(2);

        /*
         * Filter เฉพาะหัวตาราง
         */
        if (employeeCount > 0)
        {
            worksheet.Range(
                    1,
                    1,
                    lastRow,
                    lastColumn)
                .SetAutoFilter();
        }

        /*
         * ตั้งค่าการพิมพ์
         */
        worksheet.PageSetup.PageOrientation =
            XLPageOrientation.Landscape;

        worksheet.PageSetup.PaperSize =
            XLPaperSize.A4Paper;

        worksheet.PageSetup.FitToPages(
            pagesWide: 1,
            pagesTall: 0);

        worksheet.PageSetup
            .SetRowsToRepeatAtTop(1, 1);

        worksheet.PageSetup
            .SetColumnsToRepeatAtLeft(1, 2);

        worksheet.PageSetup.Margins.Top = 0.3;
        worksheet.PageSetup.Margins.Bottom = 0.3;
        worksheet.PageSetup.Margins.Left = 0.2;
        worksheet.PageSetup.Margins.Right = 0.2;

        /*
         * จัดแนวชื่อและรหัส
         */
        if (employeeCount > 0)
        {
            worksheet.Range(
                    2,
                    1,
                    lastRow,
                    1)
                .Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            worksheet.Range(
                    2,
                    2,
                    lastRow,
                    2)
                .Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
        }
    }

    private sealed class EmployeeIclockMap
    {
        public string EmployeeCode { get; set; } =
            string.Empty;

        public string? InitialName { get; set; }

        public string EmployeeName { get; set; } =
            string.Empty;

        public string IclockCode { get; set; } =
            string.Empty;
    }

    private sealed class EmployeeReportRow
    {
        public string EmployeeCode { get; set; } =
            string.Empty;

        public string? InitialName { get; set; }

        public string EmployeeName { get; set; } =
            string.Empty;
    }

    private sealed class IclockPunchDto
    {
        public string IclockCode { get; set; } =
            string.Empty;

        public DateTime PunchTime { get; set; }
    }

    private sealed class EmployeePunchDto
    {
        public string EmployeeCode { get; set; } =
            string.Empty;

        public DateTime PunchTime { get; set; }

        public DateTime WorkDate { get; set; }
    }

    private sealed class DailyPunchSummary
    {
        public int PunchCount { get; set; }

        public DateTime FirstPunch { get; set; }

        public DateTime LastPunch { get; set; }
    }
}