using TMSWeb.Models.Entities.LOG;
namespace TMSWeb.Models.ViewModels;

public class QuotationViewModel
{
    public string quotation_code { get; set; }
    public string quotation_name { get; set; }
    public string customer { get; set; }
    public string route { get; set; }
    public string rate { get; set; }
    public string price { get; set; }
    public string start_date { get; set; }
    public string expire_date { get; set; }
    public string container1 { get; set; }
    public string container2 { get; set; }
    public string container3 { get; set; }
    public string create_date { get; set; }
    public bool is_actived { get; set; }
    public TblQuotationDetail active_detail { get; set; }
}