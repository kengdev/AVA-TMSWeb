using System;
namespace TMSWeb.Models.ViewModels;
public class BillingViewModel
{
    public int id { get; set; }
    public string billing_type { get; set; }
    public string billing_code { get; set; }
    public decimal billing_total { get; set; }
    public int billing_amount { get; set; }
    public string customer_code { get; set; }
    public string customer_name { get; set; }
    public string billing_date { get; set; }
    public DateOnly billing_date_value { get; set; }
    public bool is_clearing { get; set; }
    public string clearing_date { get; set; }
    public decimal advance_total { get; set; }
    public decimal advance_other_value { get; set; }
    public decimal cashing_total { get; set; }
    public decimal cashing_other_value { get; set; }
    public string cashing_remark { get; set; }

    public string booking_no { get; set; }
    public string invoice_no { get; set; }
    public string container_no { get; set; }
    public string mission_name { get; set; }
    public decimal cost_value { get; set; }
    public DateTime job_date_value { get; set; }
    public string job_date { get; set; }
    public string job_detail_code { get; set; }

    public string job_type { get; set; }
    public string seal_no { get; set; }
    public string container_back_no { get; set; }
    public string seal_back_no { get; set; }
    public string container_type { get; set; }
    public string container_kind { get; set; }
    public string agent_code { get; set; }
    public string truck_license { get; set; }
    public string trailer_license { get; set; }
    public string driver_fullname { get; set; }
    public string supplier_name { get; set; }
    public string supplier_code { get; set; }
    public bool is_billing { get; set; }
    public decimal net_total { get; set; }
    public decimal transport_value { get; set; }
    public decimal advance_value { get; set; }

    public string advance_other { get; set; }
}

public class AdvanceViewModel
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
}

public class PendingCountViewModel
{
    public string month { get; set; }
    public int? count_pending { get; set; }
}

public class PendingBillViewModel
{
    public string customer_code { get; set; }
    public string customer_name { get; set; }
    public string supplyer_code { get; set; }
    public string month { get; set; }
    public int month_01 { get; set; }
    public int month_02 { get; set; }
    public int month_03 { get; set; }
    public int month_04 { get; set; }
    public int month_05 { get; set; }
    public int month_06 { get; set; }
    public int month_07 { get; set; }
    public int month_08 { get; set; }
    public int month_09 { get; set; }
    public int month_10 { get; set; }
    public int month_11 { get; set; }
    public int month_12 { get; set; }
}

public class CustomerBillingViewModel
{
    public string customer_code { get; set; }
    public string customer_name { get; set; }
}

/*public class FormPND50ViewModel
{
    public string supplyer_code { get; set; }
    public decimal total_value { get; set; }
}

public class PND50ViewModel
{
    public string code { get; set; }
    public string supplyer_name { get; set; }
    public string supplyer_address { get; set; }
    public string supplyer_vat_no { get; set; }
    public decimal total_value { get; set; }
    public decimal vat { get; set; }
    public string vat_text { get; set; }

    public string customer_name { get; set; }
    public string customer_address { get; set; }
    public string customer_vat_no { get; set; }
}

*/