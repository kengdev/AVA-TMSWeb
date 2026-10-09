using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace TMSWeb.Models.ViewModels;
public class JobViewModel
{
    public string job_code { get; set; }
    public string job_detail_code { get; set; }
    public string job_cashing_code { get; set; }
    public string job_type { get; set; }
    public string customer_name { get; set; }
    public string cost_name { get; set; }
    public string booking_no { get; set; }
    public string container_type { get; set; }
    public string container_size { get; set; }
    public string truck_license { get; set; }
    public string trailer_license { get; set; }
    public string driver_fullname { get; set; }
    public string driver_phone { get; set;  }
    public string container_no { get; set; }
    public string cashing_remark { get; set; }
    public string start_name { get; set; }
    public string mission_name { get; set; }
    public string job_date { get; set; }
    public DateTime job_date_value { get; set; }
    public bool? is_document { get; set; }
    public bool? is_cashing { get; set; }
    public bool? is_billing { get; set; }
    public bool? is_approve { get; set; }
    public bool is_advance_paid { get; set; }
    public bool is_advance_prove { get; set; }
    public bool is_weeklypayment { get; set; }
    public string supplier_code { get; set; }
    public string supplier_name { get; set; }
    public string job_status { get; set; }
    public string tracking_status { get; set; }
    public string seal_no { get; set; }
    public string job_invoice { get; set; }
    public decimal? cashing_total { get; set; }
    public decimal? cost_value { get; set; }
    public decimal? advance_value { get; set; }
    public decimal transport_value { get; set; }
    public decimal express_value { get; set; }
    public decimal trip_value { get; set; }
    public int fuel_litre { get; set; }
    public decimal fuel_value { get; set; }
    public decimal payment_value { get; set; }
    public string hash_code { get; set; }
    public decimal tare_weight { get; set;  }
    public string job_remark { get; set;  }
}

public class OrderStatusViewModel
{
    public string customer_code { get; set; }
    public string customer_name { get; set; }
    public string customer_address { get; set; }
    public string customer_tax { get; set; }
    public string truck_license { get; set; }
    public string trailer_license { get; set; }
    public string truck_typegroup_code { get; set; }
    public string driver_code { get; set; }
    public string driver_fullname { get; set; }
    public string job_code { get; set; }
    public string job_detail_code { get; set; }
    public string job_type { get; set; }
    public string container_size_code { get; set; }
    public string container_size { get; set; }
    public string port_no { get; set; }
    public string industry { get; set; }
    public int container_amount { get; set; }
    public decimal tare { get; set; }
    public string mission_code { get; set; }
    public string mission_name { get; set; }
    public string zone_name { get; set; }
    public string receipt_name { get; set; }
    public string receipt_address { get; set; }
    public string receipt_tax { get; set; }

    public string mission_vat { get; set; }
    public string mission_address { get; set; }
    public string booking_no { get; set; }
    public string agent_code { get; set; }
    public string start_name { get; set; }
    public string finish_name { get; set; }
    public string container_no { get; set; }
    public string seal_no { get; set; }
    public DateTime job_date { get; set; }
    public string closing_time { get; set; }
    public string job_invoice { get; set; }
    public string job_remark { get; set; }
    public decimal fuel_rate { get; set; }
    public decimal lift { get; set; }
    public decimal port { get; set; }
    public decimal yard { get; set; }
    public decimal express_way { get; set; }
    public string barcode { get; set; }
    public bool is2short { get; set; }
    public string supplier_code { get; set; }
    public string tracking_status { get; set; }
}