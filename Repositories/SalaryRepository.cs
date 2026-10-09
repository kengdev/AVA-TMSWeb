using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;

namespace TMSWeb.Repositories;

public class SalaryRepository
{
    private readonly DbavahrContext dbHr;
    private readonly DbavalogContext dbLog;
    private readonly ILogger<SalaryRepository> logger;

    public SalaryRepository(
        DbavahrContext contextHr,
        DbavalogContext contextLog,
        ILogger<SalaryRepository> _logger)
    {
        dbHr = contextHr;
        dbLog = contextLog;
        logger = _logger;
    }

    public async Task<List<EmployeeViewModel>> GetTblDriverListActive(
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in dbHr.TblEmployees.AsNoTracking()
            join b in dbHr.TblEmployeeDrivers.AsNoTracking()
                on a.EmpCode equals b.EmpCode
            where a.IsEnabled == true &&
                  b.IsEnabled == true
            select new EmployeeViewModel
            {
                emp_code = a.EmpCode,
                emp_fullname = a.EmpFullname,
                driver_code = b.DriverCode
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmployeeViewModel>> GetTblOfficeListActive(
        CancellationToken cancellationToken = default)
    {
        return await (
            from a in dbHr.TblEmployees.AsNoTracking()
            join b in dbHr.TblEmployeeDepartments.AsNoTracking()
                on a.EmpCode equals b.EmpCode
            where a.IsEnabled == true &&
                  b.DeptCode != "01"
            select new EmployeeViewModel
            {
                emp_code = a.EmpCode,
                emp_fullname = a.EmpFullname
            })
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<TblPaymentSalary?> GetTblPaymentSalary(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await dbHr.TblPaymentSalaries
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.PaymentCode == code,
                cancellationToken);
    }

    public async Task<List<TblAllowance>> GetPaymentTripByDriver(
        string driver_code,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(driver_code) ||
            end < start)
        {
            return new List<TblAllowance>();
        }

        return await dbLog.TblAllowances
            .AsNoTracking()
            .Where(w =>
                w.IsEnabled == true &&
                w.DriverCode == driver_code &&
                w.StartDate >= start &&
                w.StartDate <= end)
            .OrderBy(o => o.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TblEmployeeDriver?> GetTblEmployeeDriver(
        string emp_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(emp_code))
        {
            return null;
        }

        return await dbHr.TblEmployeeDrivers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.EmpCode == emp_code,
                cancellationToken);
    }

    public async Task<decimal> GetEmployeeSalaryByEmp(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return 0;
        }

        return await dbHr.TblEmployeeSalaries
            .AsNoTracking()
            .Where(w =>
                w.EmpCode == code &&
                w.IsEnabled == true)
            .Select(s => s.SalaryValue)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TblEmployeeFund?> GetFundByEmp(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await dbHr.TblEmployeeFunds
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.EmpCode == code &&
                     w.IsEnabled == true,
                cancellationToken);
    }

    public async Task<ResponseViewModel> AddSalary(
        TblPaymentSalary model,
        CancellationToken cancellationToken = default)
    {
        if (model == null)
        {
            return Failure("ไม่พบข้อมูลเงินเดือน");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(model.PaymentCode))
            {
                var exists = await dbHr.TblPaymentSalaries
                    .AsNoTracking()
                    .AnyAsync(
                        w => w.PaymentCode == model.PaymentCode,
                        cancellationToken);

                if (exists)
                {
                    return Failure(
                        $"รหัสเงินเดือน {model.PaymentCode} มีอยู่ในระบบแล้ว");
                }
            }

            dbHr.TblPaymentSalaries.Add(model);
            await dbHr.SaveChangesAsync(cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "AddSalary",
                model.PaymentCode);
        }
    }

    public async Task<List<SalaryListViewModel>> GetDriverSalaryListByYear(
        string year,
        CancellationToken cancellationToken = default)
    {
        var salaryQuery = dbHr.TblPaymentSalaries
            .AsNoTracking()
            .Where(a =>
                a.PaymentYear == year &&
                a.IsEnabled == true &&
                a.PaymentType == "01");

        var detailStats =
            from d in dbHr.TblPaymentSalaryDetails.AsNoTracking()
            join p in salaryQuery
                on d.PaymentCode equals p.PaymentCode
            group d by d.PaymentCode
            into g
            select new
            {
                PaymentCode = g.Key,
                Amount = g.Count(),
                Approved = g.Count(x => x.IsApprove == true)
            };

        var raw = await (
            from a in salaryQuery
            join s in detailStats
                on a.PaymentCode equals s.PaymentCode into stats
            from s in stats.DefaultIfEmpty()
            select new
            {
                a.PaymentCode,
                a.PaymentMonth,
                a.PaymentYear,
                a.PaymentTotal,
                a.PaymentDate,
                a.IsActived,
                Amount = s == null ? 0 : s.Amount,
                Approved = s == null ? 0 : s.Approved
            })
            .ToListAsync(cancellationToken);

        return raw
            .Select(a => new SalaryListViewModel
            {
                payment_code = a.PaymentCode,
                payment_month = a.PaymentMonth,
                payment_year = a.PaymentYear,
                payment_total = a.PaymentTotal,
                payment_date = a.PaymentDate.ToString("dd-MM-yyyy"),
                is_actived = a.IsActived,
                amount = a.Amount,
                approved = a.Approved
            })
            .ToList();
    }

    public async Task<List<SalaryListViewModel>> GetOfficeSalaryListByYear(
        string year,
        CancellationToken cancellationToken = default)
    {
        var salaryQuery = dbHr.TblPaymentSalaries
            .AsNoTracking()
            .Where(a =>
                a.PaymentYear == year &&
                a.IsEnabled == true &&
                a.PaymentType == "02");

        var approvedStats =
            from d in dbHr.TblPaymentSalaryDetails.AsNoTracking()
            join p in salaryQuery
                on d.PaymentCode equals p.PaymentCode
            where d.IsApprove == true
            group d by d.PaymentCode
            into g
            select new
            {
                PaymentCode = g.Key,
                Approved = g.Count()
            };

        var raw = await (
            from a in salaryQuery
            join s in approvedStats
                on a.PaymentCode equals s.PaymentCode into stats
            from s in stats.DefaultIfEmpty()
            select new
            {
                a.PaymentCode,
                a.PaymentMonth,
                a.PaymentYear,
                a.PaymentTotal,
                a.PaymentDate,
                a.IsActived,
                a.PaymentAmount,
                Approved = s == null ? 0 : s.Approved
            })
            .ToListAsync(cancellationToken);

        return raw
            .Select(a => new SalaryListViewModel
            {
                payment_code = a.PaymentCode,
                payment_month = a.PaymentMonth,
                payment_year = a.PaymentYear,
                payment_total = a.PaymentTotal,
                payment_date = a.PaymentDate.ToString("dd-MM-yyyy"),
                is_actived = a.IsActived,
                amount = a.PaymentAmount,
                approved = a.Approved
            })
            .ToList();
    }

    public async Task<List<SalaryItemViewModel>> GetSalaryList(
        string payment_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payment_code))
        {
            return new List<SalaryItemViewModel>();
        }

        var salaryData = await (
            from a in dbHr.TblEmployees.AsNoTracking()
            join b in dbHr.TblPaymentSalaryDetails.AsNoTracking()
                on a.EmpCode equals b.EmpCode
            join d in dbHr.TblPaymentSalaries.AsNoTracking()
                on b.PaymentCode equals d.PaymentCode
            join c in dbHr.TblEmployeeDrivers.AsNoTracking()
                on a.EmpCode equals c.EmpCode
            where d.PaymentCode == payment_code
            orderby a.EmpFullname
            select new SalaryItemViewModel
            {
                payment_code = b.PaymentCode,
                month = d.PaymentMonth,
                year = d.PaymentYear,
                employee_code = a.EmpCode,
                employee_name = a.EmpFullname,
                driver_code = c.DriverCode,
                salary_total = b.PaymentNetTotal,
                status = b.IsApprove
            })
            .ToListAsync(cancellationToken);

        if (salaryData.Count == 0)
        {
            return new List<SalaryItemViewModel>();
        }

        var employeeCodes = salaryData
            .Select(x => x.employee_code)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var driverCodes = salaryData
            .Select(x => x.driver_code)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        /*
         * departmentTask ใช้ dbHr
         * truckTask ใช้ dbLog
         * เป็นคนละ DbContext จึงรันพร้อมกันได้
         */
        var departmentTask = (
            from a in dbHr.TblDepartments.AsNoTracking()
            join b in dbHr.TblEmployeeDepartments.AsNoTracking()
                on a.DeptCode equals b.DeptCode
            where employeeCodes.Contains(b.EmpCode)
            select new
            {
                b.EmpCode,
                Department = a
            })
            .ToListAsync(cancellationToken);

        var truckTask = (
            from a in dbLog.TblTrucks.AsNoTracking()
            join b in dbLog.TblTruckDrivers.AsNoTracking()
                on a.TruckCode equals b.TruckCode
            join c in dbLog.TblTruckDetails.AsNoTracking()
                on a.TruckCode equals c.TruckCode
            where driverCodes.Contains(b.DriverCode)
            select new
            {
                a.TruckLicense,
                b.DriverCode,
                c.TruckNo
            })
            .ToListAsync(cancellationToken);

        await Task.WhenAll(
            departmentTask,
            truckTask);

        var departmentRows = await departmentTask;
        var truckData = await truckTask;

        var departmentsByEmployee = departmentRows
            .GroupBy(x => x.EmpCode)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Select(x => x.Department)
                    .ToList());

        var truckByDriver = truckData
            .GroupBy(x => x.DriverCode)
            .ToDictionary(
                g => g.Key,
                g => g.First());

        foreach (var item in salaryData)
        {
            item.departments =
                departmentsByEmployee.TryGetValue(
                    item.employee_code,
                    out var departments)
                    ? departments
                    : new List<TblDepartment>();

            if (!string.IsNullOrWhiteSpace(item.driver_code) &&
                truckByDriver.TryGetValue(
                    item.driver_code,
                    out var truck))
            {
                item.truck_license =
                    truck.TruckLicense ?? "-";

                item.truck_no =
                    truck.TruckNo ?? "-";
            }
            else
            {
                item.truck_license = "-";
                item.truck_no = "-";
            }
        }

        return salaryData
            .OrderBy(x => x.truck_no)
            .ThenBy(x => x.employee_name)
            .ToList();
    }

    public async Task<TblEmployeeSalary?> GetSalaryByEmpCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await dbHr.TblEmployeeSalaries
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.EmpCode == code &&
                     w.IsEnabled == true,
                cancellationToken);
    }

    public async Task<EmployeeViewModel?> GetEmployeeByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var employee = await (
            from a in dbHr.ViewEmployeeListActives.AsNoTracking()
            where a.EmpCode == code
            select new
            {
                a.EmpCode,
                a.IdCard,
                a.InitialName,
                a.EmpFullname,
                a.EmpGender,
                a.BirthDate,
                a.EmpAddress,
                a.OriginalAddress,
                a.EmpPhone,
                a.EmpMail,
                a.EmpImg,
                a.StatusName,
                a.IsActived
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (employee == null)
        {
            return null;
        }

        var today = DateTime.Today;
        var age = today.Year -
                  employee.BirthDate.Year;

        if (employee.BirthDate > DateOnly.FromDateTime(today.AddYears(-age)))
        {
            age--;
        }

        return new EmployeeViewModel
        {
            emp_code = employee.EmpCode,
            id_card = employee.IdCard,
            emp_fullname =
                employee.InitialName +
                employee.EmpFullname,
            emp_gender =
                employee.EmpGender == "M"
                    ? "ชาย"
                    : "หญิง",
            birth_date = employee.BirthDate,
            emp_current_address =
                employee.EmpAddress,
            emp_original_address =
                employee.OriginalAddress,
            emp_phone = employee.EmpPhone,
            emp_mail = employee.EmpMail,
            emp_img = employee.EmpImg,
            age = age.ToString(),
            status_name = employee.StatusName,
            is_actived = employee.IsActived
        };
    }

    public async Task<List<TblAllowance>> GetDriverAllowanceByDriver(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new List<TblAllowance>();
        }

        /*
         * Method เดิม hard-code เฉพาะเดือน April 2023
         * แต่ชื่อ method คือ GetDriverAllowanceByDriver
         * จึงคืนรายการของคนขับตาม code ทั้งหมดที่ยังใช้งาน
         * และเรียงวันที่ล่าสุดก่อน
         */
        return await dbLog.TblAllowances
            .AsNoTracking()
            .Where(a =>
                a.DriverCode == code &&
                a.IsEnabled == true)
            .OrderByDescending(a => a.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TblEmployeeDriver?> GetDriverByEmpCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await dbHr.TblEmployeeDrivers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.EmpCode == code,
                cancellationToken);
    }

    public async Task<TblPaymentSalaryDetail?> GetTblPaymentSalaryDetailByCode(
        string payment_code,
        string emp_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payment_code) ||
            string.IsNullOrWhiteSpace(emp_code))
        {
            return null;
        }

        return await dbHr.TblPaymentSalaryDetails
            .AsNoTracking()
            .Include(i => i.EmpCodeNavigation)
            .FirstOrDefaultAsync(
                w =>
                    w.PaymentCode == payment_code &&
                    w.EmpCode == emp_code,
                cancellationToken);
    }

    public async Task<List<TblPaymentSalaryAddition>>
        GetTblPaymentSalaryAdditionsByCode(
            string payment_code,
            string emp_code,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payment_code) ||
            string.IsNullOrWhiteSpace(emp_code))
        {
            return new List<TblPaymentSalaryAddition>();
        }

        return await dbHr.TblPaymentSalaryAdditions
            .AsNoTracking()
            .Where(w =>
                w.PaymentCode == payment_code &&
                w.EmpCode == emp_code)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TblPaymentSalaryDediction>>
        GetTblPaymentSalaryDedictionsByCode(
            string payment_code,
            string emp_code,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payment_code) ||
            string.IsNullOrWhiteSpace(emp_code))
        {
            return new List<TblPaymentSalaryDediction>();
        }

        return await dbHr.TblPaymentSalaryDedictions
            .AsNoTracking()
            .Where(w =>
                w.PaymentCode == payment_code &&
                w.EmpCode == emp_code)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResponseViewModel> EditSalaryItem(
        TblPaymentSalaryDetail detail,
        List<TblPaymentSalaryAddition> addition,
        List<TblPaymentSalaryDediction> dedict,
        CancellationToken cancellationToken = default)
    {
        if (detail == null ||
            string.IsNullOrWhiteSpace(detail.PaymentCode) ||
            string.IsNullOrWhiteSpace(detail.EmpCode))
        {
            return Failure(
                "ไม่พบข้อมูลรายการเงินเดือน");
        }

        addition ??=
            new List<TblPaymentSalaryAddition>();

        dedict ??=
            new List<TblPaymentSalaryDediction>();

        await using var transaction =
            await dbHr.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var model = await dbHr.TblPaymentSalaries
                .FirstOrDefaultAsync(
                    w => w.PaymentCode ==
                         detail.PaymentCode,
                    cancellationToken);

            if (model == null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    $"ไม่พบข้อมูลเงินเดือน {detail.PaymentCode}");
            }

            var oldDetail = await dbHr.TblPaymentSalaryDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w =>
                        w.PaymentCode ==
                        detail.PaymentCode &&
                        w.EmpCode ==
                        detail.EmpCode,
                    cancellationToken);

            await dbHr.TblPaymentSalaryDetails
                .Where(w =>
                    w.PaymentCode ==
                    detail.PaymentCode &&
                    w.EmpCode ==
                    detail.EmpCode)
                .ExecuteDeleteAsync(
                    cancellationToken);

            await dbHr.TblPaymentSalaryAdditions
                .Where(w =>
                    w.PaymentCode ==
                    detail.PaymentCode &&
                    w.EmpCode ==
                    detail.EmpCode)
                .ExecuteDeleteAsync(
                    cancellationToken);

            await dbHr.TblPaymentSalaryDedictions
                .Where(w =>
                    w.PaymentCode ==
                    detail.PaymentCode &&
                    w.EmpCode ==
                    detail.EmpCode)
                .ExecuteDeleteAsync(
                    cancellationToken);

            var modelFund = await dbHr.TblEmployeeFunds
                .FirstOrDefaultAsync(
                    w =>
                        w.EmpCode == detail.EmpCode &&
                        w.IsEnabled == true,
                    cancellationToken);

            if (modelFund != null)
            {
                var oldSecure =
                    oldDetail?.DedictionSecure ?? 0;

                var oldSso =
                    oldDetail?.DedictionSso ?? 0;

                /*
                 * ใช้ delta เพื่อไม่ให้การ Edit รายการเดิม
                 * บวกยอดสะสมเข้า Fund ซ้ำทุกครั้ง
                 */
                modelFund.SecureBalanceValue +=
                    detail.DedictionSecure -
                    oldSecure;

                modelFund.SsoBalanceValue +=
                    detail.DedictionSso -
                    oldSso;
            }

            foreach (var item in addition)
            {
                item.PaymentCode =
                    detail.PaymentCode;

                item.EmpCode =
                    detail.EmpCode;
            }

            foreach (var item in dedict)
            {
                item.PaymentCode =
                    detail.PaymentCode;

                item.EmpCode =
                    detail.EmpCode;
            }

            dbHr.TblPaymentSalaryDetails.Add(
                detail);

            if (addition.Count > 0)
            {
                dbHr.TblPaymentSalaryAdditions.AddRange(
                    addition);
            }

            if (dedict.Count > 0)
            {
                dbHr.TblPaymentSalaryDedictions.AddRange(
                    dedict);
            }

            var otherEmployeeTotal =
                await dbHr.TblPaymentSalaryDetails
                    .AsNoTracking()
                    .Where(w =>
                        w.PaymentCode ==
                        detail.PaymentCode &&
                        w.EmpCode !=
                        detail.EmpCode)
                    .SumAsync(
                        s => s.PaymentNetTotal,
                        cancellationToken);

            model.PaymentTotal =
                otherEmployeeTotal +
                detail.PaymentNetTotal;

            model.UpdateDate = DateTime.Now;

            model.UpdateBy =
                !string.IsNullOrWhiteSpace(
                    detail.UpdateBy)
                    ? detail.UpdateBy
                    : model.UpdateBy;

            await dbHr.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "EditSalaryItem",
                $"{detail.PaymentCode}/{detail.EmpCode}");
        }
    }

    public async Task<TblAllowance?> GetDriverAllowance(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await dbLog.TblAllowances
            .AsNoTracking()
            .FirstOrDefaultAsync(
                w => w.AllowanceCode == code,
                cancellationToken);
    }

    public async Task<ResponseViewModel> RemoveSalaryItem(
        string payment_code,
        string emp_code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payment_code) ||
            string.IsNullOrWhiteSpace(emp_code))
        {
            return Failure(
                "ไม่พบข้อมูลรายการเงินเดือน");
        }

        await using var transaction =
            await dbHr.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var oldDetail =
                await dbHr.TblPaymentSalaryDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        w =>
                            w.PaymentCode ==
                            payment_code &&
                            w.EmpCode ==
                            emp_code,
                        cancellationToken);

            if (oldDetail == null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    "ไม่พบรายการเงินเดือนที่ต้องการลบ");
            }

            var modelFund =
                await dbHr.TblEmployeeFunds
                    .FirstOrDefaultAsync(
                        w =>
                            w.EmpCode == emp_code &&
                            w.IsEnabled == true,
                        cancellationToken);

            if (modelFund != null)
            {
                modelFund.SecureBalanceValue -=
                    oldDetail.DedictionSecure;

                modelFund.SsoBalanceValue -=
                    oldDetail.DedictionSso;
            }

            await dbHr.TblPaymentSalaryAdditions
                .Where(w =>
                    w.PaymentCode == payment_code &&
                    w.EmpCode == emp_code)
                .ExecuteDeleteAsync(
                    cancellationToken);

            await dbHr.TblPaymentSalaryDedictions
                .Where(w =>
                    w.PaymentCode == payment_code &&
                    w.EmpCode == emp_code)
                .ExecuteDeleteAsync(
                    cancellationToken);

            await dbHr.TblPaymentSalaryAttendances
                .Where(w =>
                    w.PaymentCode == payment_code &&
                    w.EmpCode == emp_code)
                .ExecuteDeleteAsync(
                    cancellationToken);

            await dbHr.TblPaymentSalaryDetails
                .Where(w =>
                    w.PaymentCode == payment_code &&
                    w.EmpCode == emp_code)
                .ExecuteDeleteAsync(
                    cancellationToken);

            var model =
                await dbHr.TblPaymentSalaries
                    .FirstOrDefaultAsync(
                        w =>
                            w.PaymentCode ==
                            payment_code,
                        cancellationToken);

            if (model == null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    $"ไม่พบข้อมูลเงินเดือน {payment_code}");
            }

            model.PaymentTotal =
                await dbHr.TblPaymentSalaryDetails
                    .AsNoTracking()
                    .Where(w =>
                        w.PaymentCode ==
                        payment_code)
                    .SumAsync(
                        s => s.PaymentNetTotal,
                        cancellationToken);

            model.UpdateDate =
                DateTime.Now;

            model.UpdateBy =
                username;

            await dbHr.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "RemoveSalaryItem",
                $"{payment_code}/{emp_code}");
        }
    }

    public async Task<ResponseViewModel> RemoveSalaryAll(
        string payment_code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payment_code))
        {
            return Failure(
                "ไม่พบรหัสรายการเงินเดือน");
        }

        await using var transaction =
            await dbHr.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var model =
                await dbHr.TblPaymentSalaries
                    .FirstOrDefaultAsync(
                        w =>
                            w.PaymentCode ==
                            payment_code,
                        cancellationToken);

            if (model == null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Failure(
                    $"ไม่พบข้อมูลเงินเดือน {payment_code}");
            }

            var salaryDetails =
                await dbHr.TblPaymentSalaryDetails
                    .AsNoTracking()
                    .Where(w =>
                        w.PaymentCode ==
                        payment_code)
                    .ToListAsync(
                        cancellationToken);

            var fundDeltas = salaryDetails
                .GroupBy(x => x.EmpCode)
                .Select(g => new
                {
                    EmpCode = g.Key,
                    Secure =
                        g.Sum(
                            x => x.DedictionSecure),
                    Sso =
                        g.Sum(
                            x => x.DedictionSso)
                })
                .ToList();

            var employeeCodes = fundDeltas
                .Select(x => x.EmpCode)
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            if (employeeCodes.Count > 0)
            {
                var funds =
                    await dbHr.TblEmployeeFunds
                        .Where(w =>
                            employeeCodes.Contains(
                                w.EmpCode) &&
                            w.IsEnabled == true)
                        .ToListAsync(
                            cancellationToken);

                var fundByEmployee =
                    funds.ToDictionary(
                        x => x.EmpCode);

                foreach (var delta in fundDeltas)
                {
                    if (fundByEmployee.TryGetValue(
                        delta.EmpCode,
                        out var fund))
                    {
                        fund.SecureBalanceValue -=
                            delta.Secure;

                        fund.SsoBalanceValue -=
                            delta.Sso;
                    }
                }
            }

            await dbHr.TblPaymentSalaryAttendances
                .Where(w =>
                    w.PaymentCode ==
                    payment_code)
                .ExecuteDeleteAsync(
                    cancellationToken);

            await dbHr.TblPaymentSalaryAdditions
                .Where(w =>
                    w.PaymentCode ==
                    payment_code)
                .ExecuteDeleteAsync(
                    cancellationToken);

            await dbHr.TblPaymentSalaryDedictions
                .Where(w =>
                    w.PaymentCode ==
                    payment_code)
                .ExecuteDeleteAsync(
                    cancellationToken);

            await dbHr.TblPaymentSalaryDetails
                .Where(w =>
                    w.PaymentCode ==
                    payment_code)
                .ExecuteDeleteAsync(
                    cancellationToken);

            model.IsActived = false;
            model.IsEnabled = false;
            model.PaymentTotal = 0;
            model.UpdateDate = DateTime.Now;
            model.UpdateBy = username;

            await dbHr.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Success();
        }
        catch (Exception ex)
        {
            await RollbackSafelyAsync(
                transaction,
                cancellationToken);

            return Error(
                ex,
                "RemoveSalaryAll",
                payment_code);
        }
    }

    public async Task<ResponseViewModel> ActiveSalaryAll(
        string payment_code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payment_code))
        {
            return Failure(
                "ไม่พบรหัสรายการเงินเดือน");
        }

        try
        {
            var affected =
                await dbHr.TblPaymentSalaries
                    .Where(w =>
                        w.PaymentCode ==
                        payment_code)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(
                                x => x.IsActived,
                                true)
                            .SetProperty(
                                x => x.IsEnabled,
                                true)
                            .SetProperty(
                                x => x.UpdateDate,
                                DateTime.Now)
                            .SetProperty(
                                x => x.UpdateBy,
                                username),
                        cancellationToken);

            if (affected == 0)
            {
                return Failure(
                    $"ไม่พบข้อมูลเงินเดือน {payment_code}");
            }

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "ActiveSalaryAll",
                payment_code);
        }
    }

    public async Task<ResponseViewModel> InactiveSalaryAll(
        string payment_code,
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payment_code))
        {
            return Failure(
                "ไม่พบรหัสรายการเงินเดือน");
        }

        try
        {
            var affected =
                await dbHr.TblPaymentSalaries
                    .Where(w =>
                        w.PaymentCode ==
                        payment_code)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(
                                x => x.IsActived,
                                false)
                            .SetProperty(
                                x => x.UpdateDate,
                                DateTime.Now)
                            .SetProperty(
                                x => x.UpdateBy,
                                username),
                        cancellationToken);

            if (affected == 0)
            {
                return Failure(
                    $"ไม่พบข้อมูลเงินเดือน {payment_code}");
            }

            return Success();
        }
        catch (Exception ex)
        {
            return Error(
                ex,
                "InactiveSalaryAll",
                payment_code);
        }
    }

    public async Task<AttendanceSummaryViewModel> GetWorkAttendance(
        string payment_code,
        string emp_code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payment_code) ||
            string.IsNullOrWhiteSpace(emp_code))
        {
            return new AttendanceSummaryViewModel();
        }

        var data =
            await dbHr.TblPaymentSalaryDetails
                .AsNoTracking()
                .Where(w =>
                    w.PaymentCode ==
                    payment_code &&
                    w.EmpCode ==
                    emp_code)
                .Select(s => new
                {
                    s.LateTotal,
                    s.AbsentTotal,
                    s.LeaveTotal,

                    SecureBalance =
                        dbHr.TblEmployeeFunds
                            .Where(f =>
                                f.EmpCode ==
                                emp_code &&
                                f.IsEnabled == true)
                            .Select(
                                f => f.SecureBalanceValue)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync(
                    cancellationToken);

        if (data == null)
        {
            return new AttendanceSummaryViewModel();
        }

        return new AttendanceSummaryViewModel
        {
            /*
             * คง business logic เดิมของ total_days ไว้
             * เพราะจาก Repository อย่างเดียวไม่สามารถยืนยันได้ว่า
             * total_days ต้องมาจาก field ใด
             */
            total_days = (int)data.LateTotal,
            late = (int)data.LateTotal,
            absent_days = (int)data.AbsentTotal,
            leave_days = data.LeaveTotal,
            secure_balance = data.SecureBalance
        };
    }

    public async Task<List<TblDepartment>> GetTblDepartmentsList(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new List<TblDepartment>();
        }

        return await (
            from a in dbHr.TblDepartments.AsNoTracking()
            join b in dbHr.TblEmployeeDepartments.AsNoTracking()
                on a.DeptCode equals b.DeptCode
            where b.EmpCode == code
            select a)
            .ToListAsync(cancellationToken);
    }

    private async Task<decimal> GetEmployeeLoanAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return 0;
        }

        return await dbHr.TblLoans
            .AsNoTracking()
            .Where(w =>
                w.EmpCode == code &&
                w.IsEnabled == true)
            .Select(s => s.LoanDedictValue)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string ChangeMonth(
        string month,
        string year)
    {
        var monthName = month switch
        {
            "01" => "January",
            "02" => "February",
            "03" => "March",
            "04" => "April",
            "05" => "May",
            "06" => "June",
            "07" => "July",
            "08" => "August",
            "09" => "September",
            "10" => "October",
            "11" => "November",
            "12" => "December",
            _ => month
        };

        return $"{monthName} {year}";
    }

    private static ResponseViewModel Success()
    {
        return new ResponseViewModel
        {
            result = true,
            resultMessage =
                "บันทึกข้อมูลเรียบร้อยแล้ว"
        };
    }

    private static ResponseViewModel Failure(
        string message)
    {
        return new ResponseViewModel
        {
            result = false,
            resultMessage = message
        };
    }

    private ResponseViewModel Error(
        Exception ex,
        string operation,
        string? entityId = null)
    {
        logger.LogError(
            ex,
            "Error in {Operation}. EntityId: {EntityId}",
            operation,
            entityId);

        return new ResponseViewModel
        {
            result = false,
            resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div>" +
                "<div style='margin-top:-30px'> " +
                "กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>"
        };
    }

    private async Task RollbackSafelyAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        try
        {
            await transaction.RollbackAsync(
                cancellationToken);
        }
        catch (Exception rollbackEx)
        {
            logger.LogError(
                rollbackEx,
                "Error while rolling back Salary transaction.");
        }
    }
}
