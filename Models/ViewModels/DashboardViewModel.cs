using System;
using System.Collections.Generic;
using TMSWeb.Models.Entities.LOG;

namespace TMSWeb.Models.ViewModels;
public class SummaryDetailViewModel
{
    public string year { get; set; }
    public string year_name { get; set; }
    public List<GraphViewModel> data { get; set; }
    public List<GraphViewModel> other { get; set; }
    public List<GraphViewModel> advance { get; set; }
}

public class GraphViewModel
{
    public decimal id_sum { get; set; }
    public decimal other_sum { get; set; }
    public decimal cost_value { get; set; }
    public int id_count { get; set; }
    public string month { get; set; }
    public string year { get; set; }
    public string date { get; set; }
    public int day { get; set; }
    public string name { get; set; }
    public string code { get; set; }
}

public class DashboardHistoryViewModel
{
    public string month_2 { get; set; }
    public string month_3 { get; set; }
    public string month_4 { get; set; }
    public string month_5 { get; set; }
    public string month_6 { get; set; }

    public double net_value2 { get; set; }
    public double net_value3 { get; set; }
    public double net_value4 { get; set; }
    public double net_value5 { get; set; }
    public double net_value6 { get; set; }

    public double bill_value2 { get; set; }
    public double bill_value3 { get; set; }
    public double bill_value4 { get; set; }
    public double bill_value5 { get; set; }
    public double bill_value6 { get; set; }

    public double unbill_value2 { get; set; }
    public double unbill_value3 { get; set; }
    public double unbill_value4 { get; set; }
    public double unbill_value5 { get; set; }
    public double unbill_value6 { get; set; }

    public double unpay_value2 { get; set; }
    public double unpay_value3 { get; set; }
    public double unpay_value4 { get; set; }
    public double unpay_value5 { get; set; }
    public double unpay_value6 { get; set; }
}

public class DashboardCounterViewModel
{
    public int total_job { get; set; }
    public int total_fleet { get; set; }
    public int total_supplyer { get; set; }
    public int total_pending { get; set; }
    public double sum_netvalue { get; set; }
    public double sum_billingvalue { get; set; }
    public double sum_unbillingvalue { get; set; }
    public double sum_uncashingvalue { get; set; }
}

public class TruckWorkingViewModel
{
    public string driver_code { get; set; }
    public string truck_code { get; set; }
    public string truck_license { get; set; }
    public string driver_fullname { get; set; }
    public decimal trip { get; set; }
    public decimal fuel { get; set; }
    public decimal expressway { get; set; }
}

public class NotificationViewModel
{
    public int days { get; set; }
    public int hours { get; set; }
    public int minutes { get; set; }
    public string container_no { get; set; }
    public string customer { get; set; }
}

public class WorkingDetailViewModel
{
    public string truck_license { get; set; }
    public string driver_fullname { get; set; }
    public decimal trip { get; set; }
    public decimal fuel { get; set; }
    public decimal expressway { get; set; }
    public DateTime job_date { get; set; }
    public string container_no { get; set; }
    public string booking_no { get; set; }
    public string job_invoice { get; set; }
    public string place { get; set; }
}

public class JobLoadPlaceViewModel
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

public class MonthlyOrderChartDto
{
    public List<string> Labels { get; set; } = [];
    public List<MonthlyOrderSeriesDto> Series { get; set; } = [];
}

public class MonthlyOrderSeriesDto
{
    public int Year { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<int> Data { get; set; } = [];
}