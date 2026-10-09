namespace TMSWeb.Models;
public class RouteModel
{
    public string route_code { get; set; }
    public List<RoutePlaceModel> start { get; set; }
    public List<RoutePlaceModel> mission { get; set; }
}

public class RoutePlaceModel
{
    public string place_code { get; set; }
    public string place_name { get; set; }

}