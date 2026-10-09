namespace TMSWeb.Models.ViewModels;
public class StepViewModel
{
    public string action { get; set; }
    public string action_time { get; set; }
    public string active { get; set; }
    public string icon { get; set; }
    public string index { get; set; }
    public int column { get; set; }
}

public class StepWizardViewModels
{
    public string action { get; set; }
    public string action_time { get; set; }
    public string progress { get; set; }
    public string wizard { get; set; }
    public string icon { get; set; }
    public string index { get; set; }
}

public class TrackingModel
{
    public string job_detail_code { get; set; }
    public string tracking_status { get; set; }
    public DateTime tracking_date { get; set; }
}