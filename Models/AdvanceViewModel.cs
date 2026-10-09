using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMSWeb.Models.ViewModels;
/*public class AdvanceViewModel
{
    public string code { get; set; }
    public int id { get; set; }
    public string container_no { get; set; }
    public string booking_no { get; set; }
    public string advance_name { get; set; }
    public decimal advance_value { get; set; }
    public string advance_type { get; set; }
}

public class AdvanceJobViewModel
{
    public int id { get; set; }
    public DateTime job_date { get; set; }
    public string job_detail_code { get; set; }
    public string container_no { get; set; }
    public string booking_no { get; set; }
    public string start_name { get; set; }
    public string mission_name { get; set; }
    public string truck_license { get; set; }
    public string customer_name { get; set; }
    public string advance_other { get; set; }
    public decimal advance_other_value { get; set; }
    public decimal advance_total { get; set; }
    public string job_code { get; set; }
}*/

public class OtherJobViewModel
{
    public string name { get; set; }
    public string container_no { get; set; }
    public string place { get; set; }
    public decimal value { get; set; }
    public decimal value_up { get; set; }
    public decimal supplier_value { get; set; }
}