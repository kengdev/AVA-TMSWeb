namespace TMSWeb.Models.ViewModels;
public class CheckListViewModel
{
    public string checklist_code { get; set; }
    public string driver_code { get; set; }
    public string driver_name { get; set; }
    public string truck_license { get; set; }
    public string truck_no { get; set; }
    public DateOnly checklist_date { get; set; }
    public List<CheckListDetailViewModel> detail { get; set; }
}

public class CheckListDetailViewModel
{
    public string code { get; set; }
    public string name { get; set; }
    public bool value { get; set; }
    public string issued { get; set; }
    public string list_group { get; set; }
}