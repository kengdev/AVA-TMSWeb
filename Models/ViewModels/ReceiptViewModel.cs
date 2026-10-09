namespace TMSWeb.Models.ViewModels;
public class ReceiptViewModel
{
    public string receipt_code { get; set; }
    public string receipt_part_code { get; set; }
    public string customer_name { get; set; }
    public string officer_fullname { get; set; }
    public decimal receipt_total { get; set; }
    public string receipt_date { get; set; }
    public int receipt_amount { get; set; }
    public string billing_code { get; set; }
}

public class TempReceiptViewModel
{
    public decimal advance_total { get; set; }
    public decimal transport_total { get; set; }
    public decimal total { get; set; }
    public decimal tmp_advance_total { get; set; }
    public decimal tmp_transport_total { get; set; }
    public decimal tmp_total { get; set; }
    public bool status { get; set; }
    public DateOnly? receipt_date { get; set; }
    public string billing_code { get; set; }
}