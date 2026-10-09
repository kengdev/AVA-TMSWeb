using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMSWeb.Models.Entities.HR;

namespace TMSWeb.Models.ViewModels;
public class DriverViewModel
{
    public string emp_code { get; set; }
    public string driver_code { get; set; }
    public string id_card { get; set; }
    public string fullname { get; set; }
    public string gender_id { get; set; }
    public DateOnly driver_birthdate { get; set; }
    public string address { get; set; }
    public string original_address { get; set; }
    public int age { get; set; }
    public string phone { get; set; }
    public string mail { get; set; }
    public string supplyer_code { get; set; }
    public string supplyer_name { get; set; }
    public string truck_license { get; set; }
    public string truck_no { get; set; }
    public string truck_code { get; set; }
    public string truck_typegroup_code { get; set; }
    public string trailer_license { get; set; }
    public string driver_img { get; set; }
    public bool is_working { get; set; }
    public string fleet_type { get; set; }
    public string start_date { get; set; }
    public string status { get; set; }
    public bool is_actived { get; set; }

    public TblEmployeeFund fund { get; set; }
    public ProbationViewModel probation { get; set; }
    public List<SalaryViewModel> salaries { get; set; }
    public List<TblEmployeeDocument> documents { get; set; }
    public List<EmployeeContractViewModel> contracts { get; set; }
}
