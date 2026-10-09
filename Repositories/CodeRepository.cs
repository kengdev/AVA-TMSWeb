using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;

namespace TMSWeb.Repositories;

public class CodeRepository
{
    private static readonly CultureInfo EnUsCulture = new("en-US", false);

    private readonly DbavalogContext db;
    private readonly DbavahrContext dbHr;

    public CodeRepository(DbavalogContext context, DbavahrContext _dbHr)
    {
        db = context;
        dbHr = _dbHr;
    }

    public async Task<string> GetDriverContractCode()
    {
        string code = "";
        string prefix = DateTime.Now.ToString("yyMM") + "02";
        var query = await dbHr.TblContracts.OrderByDescending(o => o.ContractCode).FirstOrDefaultAsync();
        if (query != null)
        {
            string id = query.ContractCode;
            string preinitial = id.Substring(0, 6);
            string initial = id.Substring(6, 4);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(4, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "0001";
            }
        }
        else
        {
            code = prefix + "0001";
        }

        return code;
    }

    public async Task<string> GetEmployeeCode()
    {
        string code = "";
        string prefix_dept = "O";
        string prefix = prefix_dept + DateTime.Now.ToString("yy") + "AVA";
        var query = await dbHr.TblEmployees.OrderByDescending(o => o.CreateDate).FirstOrDefaultAsync();
        if (query != null)
        {
            string id = query.EmpCode;
            string preinitial = id.Substring(0, 6);
            string initial = id.Substring(6, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetSalaryCode()
    {
        string code = "";
        string prefix = "SAL" + DateTime.Now.ToString("yyMM");
        var query = await dbHr.TblEmployeeSalaries.OrderByDescending(o => o.SalaryCode).FirstOrDefaultAsync();
        if (query != null)
        {
            string id = query.SalaryCode;
            string preinitial = id.Substring(0, 7);
            string initial = id.Substring(7, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetDriverCode()
    {
        string code = "";
        string prefix = "DV" + DateTime.Now.ToString("yy") + "I";
        var query = await dbHr.TblEmployeeDrivers.OrderByDescending(o => o.DriverCode).FirstOrDefaultAsync();
        if (query != null)
        {
            string id = query.DriverCode;
            string preinitial = id.Substring(0, 5);
            string initial = id.Substring(5, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetSalaryPaymentCode()
    {
        string code = "";
        string prefix = "AVA" + DateTime.Now.ToString("yyMM") + "S";
        var query = await dbHr.TblPaymentSalaries.OrderByDescending(o => o.PaymentCode).FirstOrDefaultAsync();
        if (query != null)
        {
            string id = query.PaymentCode;
            string preinitial = id.Substring(0, 8);
            string initial = id.Substring(8, 2);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(2, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "01";
            }
        }
        else
        {
            code = prefix + "01";
        }

        return code;
    }

    public async Task<string> GetSalaryPaymentDetailCode()
    {
        string code = "";
        string prefix = DateTime.Now.ToString("yyMM");
        var query = await dbHr.TblPaymentSalaries.OrderByDescending(o => o.PaymentCode).FirstOrDefaultAsync();
        if (query != null)
        {
            string id = query.PaymentCode;
            string preinitial = id.Substring(0, 4);
            string initial = id.Substring(4, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetSupplierCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "SP" + DateTime.Now.ToString("yy");
        var query = await db.TblSuppliers
            .AsNoTracking()
            .OrderByDescending(o => o.SupplierCode)
            .Select(o => o.SupplierCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 4);
            string initial = id.Substring(4, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetQuotationCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "QT" + DateTime.Now.ToString("yy") + "AVA";
        var query = await db.TblQuotations
            .AsNoTracking()
            .OrderByDescending(o => o.QuotationCode)
            .Select(o => o.QuotationCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 7);
            string initial = id.Substring(7, 4);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(4, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "0001";
            }
        }
        else
        {
            code = prefix + "0001";
        }

        return code;
    }

    public async Task<string> GetBudgetTransactionCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = DateTime.Now.ToString("yyMM");
        var query = await db.TblBudgetTransactions
            .AsNoTracking()
            .OrderByDescending(o => o.TransactionCode)
            .Select(o => o.TransactionCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 4);
            string initial = id.Substring(4, 4);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(4, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "0001";
            }
        }
        else
        {
            code = prefix + "0001";
        }

        return code;
    }

    /*public string GetRouteCode()
    {
        string code = "";
        string prefix = DateTime.Now.ToString("yyMM");
        var query = db.TblRoutes.OrderByDescending(o => o.RouteCode).FirstOrDefault();
        if (query != null)
        {
            string id = query.RouteCode;
            string preinitial = id.Substring(0, 4);
            string initial = id.Substring(4, 4);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(4, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "0001";
            }
        }
        else
        {
            code = prefix + "0001";
        }

        return code;
    }*/

    public async Task<string> GetJobCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix_type = "AVA";
        string prefix_year = DateTime.Now.ToString("yy");
        string prefix_month = DateTime.Now.ToString("MM");
        string prefix_date = prefix_year + prefix_month;
        string prefix = prefix_type + prefix_date;
        var query = await db.TblCodeGenerates
            .AsNoTracking()
            .Where(w => w.CodeType == "job_master")
            .OrderByDescending(o => o.Id)
            .Select(o => o.CodeName)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 7);
            string initial = id.Substring(7, 5);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(5, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "00001";
            }
        }
        else
        {
            code = prefix + "00001";
        }

        await SaveGeneratedCodeAsync(
            code,
            "job_master",
            cancellationToken);
        return code;
    }

    public async Task<string> GetDailyPaymentCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix_type = "AVA";
        string prefix_year = DateTime.Now.ToString("yy");
        string prefix_month = DateTime.Now.ToString("MM");
        string prefix_date = prefix_year + prefix_month + "D";
        string prefix = prefix_type + prefix_date;
        var query = await db.TblDailypayments
            .AsNoTracking()
            .OrderByDescending(o => o.DailypaymentCode)
            .Select(o => o.DailypaymentCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 8);
            string initial = id.Substring(8, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetMonthlyPaymentCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix_type = "AVA";
        string prefix_year = DateTime.Now.ToString("yy");
        string prefix_month = DateTime.Now.ToString("MM");
        string prefix_date = prefix_year + prefix_month + "M";
        string prefix = prefix_type + prefix_date;
        var query = await db.TblMonthlypayments
            .AsNoTracking()
            .OrderByDescending(o => o.MonthlyPaymentCode)
            .Select(o => o.MonthlyPaymentCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 8);
            string initial = id.Substring(8, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetReceivePaymentCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix_type = "AVA";
        string prefix_year = DateTime.Now.ToString("yy");
        string prefix_month = DateTime.Now.ToString("MM");
        string prefix_date = prefix_year + prefix_month + "I";
        string prefix = prefix_type + prefix_date;
        var query = await db.TblReceivePayments
            .AsNoTracking()
            .OrderByDescending(o => o.ReceivePaymentCode)
            .Select(o => o.ReceivePaymentCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 8);
            string initial = id.Substring(8, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetJobDetailCode(
        string customer_acronym,
        CancellationToken cancellationToken = default)
    {
        var codes = await GetJobDetailCodes(
            customer_acronym,
            1,
            cancellationToken);

        return codes[0];
    }

    /// <summary>
    /// สร้าง JobDetailCode หลายเลขในครั้งเดียว
    /// เพื่อลด SELECT + SaveChangesAsync ซ้ำใน loop
    /// โดยยังคงรูปแบบเลขและ running number แบบเดิม
    /// </summary>
    public async Task<List<string>> GetJobDetailCodes(
        string customer_acronym,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return new List<string>();
        }

        var now = DateTime.Now;
        var prefixYear = now.ToString("yy");
        var prefixMonth = now.ToString("MM");
        var prefix = customer_acronym + prefixYear + "/" + prefixMonth + "-";

        var latestCode = await db.TblCodeGenerates
            .AsNoTracking()
            .Where(w => w.CodeType == "job_detail")
            .OrderByDescending(o => o.Id)
            .Select(o => o.CodeName)
            .FirstOrDefaultAsync(cancellationToken);

        var runningNumber = 0;

        if (!string.IsNullOrEmpty(latestCode))
        {
            var yearInitial = latestCode.Substring(3, 2);

            if (prefixYear == yearInitial)
            {
                runningNumber = int.Parse(
                    latestCode.Substring(9, 5));
            }
        }

        var codes = new List<string>(count);
        var generatedRows = new List<TblCodeGenerate>(count);

        for (var i = 1; i <= count; i++)
        {
            var nextNumber = runningNumber + i;
            var code = prefix + nextNumber.ToString().PadLeft(5, '0');

            codes.Add(code);

            generatedRows.Add(
                new TblCodeGenerate
                {
                    CodeName = code,
                    CodeType = "job_detail",
                    CreateDate = now
                });
        }

        db.TblCodeGenerates.AddRange(generatedRows);
        await db.SaveChangesAsync(cancellationToken);

        return codes;
    }

    public async Task<string> GetCustomerCode(string customer_type_code, CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "C" + DateTime.Now.ToString("yy");
        string suffix = "T" + customer_type_code;
        var query = await db.TblCustomers
            .AsNoTracking()
            .Where(w => w.CustomerTypeCode == customer_type_code)
            .OrderByDescending(o => o.CustomerCode)
            .Select(o => o.CustomerCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 3);
            string initial = id.Substring(3, 3);
            string postinitial = id.Substring(6, 3);

            if (prefix + suffix == preinitial + postinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id + suffix;
            }
            else
            {
                code = prefix + "001" + suffix;
            }
        }
        else
        {
            code = prefix + "001" + suffix;
        }

        return code;
    }

    public async Task<string> GetPlaceCode(string sy, CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = sy + "-";
        string suffix = DateTime.Now.ToString("yy");
        var query = await db.TblIndustries
            .AsNoTracking()
            .Where(w => w.ProvinceCode == sy)
            .OrderByDescending(o => o.IndustryCode)
            .Select(o => o.IndustryCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 4);
            string postinitial = id.Substring(4, 2);
            string initial = id.Substring(6, 3);

            if (suffix == postinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + suffix + id;
            }
            else
            {
                code = prefix + suffix + "001";
            }
        }
        else
        {
            code = prefix + suffix + "001";
        }

        return code;
    }

    public async Task<string> GetJobAdvanceCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix_year = DateTime.Now.ToString("yy");
        string prefix_month = DateTime.Now.ToString("MM");

        var query = await db.TblCodeGenerates
            .AsNoTracking()
            .Where(w => w.CodeType == "job_advance")
            .OrderByDescending(o => o.CodeName)
            .Select(o => o.CodeName)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string id = query;
            string yearinitial = id.Substring(3, 2);
            string initial = id.Substring(9, 5);
            if (prefix_year == yearinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(5, '0');

                code = "AVA" + prefix_year + prefix_month + "A-" + id;
            }
            else
            {
                code = "AVA" + prefix_year + prefix_month + "A-" + "00001";
            }
        }
        else
        {
            code = "AVA" + prefix_year + prefix_month + "A-" + "00001";
        }

        await SaveGeneratedCodeAsync(
            code,
            "job_advance",
            cancellationToken);

        return code;
    }

    public async Task<string> GetJobCashingCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix_year = DateTime.Now.ToString("yy");
        string prefix_month = DateTime.Now.ToString("MM");

        var query = await db.TblCodeGenerates
            .AsNoTracking()
            .Where(w => w.CodeType == "job_cashing")
            .OrderByDescending(o => o.CodeName)
            .Select(o => o.CodeName)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string id = query;
            string yearinitial = id.Substring(3, 2);
            string initial = id.Substring(9, 5);
            if (prefix_year == yearinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(5, '0');

                code = "AVA" + prefix_year + prefix_month + "C-" + id;
            }
            else
            {
                code = "AVA" + prefix_year + prefix_month + "C-" + "00001";
            }
        }
        else
        {
            code = "AVA" + prefix_year + prefix_month + "C-" + "00001";
        }

        await SaveGeneratedCodeAsync(
            code,
            "job_cashing",
            cancellationToken);

        return code;
    }

    public async Task<string> GetTruckCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "T" + DateTime.Now.ToString("yy");
        var query = await db.TblTrucks
            .AsNoTracking()
            .OrderByDescending(o => o.TruckCode)
            .Select(o => o.TruckCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 3);
            string initial = id.Substring(3, 4);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(4, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "0001";
            }
        }
        else
        {
            code = prefix + "0001";
        }

        return code;
    }

    public async Task<string> GetTrailerCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "R" + DateTime.Now.ToString("yy");
        var query = await db.TblTrailers
            .AsNoTracking()
            .OrderByDescending(o => o.TrailerCode)
            .Select(o => o.TrailerCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 3);
            string initial = id.Substring(3, 4);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(4, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "0001";
            }
        }
        else
        {
            code = prefix + "0001";
        }

        return code;
    }

    public async Task<string> GetCostCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "COS" + DateTime.Now.ToString("yy");
        var query = await db.TblCustomerCosts
            .AsNoTracking()
            .OrderByDescending(o => o.CostCode)
            .Select(o => o.CostCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 5);
            string initial = id.Substring(5, 4);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(4, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "0001";
            }
        }
        else
        {
            code = prefix + "0001";
        }

        return code;
    }

    public async Task<string> GetBillingCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "AVA" + DateTime.Now.ToString("yy") + "/";
        string prefix_month = DateTime.Now.ToString("MM");
        var query = await db.TblCodeGenerates
            .AsNoTracking()
            .Where(w => w.CodeType == "billing")
            .OrderByDescending(o => o.CodeName)
            .Select(o => o.CodeName)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string billing_code = query;
            string prefix_db = billing_code.Substring(0, 6);
            string initial = billing_code.Substring(9, 5);

            if (prefix == prefix_db)
            {
                int id = (int.Parse(initial) + 1);
                initial = id.ToString().PadLeft(5, '0');
                code = prefix + prefix_month + "-" + initial;
            }
            else
            {
                code = prefix + prefix_month + "-00001";
            }
        }
        else
        {
            code = prefix + prefix_month + "-00001";
        }

        await SaveGeneratedCodeAsync(
            code,
            "billing",
            cancellationToken);

        return code;
    }

    public async Task<string> GetSupplyerPaymentCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "AVA" + DateTime.Now.ToString("yy") + "/" + DateTime.Now.ToString("MM") + "SP";
        var query = await db.TblSupplierpayments
            .AsNoTracking()
            .OrderByDescending(o => o.SupplierpaymentCode)
            .Select(o => o.SupplierpaymentCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string billing_code = query;
            string prefix_db = billing_code.Substring(0, 10);
            string initial = billing_code.Substring(10, 4);

            if (prefix == prefix_db)
            {
                int id = (int.Parse(initial) + 1);
                initial = id.ToString().PadLeft(4, '0');
                code = prefix + initial;
            }
            else
            {
                code = prefix + "0001";
            }
        }
        else
        {
            code = prefix + "0001";
        }

        return code;
    }

    public async Task<string> GetFleetPaymentCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "AVA" + DateTime.Now.ToString("yy") + "/" + DateTime.Now.ToString("MM") + "FL";
        var query = await db.TblFleetpayments
            .AsNoTracking()
            .OrderByDescending(o => o.FleetpaymentCode)
            .Select(o => o.FleetpaymentCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string billing_code = query;
            string prefix_db = billing_code.Substring(0, 10);
            string initial = billing_code.Substring(10, 4);

            if (prefix == prefix_db)
            {
                int id = (int.Parse(initial) + 1);
                initial = id.ToString().PadLeft(4, '0');
                code = prefix + initial;
            }
            else
            {
                code = prefix + "0001";
            }
        }
        else
        {
            code = prefix + "0001";
        }

        return code;
    }

    public async Task<string> GetCoverSheetCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "AVA/Cov" + DateTime.Now.ToString("yy");
        var query = await db.TblCoversheets
            .AsNoTracking()
            .OrderByDescending(o => o.CreateDate)
            .Select(o => o.CoversheetCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string billing_code = query;
            string prefix_db = billing_code.Substring(0, 9);
            string initial = billing_code.Substring(10, 5);

            if (prefix == prefix_db)
            {
                int id = (int.Parse(initial) + 1);
                initial = id.ToString().PadLeft(5, '0');
                code = prefix + "-" + initial;
            }
            else
            {
                code = prefix + "-00001";
            }
        }
        else
        {
            code = prefix + "-00001";
        }

        return code;
    }

    public async Task<string> GetReceiptCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix_type = "AVA";
        string prefix_year = DateTime.Now.ToString("yy");
        string prefix = prefix_type + prefix_year;
        var query = await db.TblReceipts
            .AsNoTracking()
            .OrderByDescending(o => o.ReceiptCode)
            .Select(o => o.ReceiptCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 5);
            string initial = id.Substring(5, 5);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(5, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "00001";
            }
        }
        else
        {
            code = prefix + "00001";
        }

        return code;
    }

    public async Task<string> GetOtherReceipt(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix_type = "R";
        string prefix_year = DateTime.Now.ToString("yy/MM-");
        string prefix = prefix_type + prefix_year;
        var query = await db.TblReceiptOthers
            .AsNoTracking()
            .OrderByDescending(o => o.ReceiptOtherCode)
            .Select(o => o.ReceiptOtherCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 7);
            string initial = id.Substring(7, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetPayeeReceipt(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix_type = "A";
        string prefix_year = DateTime.Now.ToString("yyMM");
        string prefix = prefix_type + prefix_year + "RE";
        var query = await db.TblReceivePaymentReceipts
            .AsNoTracking()
            .OrderByDescending(o => o.ReceiptCode)
            .Select(o => o.ReceiptCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 7);
            string initial = id.Substring(7, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetAllownceCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = DateTime.Now.ToString("yyyyMMdd");
        var query = await db.TblCodeGenerates
            .AsNoTracking()
            .Where(w => w.CodeType == "allowance")
            .OrderByDescending(o => o.CreateDate)
            .Select(o => o.CodeName)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 8);
            string initial = id.Substring(8, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        await SaveGeneratedCodeAsync(
            code,
            "allowance",
            cancellationToken);

        return code;
    }

    public async Task<string> GetZoneCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = DateTime.Now.ToString("yyMM");
        var query = await db.TblZones
            .AsNoTracking()
            .OrderByDescending(o => o.ZoneCode)
            .Select(o => o.ZoneCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 4);
            string initial = id.Substring(4, 2);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(2, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "01";
            }
        }
        else
        {
            code = prefix + "01";
        }

        return code;

    }

    public async Task<string> GetPortCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "POR" + DateTime.Now.ToString("yy") + "-";
        var query = await db.TblPlaces
            .AsNoTracking()
            .Where(w => w.PlaceType == "PORT")
            .OrderByDescending(o => o.PlaceCode)
            .Select(o => o.PlaceCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 6);
            string initial = id.Substring(6, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetYardCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "YRD" + DateTime.Now.ToString("yy") + "-";
        var query = await db.TblPlaces
            .AsNoTracking()
            .Where(w => w.PlaceType == "YARD")
            .OrderByDescending(o => o.PlaceCode)
            .Select(o => o.PlaceCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 6);
            string initial = id.Substring(6, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }

    public async Task<string> GetPaymentCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "P" + DateTime.Now.ToString("yyMM", EnUsCulture);
        var query = await db.TblCodeGenerates
            .AsNoTracking()
            .Where(w => w.CodeType == "payment")
            .OrderByDescending(o => o.CodeName)
            .Select(o => o.CodeName)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 5);
            string initial = id.Substring(5, 3);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(3, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        await SaveGeneratedCodeAsync(
            code,
            "payment",
            cancellationToken);

        return code;
    }

    public async Task<string> GetAgentCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = DateTime.Now.ToString("yy");
        var query = await db.TblAgents
            .AsNoTracking()
            .OrderByDescending(o => o.AgentCode)
            .Select(o => o.AgentCode)
            .FirstOrDefaultAsync(cancellationToken);

        if (query != null)
        {
            string id = query;
            string preinitial = id.Substring(0, 2);
            string initial = id.Substring(2, 2);
            if (prefix == preinitial)
            {
                id = (int.Parse(initial) + 1).ToString();
                id = id.PadLeft(2, '0');

                code = prefix + id;
            }
            else
            {
                code = prefix + "01";
            }
        }
        else
        {
            code = prefix + "01";
        }

        return code;
    }

    public async Task<string> GetTaxPaymentCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = "AVA" + DateTime.Now.ToString("yy") + "/" + DateTime.Now.ToString("MM") + "SP";
        var query = await db.TblWithholdingTaxes
            .AsNoTracking()
            .OrderByDescending(o => o.WithholdingTaxCode)
            .Select(o => o.WithholdingTaxCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string billing_code = query;
            string prefix_db = billing_code.Substring(0, 10);
            string initial = billing_code.Substring(10, 4);

            if (prefix == prefix_db)
            {
                int id = (int.Parse(initial) + 1);
                initial = id.ToString().PadLeft(4, '0');
                code = prefix + initial;
            }
            else
            {
                code = prefix + "0001";
            }
        }
        else
        {
            code = prefix + "0001";
        }

        return code;
    }

    public async Task<string> GetShippingPaymentCode(CancellationToken cancellationToken = default)
    {
        string code = "";
        string prefix = DateTime.Now.ToString("yyMM");
        var query = await db.TblShippingpayments
            .AsNoTracking()
            .OrderByDescending(o => o.CreateDate)
            .Select(o => o.PaymentCode)
            .FirstOrDefaultAsync(cancellationToken);
        if (query != null)
        {
            string billing_code = query;
            string prefix_db = billing_code.Substring(0, 4);
            string initial = billing_code.Substring(4, 3);

            if (prefix == prefix_db)
            {
                int id = (int.Parse(initial) + 1);
                initial = id.ToString().PadLeft(3, '0');
                code = prefix + initial;
            }
            else
            {
                code = prefix + "001";
            }
        }
        else
        {
            code = prefix + "001";
        }

        return code;
    }
    private async Task SaveGeneratedCodeAsync(
        string codeName,
        string codeType,
        CancellationToken cancellationToken)
    {
        db.TblCodeGenerates.Add(
            new TblCodeGenerate
            {
                CodeName = codeName,
                CodeType = codeType,
                CreateDate = DateTime.Now
            });

        await db.SaveChangesAsync(cancellationToken);
    }

}