namespace TMSWeb.Models.ViewModels;
public class RouteViewModel
{
    public string code { get; set; }
    public string name { get; set; }
    public bool is_actived { get; set; }
}

public class RouteDetailViewModel
{
    public string route_code { get; set; }
    public List<RouteStartViewModel> start { get; set; }
    public List<RouteMissionViewModel> mission { get; set; }
}

public class RouteStartViewModel
{
    public string start_code { get; set; }
    public string start_name { get; set; }
}

public class RouteMissionViewModel
{
    public string mission_code { get; set; }
    public string mission_name { get; set; }
}