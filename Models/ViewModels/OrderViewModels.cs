using TMSWeb.Models.Entities.LOG;

namespace TMSWeb.Models.ViewModels;
public class OrderPlaceViewModel
{
    public string place_code { get; set; }
    public string place_name { get; set; }
    public string zone_name { get; set; }
    public string place_port_code { get; set; }
    public string place_address { get; set; }
    public string place_tax { get; set; }
    public string place_type { get; set; }
}

public class PlaceCashingViewModel
{
    public decimal? short_get_fee { get; set; }
    public decimal? short_back_fee { get; set; }
    public decimal? long_get_fee { get; set; }
    public decimal? long_back_fee { get; set; }
    public decimal? yard_fee { get; set; }
    public int fuel_value { get; set; }
    public decimal expressway_value { get; set; }
}

public class JobLoadPlaceModel
{
    public DateTime job_date { get; set; }
    public string driver_fullname { get; set; }
    public string truck_license { get; set; }
    public string customer_name { get; set; }
    public string customer_code { get; set; }
    public string start_name { get; set; }
    public string mission_name { get; set; }
    public string finish_name { get; set; }
    public string container_no { get; set; }
    public string booking_no { get; set; }
    public string job_code { get; set; }
    public string tracking_status { get; set; }
    public decimal trip_cost { get; set; }
    public decimal advance_cost { get; set; }
    public string job_invoice { get; set; }
    public bool is_billing { get; set; }
    public List<TblJobadvanceOther> other_list { get; set; }
}