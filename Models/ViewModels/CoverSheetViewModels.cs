using System;
namespace TMSWeb.Models.ViewModels;
public class CoverSheetViewModel
{
    public string billing_code { get; set; }
    public string billing_type { get; set; }
    public string customer_code { get; set; }
    public string customer_name { get; set; }
    public DateTime create_date { get; set; }
    public string billing_date { get; set; }
    public decimal advance_value { get; set; }
    public decimal transport_value { get; set; }
    public decimal billing_total { get; set; }
    public bool is_coversheet { get; set; }
    public string coversheet_code { get; set; }

    public int coversheet_amount { get; set; }
    public decimal coversheet_total { get; set; }
    public decimal coversheet_value { get; set; }
    public string coversheet_remark { get; set; }
    public string fullname { get; set; }
    public string coversheet_date { get; set; }
}