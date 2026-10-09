namespace TMSWeb.Models;
public class CostModel
{
    public string customer_code { get; set; }
    public string customer_short_name { get; set; }
    public string customer_name_th { get; set; }
    public string cost_code { get; set; }
    public string cost_name { get; set; }
    public string customer_acronym { get; set; }
    public string customer_address { get; set; }
    public string customer_tax { get; set; }
    public decimal cost_value { get; set; }
    public string cost_remark { get; set; }
    public string mission_code { get; set; }
    public string mission_name { get; set; }
    public string mission_address { get; set; }
    public string mission_tax { get; set; }
    public string mission_type { get; set; }
    public decimal expressway_value { get; set; }
}