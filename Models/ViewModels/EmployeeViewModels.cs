using TMSWeb.Models.Entities.HR;
namespace TMSWeb.Models.ViewModels;
public class EmployeeViewModel
{
    public string emp_code { get; set; }
    public string initial_name { get; set; }
    public string emp_fullname { get; set; }
    public string driver_code { get; set; }
    public string id_card { get; set; }
    public string emp_phone { get; set; }
    public string emp_mail { get; set; }
    public string emp_gender { get; set; }
    public string emp_original_address { get; set; }
    public string emp_current_address { get; set; }
    public string marital_name { get; set; }
    public string education_name { get; set; }
    public DateOnly birth_date { get; set; }
    public string age { get; set; }
    public string emp_img { get; set; }
    public string idcard_img { get; set; }
    public string status_code { get; set; }
    public string status_name { get; set; }
    public string start_date { get; set; }
    public bool is_actived { get; set; }
    public bool is_enabled { get; set; }
    public decimal current_salary { get; set; }
    public string dept_name { get; set; }
    public List<TblDepartment> departments { get; set; }
    public TblEmployeeFund fund { get; set; }
    public ProbationViewModel probation { get; set; }
    public List<SalaryViewModel> salaries { get; set; }
    public List<TblEmployeeDocument> documents { get; set; }
    public List<EmployeeContractViewModel> contracts { get; set; }
}

public class SalaryViewModel
{
    public string code { get; set; }
    public DateOnly start_date { get; set; }
    public DateOnly? end_date { get; set; }
    public string salary_type { get; set; }
    public decimal salary_value { get; set; }
    public string detail { get; set; }
    public bool is_enabled { get; set; }
}

public class ProbationViewModel
{
    public int days { get; set; }
    public DateOnly start_date { get; set; }
    public DateOnly finish_date { get; set; }

}

public class EmployeeContractViewModel
{
    public string contract_code { get; set; }
    public DateOnly contract_date { get; set; }
    public DateOnly start_date { get; set; }
    public string dept_name { get; set; }
    public string probation_status { get; set; }
    public int probation_days { get; set; }
    public int probation_days_remaining { get; set; }
    public decimal salary { get; set; }
    public string salary_type { get; set; }
    public string contract_type { get; set; }
    public bool contract_status { get; set; }
    public string license_type { get; set; }
    public string license { get; set; }
    public string remark { get; set; }
}