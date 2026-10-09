namespace TMSWeb.Repositories;
public class AccountViewModel
{
    public string acc_username { get; set; }
    public string acc_firstname { get; set; }
    public string acc_lastname { get; set; }
    public string group_code { get; set; }
    public string group_name { get; set; }
}

public class ProfileViewModel
{
    public string usercode { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public string fullname { get; set; }
    public string phone { get; set; }
    public string mail { get; set; }
    public string dept_name { get; set; }
    public string pic { get; set; }
    public string web_theme { get; set; }
    public string group_code { get; set; }
    public string group_name { get; set; }
    public bool is_active { get; set; }
}