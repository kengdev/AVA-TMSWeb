namespace TMSWeb.Models;
public class EmployeeModel
{
    public string emp_code { get; set; }
    public string initial_name { get; set; }
    public string fullname { get; set; }
    public string[] dept_code { get; set; }
    public string id_card { get; set; }
    public string id_line { get; set; }
    public string phone { get; set; }
    public string mail { get; set; }
    public string gender { get; set; }
    public string address { get; set; }
    public string original_address { get; set; }
    public DateOnly birth_date { get; set; }
    public string img { get; set; }


    public DateOnly contract_date { get; set; }
    public DateOnly start_date { get; set; }
    public int probation_days { get; set; }
    public string truck_code { get; set; }
    public decimal salary { get; set; }
    public string license_type { get; set; }
    public string salary_type { get; set; }
    public string contract_type { get; set; }
    public string remark { get; set; }
    public string create_by { get; set; }
    public string emp_idcard { get; set; }

    public List<EmployeeFileModel> employee_files { get; set; }
}

public class EmployeeFileModel
{
    public string filename { get; set; }
    public string filetype { get; set; }
    public string filepath { get; set; }
    public string detail { get; set; }
}

public class EmployeePicModel
{
    public string emp_code { get; set; }
    public string emp_pic { get; set; }
    public List<IFormFile> files { get; set; }
}