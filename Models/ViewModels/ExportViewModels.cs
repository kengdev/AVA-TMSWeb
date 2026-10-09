using System;

namespace TMSWeb.Models;
public class ExpInvoiceModel
{
    public string job_code { get; set; }
    public string job_detail_code { get; set; }
    public string customer_name { get; set; }
    public string start_name { get; set; }
    public string mission_name { get; set; }
    public string booking_no { get; set; }
    public string seal_no { get; set; }
    public string tracking_status { get; set; }
    public string truck_license { get; set; }
    public string trailer_license { get; set; }
    public string driver_fullname { get; set; }
    public string driver_phone { get; set; }
    public string container_no { get; set; }
    public string container_size { get; set; }
    public DateTime job_date { get; set; }
    public string closing_time { get; set; }
    public string job_remark { get; set; }
    public decimal tare_weight { get; set; }
    public string supplier { get; set; }
    public string industrial { get; set; }
}

public class ExpPaymentModel
{
    public string job_type { get; set; }
    public string job_detail_code { get; set; }
    public string start_name { get; set; }
    public string mission_name { get; set; }
    public string booking_no { get; set; }
    public string truck_license { get; set; }
    public string driver_fullname { get; set; }
    public string container_no { get; set; }
    public string container_size { get; set; }
    public DateTime job_date { get; set; }
    public string supplier { get; set; }
    public string industrial { get; set; }
    public int fuel { get; set; }
    public decimal port { get; set; }
    public decimal lift { get; set; }
    public decimal express { get; set; }
    public decimal sh { get; set; }
}

public class ExpOverdueModel
{
    public string billing_code { get; set; }
    public string booking_no { get; set; }
    public DateOnly billing_date { get; set; }
    public string customer_name { get; set; }
    public string mission_name { get; set; }
    public decimal transport_value { get; set; }
    public decimal advance_value { get; set; }
    public decimal billing_total { get; set; }
    public string billing_type { get; set; }
}