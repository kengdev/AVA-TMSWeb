namespace TMSWeb.Models.ViewModels;
public class DefaultAutoCompleteViewModel
{
    public string code { get; set; }
    public string key { get; set; }
    public string value { get; set; }
    public string title { get; set; }
    public string name { get; set; }
}

public class SparepartsAutoCompleteViewModel
{
    public string value { get; set; }
    public string title { get; set; }
    public string name { get; set; }
    public int quantity { get; set; }
    public decimal price { get; set; }
    public string type { get; set; }
}