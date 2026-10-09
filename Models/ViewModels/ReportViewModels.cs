namespace TMSWeb.Models.ViewModels;
public class ReportJobViewModel
{
    public string code { get; set; }
    public DateTime job_date_value { get; set; }
    public string job_date { get; set; }
    public string customer_name { get; set; }
    public string booking_no { get; set; }
    public string container_no { get; set; }
    public string invoice_no { get; set; }
    public string container_size { get; set; }
    public string mission_name { get; set; }
    public string zone_name { get; set; }
    public string supplier_name { get; set; }
    public string truck_license { get; set; }
    public string truck_no { get; set; }
    public string driver_name { get; set; }
    public decimal supplier_charge { get; set; }
    public decimal customer_charge { get; set; }
    public string billing_code { get; set; }
    public List<ReportAdvanceViewModel>? advance_list { get; set; }
}

public class ReportAdvanceViewModel
{
    public string advance_name { get; set; }
    public decimal supplier { get; set; }
    public decimal customer { get; set; }
}