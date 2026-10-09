namespace TMSWeb.Models;
public class PlaceModel
{
    public string place_code { get; set; }
    public string place_port_code { get; set; }
    public string place_tax { get; set; }
    public string place_fullname { get; set; }
    public string place_name { get; set; }
    public string place_name_th { get; set; }
    public string place_address { get; set; }
    public string zone_code { get; set; }
    public string zone_name { get; set; }
    public string province_code { get; set; }
    public string place_lat { get; set; }
    public string place_lon { get; set; }
    public string place_remark { get; set; }
    public bool is_actived { get; set; }
    public bool is_enabled { get; set; }

    public decimal expressway_value { get; set; }
    public string create_by { get; set; }
    public string update_by { get; set; }
}