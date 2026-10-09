using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class ReportRepository
{
    private readonly DbavalogContext db;
    private readonly string connStr;

    public ReportRepository(DbavalogContext context)
    {
        db = context;
        connStr = db.Database.GetConnectionString()
            ?? throw new InvalidOperationException("Database connection string is not configured.");
    }

    public async Task<List<ReportJobViewModel>> GetCostData(
        string year,
        string month)
    {
        var y = int.Parse(year);
        var m = int.Parse(month);

        await using var connection = new SqlConnection(connStr);
        await connection.OpenAsync();

        const string sql = """
            SELECT
                jd.job_detail_code           AS code,
                jm.job_date                  AS job_date_value,
                c.customer_acronym           AS customer_name,
                jm.booking_no,
                jm.job_invoice               AS invoice_no,
                jd.container_no,
                jm.container_size_name       AS container_size,
                p.place_name                 AS mission_name,
                z.zone_name,
                t.truck_license,
                td.truck_no,
                dr.driver_fullname           AS driver_name,
                s.supplier_name,
                jd.cost_value                AS customer_charge,
                sa.cost_reserve              AS supplier_charge,
                ISNULL(bm.billing_code, '-') AS billing_code
            FROM tbl_job_detail jd
            INNER JOIN tbl_job_master jm
                ON jm.job_code = jd.job_code
            INNER JOIN tbl_customer c
                ON c.customer_code = jm.customer_code
            INNER JOIN tbl_place p
                ON p.place_code = jd.mission_place_code
            INNER JOIN tbl_industry i
                ON i.industry_code = p.place_code
            INNER JOIN tbl_zone z
                ON z.zone_code = i.zone_code
            INNER JOIN tbl_supplier s
                ON s.supplier_code = jd.supplier_code
            INNER JOIN tbl_supplier_allowance sa
                ON sa.job_detail_code = jd.job_detail_code
            INNER JOIN tbl_truck t
                ON t.truck_code = jd.truck_code
            INNER JOIN tbl_truck_detail td
                ON td.truck_code = t.truck_code
            INNER JOIN tbl_driver dr
                ON dr.driver_code = jd.driver_code
            LEFT JOIN tbl_billing_detail bd
                ON bd.job_detail_code = jd.job_detail_code
            LEFT JOIN tbl_billing_master bm
                ON bm.billing_code = bd.billing_code
            WHERE
                jm.job_date >= DATEFROMPARTS(@year, @month, 1)
                AND jm.job_date < DATEADD(
                    MONTH,
                    1,
                    DATEFROMPARTS(@year, @month, 1));
            """;

        var data = await connection.QueryAsync<ReportJobViewModel>(
            sql,
            new
            {
                month = m,
                year = y
            });

        return data.ToList();
    }

    public async Task<List<ReportAdvanceViewModel>> GetOtherAdvanceData(
        string jobdetailcode)
    {
        return await (
            from a in db.TblJobadvances.AsNoTracking()
            join b in db.TblJobadvanceOthers.AsNoTracking()
                on a.AdvanceCode equals b.AdvanceCode
            where a.JobDetailCode == jobdetailcode
            select new ReportAdvanceViewModel
            {
                advance_name = b.AdvanceOther,
                supplier = b.AdvanceOtherValue,
                customer = b.AdvanceOtherValueup
            }
        ).ToListAsync();
    }
}
