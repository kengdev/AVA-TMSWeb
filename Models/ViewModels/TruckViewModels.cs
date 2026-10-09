namespace TMSWeb.Models.ViewModels;
public class TruckViewModel
{
    public string truck_code { get; set; }
    public string truck_license { get; set; }
    public string truck_chassis_number { get; set; }
    public string truck_type_name { get; set; }
    public string fuel_type_name { get; set; }
    public string brand_name { get; set; }
    public string supplier_code { get; set; }
    public string supplier_name { get; set; }
    public string truck_no { get; set; }
    public string driver_code { get; set; }
    public string driver_name { get; set; }
    public string driver_phone { get; set; }
    public bool is_actived { get; set; }
}