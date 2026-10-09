namespace TMSWeb.Models.ViewModels;
public class SparePartViewModel
{
    public string sparepart_code { get; set; }
    public string sparepart_name { get; set; }
    public decimal price { get; set; }
    public int quantity { get; set; }
    public string unit { get; set; }
    public string type { get; set; }
}