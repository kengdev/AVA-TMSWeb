using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace TMSWeb.Models;
public class FinishJobModel
{
    public string job_detail_code { get; set; }
    public string container_no { get; set; }
    public string seal_no { get; set; }
    public int tare_weight { get; set; }
    public decimal cost_value { get; set; }
    public decimal supplier_value { get; set; }
    public decimal shipping_advance { get; set; }
    public string[] nameCashing { get; set; }
    public decimal[] valWithdrawCashing { get; set; }
    public decimal[] valCashing { get; set; }
    public string[] nameAdvance { get; set; }
    public decimal[] valWithdrawAdvance { get; set; }
    public decimal[] valAdvance { get; set; }
    public decimal[] valSupplier { get; set; }
    public string[] cost_type { get; set; }
    public string[] driver_code { get; set; }
    public decimal[] driver_value { get; set; }
    public bool is_document { get; set; }
    public string username { get; set; }
    public string emp_code { get; set; }
    public string supplier_invoice { get; set; }
    public bool is_save { get; set; }
}

public class UpdateStatusModel
{
    public string job_detail_code { get; set; }
    public string status { get; set; }
    public DateTime status_stamp { get; set; }
    public string create_by { get; set; }
}

public class UpdateDataModel
{
    public DateTime job_date { get; set; }
    public string job_detail_code { get; set; }
    public string truck_code { get; set; }
    public string new_driver_code { get; set; }
    public string container_no { get; set; }
    public string seal_no { get; set; }
    public int tare_weight { get; set; }
    public string container_size { get; set; }
    public string container_size_code { get; set; }
    public string job_type { get; set; }
    public string supplier_code { get; set; }
    public string tracking_status { get; set; }
    public string trailer_license { get; set; }
    public string truck_license { get; set; }
    public string finish_place_code { get; set; }
    public string finish_name { get; set; }
}

public class ChangeDriverModel
{
    public string job_detail_code { get; set; }
    public string mission_code { get; set; }
    public string driver_code { get; set; }
    public string truck_code { get; set; }
    public string supplier_code { get; set; }
    public string new_driver_code { get; set; }
    public string new_truck_code { get; set; }
    public string new_supplier_code { get; set; }
    public string trailer_license { get; set; }
    public decimal driver_cost { get; set; }
    public decimal fuel { get; set; }
}

public class CancelJobModel
{
    public string job_detail_code { get; set; }
    public string driver_code { get; set; }
    public decimal trip_cost { get; set; }
    public decimal driver_cost { get; set; }
    public string create_by { get; set; }
}