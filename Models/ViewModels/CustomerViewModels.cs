namespace TMSWeb.Models.ViewModels;
public class CustomerViewModel
{
    public string customer_code { get; set; }
    public string customer_name_th { get; set; }
    public string customer_name_en { get; set; }
    public string customer_acronym { get; set; }
    public string customer_type_name { get; set; }
    public string customer_address { get; set; }
    public string customer_tax { get; set; }
    public bool is_actived { get; set; }
}