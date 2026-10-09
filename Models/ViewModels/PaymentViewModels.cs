namespace TMSWeb.Models.ViewModels;
public class SubstractPaymentViewModel
{
    public string sub_type { get; set; }
    public string sub_name { get; set; }
    public decimal sub_value { get; set; }
}

public class DriverPaymentViewModel
{
    public string allowance_code { get; set; }
    public string driver_code { get; set; }
    public string driver_fullname { get; set; }
    public string truck_license { get; set; }
    public string truck_no { get; set; }
    public DateOnly start_date { get; set; }
    public DateOnly end_date { get; set; }
    public decimal allowance_total { get; set; }

    public string start_date_form { get; set; }
    public string end_date_form { get; set; }
    public string start_date_text { get; set; }
    public string end_date_text { get; set; }
}

public class DriverPaymentDetailViewModel
{
    public string fullname { get; set; }
    public string allowance_code { get; set; }
    public int id { get; set; }
    public string code { get; set; }
    public string job_detail_code { get; set; }
    public string booking_no { get; set; }
    public string container_no { get; set; }
    public string trip_place { get; set; }
    public string trip_date { get; set; }
    public DateTime working_date { get; set; }
    public DateTime allownace_date { get; set; }
    public string truck_license { get; set; }
    public string driver_code { get; set; }
    public decimal fuel_value { get; set; }
    public decimal trip_value { get; set; }
    public decimal waste_value { get; set; }
    public decimal other_value { get; set; }
    public bool? is_approve { get; set; }
    public string mission_name { get; set; }
    public string zone_name { get; set; }
}

public class OtherPaymentViewModel
{
    public List<string> subtractType { get; set; }
    public List<string> subtractName { get; set; }
    public List<decimal> subtractVal { get; set; }
}



public class SupplierPaymentViewModel
{
    public string truck_license { get; set; }
    public string supplierpayment_code { get; set; }
    public string supplier_code { get; set; }
    public string supplier_name { get; set; }
    public string supplierpayment_date { get; set; }
    public int supplierpayment_amount { get; set; }
    public decimal supplierpayment_net { get; set; }
}

public class FleetPaymentViewModel
{
    public string code { get; set; }
    public string supplier_name { get; set; }
    public string month { get; set; }
    public string year { get; set; }
    public decimal salary { get; set; }
    public decimal shore { get; set; }
    public decimal area { get; set; }
    public decimal maintenance { get; set; }
    public decimal net_total { get; set; }
}