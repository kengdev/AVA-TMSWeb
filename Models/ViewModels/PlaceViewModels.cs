namespace TMSWeb.Models.ViewModels;

public class PlaceViewModel
{
    public string place_code { get; set; }
    public string place_name { get; set; }
    public string zone_name { get; set; }
    public string place_port_code { get; set; }
    public string place_address { get; set; }
    public string place_tax { get; set; }
    public string place_type { get; set; }
    public bool is_actived { get; set; }
}
/*public class PlaceViewModel
{
    public string txtCustomerName { get; set; }
    public string acronym { get; set; }
    public string flooracronym { get; set; }
    public string txtStartCode { get; set; }
    public string StartName { get; set; }
    public string txtMissionCode { get; set; }
    public string MissionName { get; set; }
    public string txtMissionType { get; set; }
    public string txtStartType { get; set; }
    public string txtFloorStartCode { get; set; }
    public string txtFloorMissionCode { get; set; }
    public string txtFinishCode { get; set; }
    public string FinishName { get; set; }
    public decimal expressway { get; set; }
    public bool chkStartPortYard { get; set; } = false;
    public bool chkFinishPortYard { get; set; } = false;
}

*/
