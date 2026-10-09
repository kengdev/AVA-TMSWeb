namespace TMSWeb.Models.ViewModels;
public class ShippingPaymentViewModel
{
    public string payment_code { get; set; }
    public string customer_name { get; set; }
    public string payment_date { get; set; }
    public decimal payment_total { get; set; }
    public int payment_amount { get; set; }
}
