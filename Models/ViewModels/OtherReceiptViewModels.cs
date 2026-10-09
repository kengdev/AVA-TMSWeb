using TMSWeb.Models.Entities.LOG;
namespace TMSWeb.Models.ViewModels;
public class ContainerHandlingViewModel
{
    public string code { get; set; }
    public string customer_name { get; set; }
    public string customer_tax { get; set; }
    public string customer_address { get; set; }

    public string container_no { get; set; }
    public string container_size { get; set; }
    public decimal total { get; set; }
    public decimal vat { get; set; }
    public decimal net_total { get; set; }
    public string receipt_date { get; set; }
    
    public decimal price { get; set; }
    public string payment_type { get; set; }
    public string bank_name { get; set; }
    public string branch { get; set; }
    public string cheque_no { get; set; }
    public DateOnly? cheque_date { get; set; }

}