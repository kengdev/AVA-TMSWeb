using TMSWeb.Models.Entities.HR;

namespace TMSWeb.Models.ViewModels;

public class SalaryListViewModel
{
    public string payment_code { get; set; }
    public string payment_month { get; set; }
    public string payment_year { get; set; }
    public decimal payment_total { get; set; }
    public int amount { get; set; }
    public int approved { get; set; }
    public string payment_date { get; set; }
    public bool is_actived { get; set; }
}
public class SalaryItemViewModel
{

    public string payment_code { get; set; }
    public string employee_code { get; set; }
    public string employee_name { get; set; }
    public string emp_img { get; set; }
    public string dept_code { get; set; }
    public string dept_name { get; set; }
    public string month { get; set; }
    public string year { get; set; }
    public decimal salary_total { get; set; }
    public bool status { get; set; }
    public string driver_code { get; set; }
    public string truck_license { get; set; }
    public string truck_no { get; set; }
    public List<TblDepartment> departments { get; set; }
}