using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMSWeb.Models;
using TMSWeb.Models.Entities.HR;
using TMSWeb.Models.Entities.LOG;
using TMSWeb.Models.ViewModels;
using TMSWeb.Repositories;

namespace TMSWeb.Repositories;

public class EmployeeRepository
{
    private readonly DbavahrContext dbHr;
    private readonly DbavalogContext dbLog;
    private readonly CodeRepository codeRepo;

    public EmployeeRepository(DbavahrContext contextHr, DbavalogContext contextLog, CodeRepository _codeRepo)
    {
        dbHr = contextHr;
        dbLog = contextLog;
        codeRepo = _codeRepo;
    }

    public async Task<List<EmployeeViewModel>> GetAllOfficer()
    {
        return await (from a in dbHr.ViewEmployeeLists.AsNoTracking()
                      orderby a.IsActived, a.StatusCode, a.EmpFullname
                      select new EmployeeViewModel
                      {
                          emp_code = a.EmpCode,
                          emp_fullname = a.EmpFullname,
                          initial_name = a.InitialName,
                          emp_phone = a.EmpPhone,
                          start_date = a.StartDate.ToString("dd-MM-yyyy"),
                          age = a.Age.Value.ToString(),
                          status_code = a.StatusCode,
                          status_name = a.StatusName,
                          is_actived = a.IsActived
                      }).ToListAsync();
    }

    public async Task<List<EmployeeViewModel>> GetEmployeeListActive()
    {
        var employees = await (from a in dbHr.TblEmployees.AsNoTracking()
                               join b in dbHr.TblEmployeeDetails.AsNoTracking()
                                   on a.EmpCode equals b.EmpCode
                               where a.IsEnabled == true
                               orderby a.EmpFullname
                               select new EmployeeViewModel
                               {
                                   emp_code = a.EmpCode,
                                   emp_fullname = a.EmpFullname,
                                   initial_name = a.InitialName,
                                   emp_phone = b.EmpPhone,
                                   emp_img = b.EmpImg,
                                   start_date = b.StartDate.ToString("dd-MM-yyyy"),
                                   status_code = a.StatusCode,
                                   is_actived = a.IsActived
                               }).ToListAsync();

        if (employees.Count == 0)
            return employees;

        var empCodes = employees.Select(x => x.emp_code).Distinct().ToList();

        // เดิมยิง GetTblDepartmentsList() ทีละพนักงาน (N+1 query)
        // เปลี่ยนเป็นโหลด Department ของพนักงานทั้งหมดในครั้งเดียว
        var departmentRows = await (from a in dbHr.TblDepartments.AsNoTracking()
                                    join b in dbHr.TblEmployeeDepartments.AsNoTracking()
                                        on a.DeptCode equals b.DeptCode
                                    where empCodes.Contains(b.EmpCode)
                                    select new
                                    {
                                        b.EmpCode,
                                        Department = a
                                    }).ToListAsync();

        var departmentLookup = departmentRows
            .GroupBy(x => x.EmpCode)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Department).ToList());

        foreach (var item in employees)
        {
            item.departments = departmentLookup.TryGetValue(item.emp_code, out var departments)
                ? departments
                : new List<TblDepartment>();

            item.dept_name = string.Join(",", item.departments.Select(s => s.DeptName));
        }

        return employees;
    }

    public async Task<List<DriverViewModel>> GetAllDriverList()
    {
        return await (from a in dbHr.ViewHrDriverLists.AsNoTracking()
                      orderby a.IsActived, a.TruckNo
                      select new DriverViewModel
                      {
                          emp_code = a.EmpCode,
                          fullname = a.EmpFullname,
                          phone = a.EmpPhone,
                          start_date = a.StartDate.ToString("dd-MM-yyyy"),
                          truck_code = a.TruckCode,
                          truck_license = a.TruckLicense,
                          truck_no = a.TruckNo,
                          is_actived = a.IsActived == 1,
                          status = a.StatusName
                      }).ToListAsync();
    }

    public async Task<List<DriverViewModel>> GetActiveDriverList()
    {
        return await (from a in dbHr.ViewHrDriverListActives.AsNoTracking()
                      where a.IsEnabled == true
                      orderby a.TruckNo
                      select new DriverViewModel
                      {
                          emp_code = a.EmpCode,
                          fullname = a.EmpFullname,
                          phone = a.EmpPhone,
                          driver_img = a.EmpImg,
                          start_date = a.StartDate.ToString("dd-MM-yyyy"),
                          truck_code = a.TruckCode,
                          truck_license = a.TruckLicense,
                          truck_no = a.TruckNo,
                          status = a.StatusName
                      }).ToListAsync();
    }

    public async Task<TblEmployee> GetEmployeeDataByCode(string code)
    {
        return await dbHr.TblEmployees
            .FirstOrDefaultAsync(w => w.EmpCode == code);
    }

    public async Task<ResponseViewModel> AddEmployee(EmployeeModel model)
    {
        var response = new ResponseViewModel();

        await using var trHr = await dbHr.Database.BeginTransactionAsync();
        await using var trLog = await dbLog.Database.BeginTransactionAsync();
        try
        {
            var employee = new TblEmployee
            {
                EmpCode = model.emp_code,
                InitialName = model.initial_name,
                EmpFullname = model.fullname,
                StatusCode = model.probation_days > 0 ? "02" : "01",
                IsActived = true,
                IsEnabled = true,
                CreateDate = DateTime.Now,
                CreateBy = model.create_by,
                UpdateDate = DateTime.Now,
                UpdateBy = model.create_by
            };

            employee.TblEmployeeDetail = new TblEmployeeDetail
            {
                IdCard = model.id_card,
                EmpPhone = model.phone,
                EmpMail = model.mail ?? "-",
                EmpAddress = model.address,
                EmpGender = model.gender,
                BirthDate = model.birth_date,
                StartDate = model.start_date,
                EmpAvatar = "dfavatar.jpg",
                EmpImg = model.emp_code + ".jpg",
                EmpIdcard = model.emp_idcard,
                IdLine = model.id_line
            };

            foreach (var deptCode in model.dept_code)
            {
                if (deptCode == "01")
                {
                    var driverCode = await codeRepo.GetDriverCode();

                    dbHr.TblEmployeeDrivers.Add(new TblEmployeeDriver
                    {
                        EmpCode = employee.EmpCode,
                        DriverCode = driverCode,
                        LicenseTypeCode = model.license_type,
                        IsEnabled = true,
                        CreateDate = DateTime.Now,
                        CreateBy = model.create_by,
                        UpdateDate = DateTime.Now,
                        UpdateBy = model.create_by
                    });

                    dbLog.TblDrivers.Add(new TblDriver
                    {
                        DriverCode = driverCode,
                        DriverFullname = model.initial_name + model.fullname,
                        DriverPhone = model.phone,
                        IsEnabled = true
                    });
                }

                dbHr.TblEmployeeDepartments.Add(new TblEmployeeDepartment
                {
                    EmpCode = model.emp_code,
                    DeptCode = deptCode,
                    CreateDate = DateTime.Now,
                    CreateBy = model.create_by
                });
            }

            dbHr.TblEmployeeFunds.Add(new TblEmployeeFund
            {
                EmpCode = model.emp_code,
                SsoDeductValue = 0,
                SsoBalanceValue = 0,
                SecureDeductValue = 0,
                SecureBalanceValue = 0,
                IsEnabled = true,
                CreateDate = DateTime.Now,
                CreateBy = model.create_by,
                UpdateDate = DateTime.Now,
                UpdateBy = model.create_by
            });

            var contract = new TblContract
            {
                ContractCode = await codeRepo.GetDriverContractCode(),
                EmpCode = model.emp_code,
                ContractDate = model.contract_date,
                StartDate = model.start_date,
                ContractTypeCode = model.contract_type,
                Remark = model.remark,
                IsEnabled = true,
                CreateDate = DateTime.Now,
                CreateBy = model.create_by,
                UpdateDate = DateTime.Now,
                UpdateBy = model.create_by
            };

            contract.TblEmployeeProbations.Add(new TblEmployeeProbation
            {
                ContractCode = contract.ContractCode,
                ProbationDays = model.probation_days,
                StartDate = model.start_date,
                FinishDate = model.start_date.AddDays(model.probation_days),
                ProbationStatus = model.probation_days > 0 ? "P" : "D",
                Remark = "-",
                CreateDate = DateTime.Now,
                CreateBy = model.create_by
            });

            employee.TblEmployeeDocuments = model.employee_files
                .Select(item => new TblEmployeeDocument
                {
                    DocumentType = item.filetype,
                    DocumentPath = item.filepath,
                    DocumentName = item.filename,
                    DocumentDetail = item.detail,
                    EmpCode = model.emp_code,
                    CreateDate = DateTime.Now,
                    CreateBy = model.create_by
                })
                .ToList();

            dbHr.TblEmployees.Add(employee);
            dbHr.TblContracts.Add(contract);

            // เดิม SaveChanges ฝั่ง LOG อยู่ใน loop
            // ตอนนี้บันทึกแต่ละ DbContext เพียงครั้งเดียว
            await dbLog.SaveChangesAsync();
            await dbHr.SaveChangesAsync();

            await trHr.CommitAsync();
            await trLog.CommitAsync();

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            try
            {
                await trLog.RollbackAsync();
            }
            catch
            {
                // Transaction อาจถูก rollback/dispose ไปแล้ว
            }

            try
            {
                await trHr.RollbackAsync();
            }
            catch
            {
                // Transaction อาจถูก rollback/dispose ไปแล้ว
            }

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> EditSecureValue(
        string code,
        decimal value,
        decimal balance,
        string username)
    {
        var response = new ResponseViewModel();

        try
        {
            var affected = await dbHr.TblEmployeeFunds
                .Where(w => w.EmpCode == code)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.SecureDeductValue, value)
                    .SetProperty(x => x.SecureBalanceValue, balance)
                    .SetProperty(x => x.UpdateBy, username)
                    .SetProperty(x => x.UpdateDate, DateTime.Now));

            response.result = affected > 0;
        }
        catch (Exception ex)
        {
            response.result = false;
            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> EditSSOValue(
        string code,
        decimal value,
        decimal balance,
        string username)
    {
        var response = new ResponseViewModel();

        try
        {
            var affected = await dbHr.TblEmployeeFunds
                .Where(w => w.EmpCode == code)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.SsoDeductValue, value)
                    .SetProperty(x => x.SsoBalanceValue, balance)
                    .SetProperty(x => x.UpdateBy, username)
                    .SetProperty(x => x.UpdateDate, DateTime.Now));

            response.result = affected > 0;
        }
        catch (Exception ex)
        {
            response.result = false;
            response.errorException = ex;
        }

        return response;
    }

    public async Task<List<TblEmployeeSalary>> GetSalaryListByEmp(string code)
    {
        return await dbHr.TblEmployeeSalaries
            .Where(w => w.EmpCode == code)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<EmployeeModel> GetEmployeeModelDataByCode(string code)
    {
        return await (from a in dbHr.TblEmployees.AsNoTracking()
                      join b in dbHr.TblEmployeeDetails.AsNoTracking()
                          on a.EmpCode equals b.EmpCode
                      where a.EmpCode == code && a.IsEnabled == true
                      select new EmployeeModel
                      {
                          emp_code = a.EmpCode,
                          id_card = b.IdCard,
                          initial_name = a.InitialName,
                          fullname = a.EmpFullname,
                          gender = b.EmpGender,
                          birth_date = b.BirthDate,
                          address = b.EmpAddress,
                          original_address = b.OriginalAddress,
                          phone = b.EmpPhone,
                          mail = b.EmpMail,
                          img = b.EmpImg,
                          emp_idcard = b.EmpIdcard,
                          id_line = b.IdLine
                      }).FirstOrDefaultAsync();
    }

    public async Task<EmployeeViewModel> GetEmployeeByCode(string code)
    {
        var dataProfile = await (from a in dbHr.ViewEmployeeLists.AsNoTracking()
                                 where a.EmpCode == code
                                 select new EmployeeViewModel
                                 {
                                     emp_code = a.EmpCode,
                                     id_card = a.IdCard,
                                     emp_fullname = a.InitialName + a.EmpFullname,
                                     emp_gender = a.EmpGender == "M" ? "ชาย" : "หญิง",
                                     birth_date = a.BirthDate,
                                     emp_current_address = a.EmpAddress,
                                     emp_original_address = a.OriginalAddress,
                                     emp_phone = a.EmpPhone,
                                     emp_mail = a.EmpMail,
                                     emp_img = a.EmpImg,
                                     age = (DateTime.Now.Year - a.BirthDate.Year).ToString(),
                                     status_code = a.StatusCode,
                                     status_name = a.StatusName,
                                     is_actived = a.IsActived
                                 }).FirstOrDefaultAsync();

        if (dataProfile == null)
            return dataProfile;

        dataProfile.contracts = await GetOfficeWorking(dataProfile.emp_code);
        dataProfile.documents = await GetDocumentListByCode(dataProfile.emp_code);
        dataProfile.departments = await GetTblDepartmentsList(dataProfile.emp_code);

        return dataProfile;
    }

    public async Task<DriverViewModel> GetDriverByCode(string code)
    {
        var dataProfile = await (from a in dbHr.ViewHrDriverLists.AsNoTracking()
                                 where a.EmpCode == code
                                 select new DriverViewModel
                                 {
                                     emp_code = a.EmpCode,
                                     id_card = a.IdCard,
                                     fullname = a.EmpFullname,
                                     gender_id = a.EmpGender,
                                     driver_birthdate = a.BirthDate,
                                     address = a.EmpAddress,
                                     original_address = a.OriginalAddress,
                                     phone = a.EmpPhone,
                                     mail = a.EmpMail,
                                     driver_img = a.EmpImg,
                                     age = DateTime.Now.Year - a.BirthDate.Year,
                                     truck_no = a.TruckNo,
                                     truck_license = a.TruckLicense,
                                     status = a.StatusName,
                                     is_actived = a.IsActived == 1
                                 }).FirstOrDefaultAsync();

        if (dataProfile == null)
            return dataProfile;

        dataProfile.contracts = await GetOfficeWorking(dataProfile.emp_code);
        dataProfile.documents = await GetDocumentListByCode(dataProfile.emp_code);

        return dataProfile;
    }

    public async Task UpdateImageFile(string img, string code)
    {
        try
        {
            await dbHr.TblEmployeeDetails
                .Where(w => w.EmpCode == code)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.EmpImg, img));
        }
        catch
        {
            // รักษาพฤติกรรมเดิมของ method ที่ไม่ throw exception ออกไป
        }
    }

    public async Task<ResponseViewModel> EditDriver(EmployeeModel model)
    {
        var response = new ResponseViewModel();

        await using var tr = await dbHr.Database.BeginTransactionAsync();

        try
        {
            var employee = await dbHr.TblEmployees
                .FirstOrDefaultAsync(w => w.EmpCode == model.emp_code);

            if (employee == null)
                throw new InvalidOperationException($"ไม่พบพนักงานรหัส {model.emp_code}");

            var detail = await dbHr.TblEmployeeDetails
                .FirstOrDefaultAsync(w => w.EmpCode == model.emp_code);

            if (detail == null)
                throw new InvalidOperationException($"ไม่พบรายละเอียดพนักงานรหัส {model.emp_code}");

            employee.InitialName = model.initial_name;
            employee.EmpFullname = model.fullname;
            employee.UpdateDate = DateTime.Now;
            employee.UpdateBy = model.create_by;

            detail.EmpGender = model.gender;
            detail.BirthDate = model.birth_date;
            detail.OriginalAddress = model.original_address;
            detail.EmpAddress = model.address;
            detail.EmpPhone = model.phone;
            detail.EmpMail = model.mail;
            detail.EmpIdcard = model.emp_idcard;
            detail.IdLine = model.id_line;

            await dbHr.SaveChangesAsync();
            await tr.CommitAsync();

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            await tr.RollbackAsync();

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }

    public async Task<List<TblDepartment>> GetTblDepartmentsList(string code)
    {
        return await (from a in dbHr.TblDepartments.AsNoTracking()
                      join b in dbHr.TblEmployeeDepartments.AsNoTracking()
                          on a.DeptCode equals b.DeptCode
                      where b.EmpCode == code
                      select a).ToListAsync();
    }

    public async Task<List<TblEmployeeDocument>> GetDocumentListByCode(string code)
    {
        return await dbHr.TblEmployeeDocuments
            .AsNoTracking()
            .Where(a => a.EmpCode == code)
            .ToListAsync();
    }

    public async Task<ProbationViewModel> GetProbation(string emp_code)
    {
        var dataProbation = await (from a in dbHr.TblEmployees.AsNoTracking()
                                   join b in dbHr.TblContracts.AsNoTracking()
                                       on a.EmpCode equals b.EmpCode
                                   join c in dbHr.TblEmployeeProbations.AsNoTracking()
                                       on b.ContractCode equals c.ContractCode
                                   where a.EmpCode == emp_code &&
                                         c.ProbationStatus == "P"
                                   select new ProbationViewModel
                                   {
                                       start_date = c.StartDate,
                                       finish_date = c.FinishDate
                                   }).FirstOrDefaultAsync();

        if (dataProbation != null)
        {
            dataProbation.days =
                (dataProbation.finish_date.ToDateTime(TimeOnly.MinValue) - DateTime.Now.Date).Days;
        }

        return dataProbation;
    }

    public async Task<List<SalaryViewModel>> GetSalary(string code)
    {
        return await (from a in dbHr.TblEmployeeSalaries.AsNoTracking()
                      join b in dbHr.TblSalaryTypes.AsNoTracking()
                          on a.SalaryTypeCode equals b.SalaryTypeCode
                      where a.EmpCode == code
                      orderby a.StartDate descending
                      select new SalaryViewModel
                      {
                          code = b.SalaryTypeCode,
                          start_date = a.StartDate,
                          end_date = a.EndDate,
                          salary_type = b.SalaryTypeName,
                          salary_value = a.SalaryValue,
                          detail = a.SalaryDetail,
                          is_enabled = a.IsEnabled
                      })
            .Take(5)
            .ToListAsync();
    }

    public async Task<TblEmployeeFund> GetFund(string code)
    {
        return await dbHr.TblEmployeeFunds
            .FirstOrDefaultAsync(w => w.EmpCode == code);
    }

    public async Task<ResponseViewModel> Resign(TblEmployeeResign model, string username)
    {
        var response = new ResponseViewModel();

        await using var trHr = await dbHr.Database.BeginTransactionAsync();
        await using var trLog = await dbLog.Database.BeginTransactionAsync();

        try
        {
            var employee = await dbHr.TblEmployees
                .FirstOrDefaultAsync(w => w.EmpCode == model.EmpCode);

            if (employee == null)
                throw new InvalidOperationException($"ไม่พบพนักงานรหัส {model.EmpCode}");

            employee.IsActived = false;
            employee.IsEnabled = false;
            employee.UpdateDate = DateTime.Now;
            employee.UpdateBy = username;
            employee.StatusCode = "03";

            // เดิมโหลด Department ทั้งหมดแล้ววน foreach ทั้งที่ต้องการเช็กเพียง DeptCode = 01
            var isDriver = await dbHr.TblEmployeeDepartments
                .AsNoTracking()
                .AnyAsync(w => w.EmpCode == model.EmpCode && w.DeptCode == "01");

            if (isDriver)
            {
                var employeeDriver = await dbHr.TblEmployeeDrivers
                    .FirstOrDefaultAsync(w =>
                        w.EmpCode == model.EmpCode &&
                        w.IsEnabled == true);

                if (employeeDriver != null)
                {
                    employeeDriver.IsEnabled = false;
                    employeeDriver.UpdateDate = DateTime.Now;
                    employeeDriver.UpdateBy = username;

                    var driver = await dbLog.TblDrivers
                        .FirstOrDefaultAsync(w => w.DriverCode == employeeDriver.DriverCode);

                    if (driver != null)
                    {
                        driver.IsEnabled = false;

                        var driverTruck = await dbLog.TblTruckDrivers
                            .FirstOrDefaultAsync(w => w.DriverCode == driver.DriverCode);

                        if (driverTruck != null)
                            dbLog.TblTruckDrivers.Remove(driverTruck);
                    }
                }
            }

            await dbHr.SaveChangesAsync();
            await dbLog.SaveChangesAsync();

            await trHr.CommitAsync();
            await trLog.CommitAsync();

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            try
            {
                await trLog.RollbackAsync();
            }
            catch
            {
            }

            try
            {
                await trHr.RollbackAsync();
            }
            catch
            {
            }

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }

    public async Task<ResponseViewModel> UpdateSalary(TblEmployeeSalary model)
    {
        var response = new ResponseViewModel();

        await using var tr = await dbHr.Database.BeginTransactionAsync();

        try
        {
            var currentSalary = await dbHr.TblEmployeeSalaries
                .FirstOrDefaultAsync(w =>
                    w.EmpCode == model.EmpCode &&
                    w.IsEnabled == true);

            if (currentSalary != null)
            {
                currentSalary.EndDate = model.StartDate.AddDays(-1);
                currentSalary.IsEnabled = false;
                currentSalary.UpdateDate = DateTime.Now;
                currentSalary.UpdateBy = model.CreateBy;
            }

            model.SalaryCode = await codeRepo.GetSalaryCode();
            dbHr.TblEmployeeSalaries.Add(model);

            await dbHr.SaveChangesAsync();
            await tr.CommitAsync();

            response.result = true;
            response.resultMessage = "บันทึกข้อมูลเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            await tr.RollbackAsync();

            response.result = false;
            response.resultMessage =
                "<div>เกิดข้อผิดพลาดระหว่างการทำงาน</div><div style='margin-top:-30px'> กรุณาลองใหม่อีกครั้ง หรือติดต่อผู้ดูแลระบบ</div>";
            response.errorException = ex;
        }

        return response;
    }

    private async Task<List<EmployeeContractViewModel>> GetOfficeWorking(string code)
    {
        return await (from a in dbHr.TblEmployees.AsNoTracking()
                      join b in dbHr.TblContracts.AsNoTracking()
                          on a.EmpCode equals b.EmpCode
                      join d in dbHr.TblContractTypes.AsNoTracking()
                          on b.ContractTypeCode equals d.ContractTypeCode
                      where a.EmpCode == code
                      select new EmployeeContractViewModel
                      {
                          contract_code = b.ContractCode,
                          contract_date = b.ContractDate,
                          start_date = b.StartDate,
                          contract_type = d.ContractTypeName,
                          contract_status = b.IsEnabled,
                          remark = b.Remark
                      }).ToListAsync();
    }
}
