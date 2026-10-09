using TMSWeb.Models.ViewModels;
namespace TMSWeb.Models;
public class SalaryModel
{
    public string emp_code { get; set; }
    public string month { get; set; }
    public string year { get; set; }
    public decimal salary_incoming { get; set; }
    public decimal diligence_incoming { get; set; }
    public decimal sso_dedict { get; set; }
    public decimal late_dedict { get; set; }
    public int late { get; set; }
    public decimal absent_dedict { get; set; }

    public decimal secure_dedict { get; set; }
    public decimal secure_balance { get; set; }
    public decimal secure_total { get; set; }

    public decimal loan_dedict { get; set; }

    public decimal vat { get; set; }
    public decimal total_income { get; set; }
    public decimal total_dedict { get; set; }
    public decimal net_total { get; set; }
    public string create_by { get; set; }
    public DateTime start { get; set; }
    public DateTime end { get; set; }
    public AttendanceSummaryViewModel attendance { get; set; }
    public List<TripValueModel> trip_value { get; set; }
    public List<AdditionValueModel> addition_value { get; set; }
}


public class TripValueModel
{
    public string trip { get; set; }
    public decimal trip_value { get; set; }
}

public class AdditionValueModel
{
    public string[] additionName { get; set; }
    public string[] additionType { get; set; }
    public decimal[] additionValue { get; set; }
}