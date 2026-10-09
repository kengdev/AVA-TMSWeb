namespace TMSWeb.Models;
public class PriviledgeLevelModel
{
    public string action_code { get; set; }
    public string level_01_code { get; set; }
    public string level_02_code { get; set; }
    public string title { get; set; }
    public bool is_enabled { get; set; }
}
public class UserPriviledgeModel
{
    public string priviledge_code { get; set; }
    public string priviledge_name { get; set; }
    public string priviledge_icon { get; set; }
    public string priviledge_link { get; set; }
    public string priviledge_active { get; set; }
    public List<UserPriviledgeModel> sub_level { get; set; }
}