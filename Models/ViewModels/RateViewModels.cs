namespace TMSWeb.Models.ViewModels;
public class DriverRateViewModel
{
    public string rate_code { get; set; }
    public string name { get; set; }
    public string zone_name { get; set; }
    public decimal rate_value { get; set; }
}

public class SupplyerRateViewModel
{
    public string supplyer_code { get; set; }
    public string place_code { get; set; }
    public string place_name { get; set; }
    public string supplyer_name { get; set; }
    public string zone_name { get; set; }
    public string truck_typegroup_name { get; set; }
    public string truck_typegroup_code { get; set; }
    public decimal rate_4W_value { get; set; }
    public decimal rate_6W_value { get; set; }
    public decimal rate_10W_value { get; set; }
    public decimal rate_sup_value { get; set; }
}

public class FuelRateViewModel
{
    public FuelRateViewModel()
    {
        values = new List<RateValueViewModel>();
    }

    public string zone_code { get; set; }
    public string zone_name { get; set; }
    public List<RateValueViewModel> values { get; set; }
}

public class RateValueViewModel
{
    public int id { get; set; }
    public bool is_enabled { get; set; }
    public string rate_code { get; set; }
    public decimal rate_value { get; set; }
}