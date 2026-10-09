using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMSWeb.Models;
using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class DashboardRepository
{
    private readonly DbavalogContext db;
    private readonly DbavahrContext dbHr;
    private readonly string connStr;

    public DashboardRepository(
        DbavalogContext context,
        DbavahrContext contextHr)
    {
        db = context;
        dbHr = contextHr;

        connStr =
            db.Database.GetConnectionString()
            ?? throw new InvalidOperationException(
                "Database connection string is not configured.");
    }

    public async Task<List<TimeAttendanceViewModel>> GetTimeAttendance(
        CancellationToken cancellationToken = default)
    {
        var currentDate = DateTime.Now.Date;
        var nextDate = currentDate.AddDays(1);

        // ดึงเฉพาะ field ที่ต้องใช้ก่อน แล้วค่อย format URL/status ใน memory
        // เพื่อลดงานแปลง string ใน SQL projection
        var rows = await (
            from a in dbHr.IclockTransactions.AsNoTracking()
            join b in dbHr.IclockTerminals.AsNoTracking()
                on a.TerminalSn equals b.TerminalSn
            join c in dbHr.TblEmployeeIclocks.AsNoTracking()
                on a.EmpCode equals c.IclockCode
            join d in dbHr.TblEmployees.AsNoTracking()
                on c.EmpCode equals d.EmpCode
            where a.PunchTime >= currentDate
                  && a.PunchTime < nextDate
            orderby a.PunchTime descending
            select new
            {
                EmpCode = c.EmpCode,
                EmpName = d.EmpFullname,
                a.PunchTime,
                a.TerminalAlias,
                b.AreaAlias,
                a.TerminalSn,
                IclockCode = a.EmpCode
            })
            .Take(30)
            .ToListAsync(cancellationToken);

        return rows
            .Select(a => new TimeAttendanceViewModel
            {
                emp_code = a.EmpCode,
                emp_name = a.EmpName,
                punchtime = a.PunchTime,
                terminal_alias = a.TerminalAlias,
                area_alias = a.AreaAlias,
                status = a.PunchTime.TimeOfDay > new TimeSpan(8, 0, 0)
                    ? "Late"
                    : "On time",
                emp_img =
                    "https://ava-biotime-image.watchana.net/images/"
                    + a.PunchTime.ToString("MMyyyy")
                    + "/"
                    + a.TerminalSn
                    + "/"
                    + a.PunchTime.ToString("yyyyMMddHHmmss")
                    + "-"
                    + a.IclockCode
                    + ".jpg"
            })
            .ToList();
    }

    public async Task<decimal> GetSumNetValue(
        DateTime current,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var startDate =
                new DateTime(
                    current.Year,
                    current.Month,
                    1);

            var endDate =
                startDate.AddMonths(1);

            return await db.ViewJobSummaries
                .AsNoTracking()
                .Where(w =>
                    w.JobDate >= startDate &&
                    w.JobDate < endDate &&
                    w.JobDetailCode != null)
                .Select(w => w.CostValue)
                .SumAsync(cancellationToken)
                ?? 0m;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public async Task<decimal> GetSumBillingValue(
        DateTime current,
        CancellationToken cancellationToken = default)
    {
        var startDate =
            new DateTime(
                current.Year,
                current.Month,
                1);

        var endDate =
            startDate.AddMonths(1);

        return await db.ViewJobSummaries
            .AsNoTracking()
            .Where(w =>
                w.JobDate >= startDate &&
                w.JobDate < endDate &&
                w.IsBilling == true &&
                w.JobDetailCode != null)
            .Select(w => w.CostValue)
            .SumAsync(cancellationToken)
            ?? 0m;
    }

    public async Task<decimal> GetSumUnBillingValue(
        DateTime current,
        CancellationToken cancellationToken = default)
    {
        var startDate =
            new DateTime(
                current.Year,
                current.Month,
                1);

        var endDate =
            startDate.AddMonths(1);

        return await db.ViewJobSummaries
            .AsNoTracking()
            .Where(w =>
                w.JobDate >= startDate &&
                w.JobDate < endDate &&
                w.IsBilling == false &&
                w.IsApprove == true &&
                w.JobDetailCode != null)
            .Select(w => w.CostValue)
            .SumAsync(cancellationToken)
            ?? 0m;
    }

    public async Task<decimal> GetSumUnCashingValue(
        DateTime current,
        CancellationToken cancellationToken = default)
    {
        var startDate =
            new DateTime(
                current.Year,
                current.Month,
                1);

        var endDate =
            startDate.AddMonths(1);

        return await db.ViewBillingListActives
            .AsNoTracking()
            .Where(w =>
                w.JobDateValue >= startDate &&
                w.JobDateValue < endDate &&
                (w.BillingType == "M" ||
                 w.BillingType == "T") &&
                w.IsClearing == false)
            .Select(w => w.CostValue)
            .SumAsync(cancellationToken)
            ?? 0m;
    }

    public async Task<List<JobViewModel>> GetListUnCompleteJob(
        CancellationToken cancellationToken = default)
    {
        var data =
            await (
                from a in db.TblJobMasters.AsNoTracking()
                join b in db.TblCustomers.AsNoTracking()
                    on a.CustomerCode equals b.CustomerCode
                select new JobViewModel
                {
                    job_code = a.JobCode,
                    customer_name = b.CustomerNameTh,
                    booking_no = a.BookingNo,
                    container_type = a.ContainerType,
                    container_size = a.ContainerSizeName
                })
            .ToListAsync(cancellationToken);

        return data;
    }

    public async Task<List<JobLoadPlaceViewModel>> GetJob(
        string truck_license,
        CancellationToken cancellationToken = default)
    {
        var data =
            await db.ViewJobListActives
                .AsNoTracking()
                .Where(a =>
                    a.TruckLicense == truck_license &&
                    a.TrackingStatus != "BO")
                .Select(a =>
                    new JobLoadPlaceViewModel
                    {
                        job_date = a.JobDate,
                        driver_fullname = a.DriverFullname,
                        truck_license = a.TruckLicense,
                        job_code = a.JobCode,
                        booking_no = a.BookingNo,
                        container_no = a.ContainerNo,
                        tracking_status = a.TrackingStatus
                    })
                .OrderBy(o => o.job_date)
                .ToListAsync(cancellationToken);

        return data;
    }

    public async Task<List<GraphViewModel>> GetJobCostCustomerBySupplier(
        string current_month,
        string current_year,
        CancellationToken cancellationToken = default)
    {
        var monthValue = int.Parse(current_month);
        var yearValue = int.Parse(current_year);
        var startDate = new DateTime(yearValue, monthValue, 1);
        var endDate = startDate.AddMonths(1);

        const string sql = """
            SELECT
                b.id_sum,
                ISNULL(c.total_other, 0) AS other_sum,
                b.name,
                b.code
            FROM
            (
                SELECT
                    SUM(jd.cost_value) AS id_sum,
                    c.customer_name_th AS name,
                    c.customer_code AS code
                FROM tbl_job_detail jd
                INNER JOIN tbl_job_master jm
                    ON jd.job_code = jm.job_code
                INNER JOIN tbl_customer c
                    ON jm.customer_code = c.customer_code
                WHERE
                    jd.is_enabled = '1'
                    AND jm.is_enabled = '1'
                    AND jm.job_date >= @startDate
                    AND jm.job_date < @endDate
                GROUP BY
                    c.customer_code,
                    c.customer_name_th
            ) AS b
            LEFT JOIN
            (
                SELECT
                    c.customer_code,
                    SUM(jao.advance_other_valueup) AS total_other
                FROM tbl_job_detail jd
                INNER JOIN tbl_job_master jm
                    ON jm.job_code = jd.job_code
                INNER JOIN tbl_customer c
                    ON jm.customer_code = c.customer_code
                INNER JOIN tbl_jobadvance ja
                    ON jd.job_detail_code = ja.job_detail_code
                INNER JOIN tbl_jobadvance_other jao
                    ON ja.advance_code = jao.advance_code
                WHERE
                    jd.is_enabled = '1'
                    AND jm.is_enabled = '1'
                    AND jm.job_date >= @startDate
                    AND jm.job_date < @endDate
                GROUP BY
                    c.customer_code
            ) AS c
                ON b.code = c.customer_code
            ORDER BY
                b.id_sum DESC;
            """;

        return await QueryGraphAsync(
            sql,
            new { startDate, endDate },
            cancellationToken);
    }

    public async Task<List<GraphViewModel>> GetYearlyContainerAdvance(
        string current_year,
        CancellationToken cancellationToken = default)
    {
        var yearValue = int.Parse(current_year);
        var startDate = new DateTime(yearValue, 1, 1);
        var endDate = startDate.AddYears(1);

        const string sql = """
            SELECT
                m.month AS month,
                ISNULL(p.cost_value, 0) AS cost_value
            FROM
            (
                SELECT '01' AS month
                UNION SELECT '02'
                UNION SELECT '03'
                UNION SELECT '04'
                UNION SELECT '05'
                UNION SELECT '06'
                UNION SELECT '07'
                UNION SELECT '08'
                UNION SELECT '09'
                UNION SELECT '10'
                UNION SELECT '11'
                UNION SELECT '12'
            ) AS m
            LEFT JOIN
            (
                SELECT
                    SUM(ja.advance_totalup) AS cost_value,
                    MONTH(jm.job_date) AS month_date
                FROM tbl_job_detail jd
                INNER JOIN tbl_job_master jm
                    ON jm.job_code = jd.job_code
                INNER JOIN tbl_jobadvance ja
                    ON jd.job_detail_code = ja.job_detail_code
                WHERE
                    jd.is_enabled = '1'
                    AND jm.is_enabled = '1'
                    AND jm.job_date >= @startDate
                    AND jm.job_date < @endDate
                GROUP BY
                    MONTH(jm.job_date)
            ) AS p
                ON TRY_CONVERT(int, m.month) = p.month_date
            ORDER BY
                TRY_CONVERT(int, m.month);
            """;

        return await QueryGraphAsync(
            sql,
            new { startDate, endDate },
            cancellationToken);
    }

    public async Task<List<GraphViewModel>> GetYearlyContainerOther(
        string current_year,
        CancellationToken cancellationToken = default)
    {
        var yearValue = int.Parse(current_year);
        var startDate = new DateTime(yearValue, 1, 1);
        var endDate = startDate.AddYears(1);

        const string sql = """
            SELECT
                m.month AS month,
                ISNULL(p.cost_value, 0) AS cost_value
            FROM
            (
                SELECT '01' AS month
                UNION SELECT '02'
                UNION SELECT '03'
                UNION SELECT '04'
                UNION SELECT '05'
                UNION SELECT '06'
                UNION SELECT '07'
                UNION SELECT '08'
                UNION SELECT '09'
                UNION SELECT '10'
                UNION SELECT '11'
                UNION SELECT '12'
            ) AS m
            LEFT JOIN
            (
                SELECT
                    SUM(jao.advance_other_valueup) AS cost_value,
                    MONTH(jm.job_date) AS month_date
                FROM tbl_job_detail jd
                INNER JOIN tbl_job_master jm
                    ON jm.job_code = jd.job_code
                INNER JOIN tbl_jobadvance ja
                    ON jd.job_detail_code = ja.job_detail_code
                INNER JOIN tbl_jobadvance_other jao
                    ON ja.advance_code = jao.advance_code
                WHERE
                    jd.is_enabled = '1'
                    AND jm.is_enabled = '1'
                    AND jm.job_date >= @startDate
                    AND jm.job_date < @endDate
                GROUP BY
                    MONTH(jm.job_date)
            ) AS p
                ON TRY_CONVERT(int, m.month) = p.month_date
            ORDER BY
                TRY_CONVERT(int, m.month);
            """;

        return await QueryGraphAsync(
            sql,
            new { startDate, endDate },
            cancellationToken);
    }

    public async Task<List<GraphViewModel>> GetYearlyContainerCost(
        string current_year,
        CancellationToken cancellationToken = default)
    {
        var yearValue = int.Parse(current_year);
        var startDate = new DateTime(yearValue, 1, 1);
        var endDate = startDate.AddYears(1);

        const string sql = """
            SELECT
                m.month AS month,
                ISNULL(p.cost_value, 0) AS cost_value
            FROM
            (
                SELECT '01' AS month
                UNION SELECT '02'
                UNION SELECT '03'
                UNION SELECT '04'
                UNION SELECT '05'
                UNION SELECT '06'
                UNION SELECT '07'
                UNION SELECT '08'
                UNION SELECT '09'
                UNION SELECT '10'
                UNION SELECT '11'
                UNION SELECT '12'
            ) AS m
            LEFT JOIN
            (
                SELECT
                    SUM(cost_value) AS cost_value,
                    MONTH(job_date) AS month_date
                FROM viewJobSummary
                WHERE
                    job_date >= @startDate
                    AND job_date < @endDate
                    AND job_detail_code IS NOT NULL
                GROUP BY
                    MONTH(job_date)
            ) AS p
                ON TRY_CONVERT(int, m.month) = p.month_date
            ORDER BY
                TRY_CONVERT(int, m.month);
            """;

        return await QueryGraphAsync(
            sql,
            new { startDate, endDate },
            cancellationToken);
    }

    public async Task<List<GraphViewModel>> GetMonthlyContainerByPlaceData(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var startDate = new DateTime(now.Year, now.Month, 1);
        var endDate = startDate.AddMonths(1);

        const string sql = """
            SELECT TOP(10)
                COUNT(jd.job_detail_code) AS id_count,
                c.customer_acronym AS name
            FROM tbl_job_detail jd
            INNER JOIN tbl_job_master jm
                ON jd.job_code = jm.job_code
            INNER JOIN tbl_customer c
                ON jm.customer_code = c.customer_code
            WHERE
                jm.job_date >= @startDate
                AND jm.job_date < @endDate
                AND jd.job_detail_code IS NOT NULL
            GROUP BY
                c.customer_acronym
            ORDER BY
                id_count DESC;
            """;

        return await QueryGraphAsync(
            sql,
            new { startDate, endDate },
            cancellationToken);
    }

    public async Task<List<GraphViewModel>> GetYearlyContainerData(
        string current_year,
        CancellationToken cancellationToken = default)
    {
        var yearValue = int.Parse(current_year);
        var startDate = new DateTime(yearValue, 1, 1);
        var endDate = startDate.AddYears(1);

        const string sql = """
            SELECT
                m.month AS month,
                ISNULL(p.id_count, 0) AS id_count
            FROM
            (
                SELECT '01' AS month
                UNION SELECT '02'
                UNION SELECT '03'
                UNION SELECT '04'
                UNION SELECT '05'
                UNION SELECT '06'
                UNION SELECT '07'
                UNION SELECT '08'
                UNION SELECT '09'
                UNION SELECT '10'
                UNION SELECT '11'
                UNION SELECT '12'
            ) AS m
            LEFT JOIN
            (
                SELECT
                    COUNT(job_code) AS id_count,
                    MONTH(job_date) AS month_date
                FROM viewJobSummary
                WHERE
                    job_date >= @startDate
                    AND job_date < @endDate
                    AND job_detail_code IS NOT NULL
                GROUP BY
                    MONTH(job_date)
            ) AS p
                ON TRY_CONVERT(int, m.month) = p.month_date
            ORDER BY
                TRY_CONVERT(int, m.month);
            """;

        return await QueryGraphAsync(
            sql,
            new { startDate, endDate },
            cancellationToken);
    }

    public async Task<List<GraphViewModel>> GetDailyContainerData(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH Calendar AS
            (
                SELECT CAST(DATEFROMPARTS(@year, @month, 1) AS date) AS CalendarDate

                UNION ALL

                SELECT DATEADD(day, 1, CalendarDate)
                FROM Calendar
                WHERE CalendarDate < EOMONTH(DATEFROMPARTS(@year, @month, 1))
            )
            SELECT
                DAY(c.CalendarDate) AS day,
                COUNT(d.container_no) AS id_count
            FROM Calendar c
            LEFT JOIN tbl_job_master m
                ON m.job_date >= c.CalendarDate
                AND m.job_date < DATEADD(day, 1, c.CalendarDate)
            LEFT JOIN tbl_job_detail d
                ON d.job_code = m.job_code
            GROUP BY
                c.CalendarDate
            ORDER BY
                c.CalendarDate
            OPTION(MAXRECURSION 1000);
            """;

        return await QueryGraphAsync(
            sql,
            new { year, month },
            cancellationToken);
    }

    public async Task<List<TruckWorkingViewModel>> GetTruckWorkingData(
        DateTime date_val,
        CancellationToken cancellationToken = default)
    {
        var start = date_val.Date;
        var end = DateTime.Now.Date.AddDays(1);

        const string sql = """
            SELECT
                ISNULL(a.trip, 0) AS trip,
                ISNULL(a.fuel, 0) AS fuel,
                ISNULL(a.expressway, 0) AS expressway,
                d.driver_fullname,
                t.truck_license,
                t.truck_code,
                d.driver_code
            FROM tbl_truck t
            INNER JOIN tbl_truck_driver td
                ON t.truck_code = td.truck_code
            INNER JOIN tbl_driver d
                ON td.driver_code = d.driver_code
            LEFT JOIN
            (
                SELECT
                    dc.driver_code,
                    SUM(dc.trip_value) AS trip,
                    SUM(dc.fuel_value) AS fuel,
                    SUM(dc.expressway_value) AS expressway
                FROM tbl_driver_cost dc
                INNER JOIN tbl_job_detail jd
                    ON dc.job_detail_code = jd.job_detail_code
                INNER JOIN tbl_job_master jm
                    ON jd.job_code = jm.job_code
                WHERE
                    jm.job_date >= @start
                    AND jm.job_date < @end
                GROUP BY
                    dc.driver_code
            ) AS a
                ON td.driver_code = a.driver_code
            WHERE
                d.is_enabled = '1';
            """;

        return await QueryAsync<TruckWorkingViewModel>(
            sql,
            new { start, end },
            cancellationToken);
    }

    public async Task<List<JobLoadPlaceViewModel>> GetTotalDetailData(
        string code,
        string month,
        string year,
        CancellationToken cancellationToken = default)
    {
        var monthValue = int.Parse(month);
        var yearValue = int.Parse(year);
        var startDate = new DateTime(yearValue, monthValue, 1);
        var endDate = startDate.AddMonths(1);

        // โหลดรายการหลักก่อน แล้วโหลด Advance Other ของทุก JobDetail ใน query เดียว
        // แทน collection subquery ที่ซ่อนอยู่ใน projection
        var rows = await (
            from a in db.TblJobMasters.AsNoTracking()
            join b in db.TblJobDetails.AsNoTracking()
                on a.JobCode equals b.JobCode
            join c in db.TblCustomers.AsNoTracking()
                on a.CustomerCode equals c.CustomerCode
            join d in db.TblPlaces.AsNoTracking()
                on b.MissionPlaceCode equals d.PlaceCode
            where a.CustomerCode == code
                  && a.JobDate >= startDate
                  && a.JobDate < endDate
                  && a.IsEnabled == true
                  && b.IsEnabled == true
            orderby a.JobDate, d.PlaceName
            select new
            {
                b.JobDetailCode,
                Item = new JobLoadPlaceViewModel
                {
                    job_date = a.JobDate,
                    job_invoice = a.JobInvoice,
                    mission_name = d.PlaceName,
                    booking_no = a.BookingNo,
                    container_no = b.ContainerNo,
                    customer_code = a.CustomerCode,
                    customer_name = c.CustomerNameTh,
                    trip_cost = b.CostValue,
                    is_billing = b.IsBilling
                }
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
            return new List<JobLoadPlaceViewModel>();

        var jobDetailCodes = rows
            .Select(x => x.JobDetailCode)
            .Distinct()
            .ToList();

        var otherRows = await (
            from q in db.TblJobadvances.AsNoTracking()
            join y in db.TblJobadvanceOthers.AsNoTracking()
                on q.AdvanceCode equals y.AdvanceCode
            where jobDetailCodes.Contains(q.JobDetailCode)
            select new
            {
                q.JobDetailCode,
                Item = y
            })
            .ToListAsync(cancellationToken);

        var otherLookup = otherRows
            .GroupBy(x => x.JobDetailCode)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Item).ToList());

        foreach (var row in rows)
        {
            row.Item.other_list = otherLookup.TryGetValue(
                row.JobDetailCode,
                out var otherList)
                    ? otherList
                    : new List<TblJobadvanceOther>();
        }

        return rows.Select(x => x.Item).ToList();
    }

    public async Task<List<WorkingDetailViewModel>> GetWorkingDetailData(
        string code,
        CancellationToken cancellationToken = default)
    {
        var dayOfWeek = (int)DateTime.Now.DayOfWeek;
        var lastSat = GetLastSaturn(dayOfWeek);

        var start = DateTime.Now.Date.AddDays(lastSat);
        var end = DateTime.Now.Date.AddDays(1);

        return await (
            from a in db.TblJobMasters.AsNoTracking()
            join b in db.TblJobDetails.AsNoTracking()
                on a.JobCode equals b.JobCode
            join c in db.TblDriverCosts.AsNoTracking()
                on b.JobDetailCode equals c.JobDetailCode
            join d in db.TblDrivers.AsNoTracking()
                on b.DriverCode equals d.DriverCode
            join e in db.TblTrucks.AsNoTracking()
                on b.TruckCode equals e.TruckCode
            join f in db.TblPlaces.AsNoTracking()
                on b.MissionPlaceCode equals f.PlaceCode
            where b.DriverCode == code
                  && a.JobDate >= start
                  && a.JobDate < end
            select new WorkingDetailViewModel
            {
                truck_license = e.TruckLicense,
                driver_fullname = d.DriverFullname,
                trip = c.TripValue,
                fuel = c.FuelValue,
                expressway = c.ExpresswayValue,
                job_date = a.JobDate,
                container_no = b.ContainerNo,
                booking_no = a.BookingNo,
                job_invoice = a.JobInvoice,
                place = f.PlaceName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OtherJobViewModel>> GetAdvanceOtherList(
        string code,
        string month,
        string year,
        CancellationToken cancellationToken = default)
    {
        var monthValue = int.Parse(month);
        var yearValue = int.Parse(year);
        var startDate = new DateTime(yearValue, monthValue, 1);
        var endDate = startDate.AddMonths(1);

        const string sql = """
            SELECT
                jao.advance_other AS name,
                jao.advance_other_value AS value,
                jd.container_no,
                p.place_name AS place
            FROM tbl_jobadvance_other jao
            INNER JOIN tbl_jobadvance ja
                ON jao.advance_code = ja.advance_code
            INNER JOIN tbl_job_detail jd
                ON ja.job_detail_code = jd.job_detail_code
            INNER JOIN tbl_job_master jm
                ON jd.job_code = jm.job_code
            INNER JOIN tbl_place p
                ON jd.mission_place_code = p.place_code
            WHERE
                jm.customer_code = @code
                AND jm.job_date >= @startDate
                AND jm.job_date < @endDate;
            """;

        return await QueryAsync<OtherJobViewModel>(
            sql,
            new { code, startDate, endDate },
            cancellationToken);
    }

    private Task<List<GraphViewModel>> QueryGraphAsync(
        string sql,
        object parameters,
        CancellationToken cancellationToken)
    {
        return QueryAsync<GraphViewModel>(
            sql,
            parameters,
            cancellationToken);
    }

    private async Task<List<T>> QueryAsync<T>(
        string sql,
        object parameters,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connStr);
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        var data = await connection.QueryAsync<T>(command);
        return data.ToList();
    }

    private int GetLastSaturn(int val)
    {
        int saturn = 0;

        switch (val)
        {
            case 0:
                saturn = -1;
                break;

            case 1:
                saturn = -2;
                break;

            case 2:
                saturn = -3;
                break;

            case 3:
                saturn = -4;
                break;

            case 4:
                saturn = -5;
                break;

            case 5:
                saturn = -6;
                break;
        }

        return saturn;
    }
}