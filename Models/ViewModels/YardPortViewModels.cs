namespace TMSWeb.Models.ViewModels;
public class YardPortViewModel
{
    public string yardport_code { get; set; }
    public string yardport_name { get; set; }
    public string yardport_type { get; set; }
    public decimal import_fee { get; set; }
    public decimal export_fee { get; set; }
    public bool is_actived { get; set; }
}

public class PortViewModel
{
    public string yardport_code { get; set; }
    public string container_size { get; set; }
    public string container_kind_code { get; set; }
    public decimal container_fee { get; set; }
    public string container_group { get; set; }
    public bool is_actived { get; set; }
}