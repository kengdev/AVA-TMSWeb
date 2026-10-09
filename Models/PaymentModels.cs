using System;
using System.Collections.Generic;

namespace TMSWeb.Models;

public class PaymentModel
{
    public bool payment_status { get; set; }
    public string payment_id { get; set; }
    public string payment_date { get; set; }
    public string payment_name { get; set; }
    public string payment_type { get; set; }
    public decimal total { get; set; }
    public string create_by { get; set; }
    public string approve_status { get; set; }
}

public class DriverPaymentModel
{
    public string driver_code { get; set; }
    public string allowance_code { get; set; }
    public List<int> allowance_detail_id { get; set; }
    public List<string> subtractType { get; set; }
    public List<string> subtractName { get; set; }
    public List<decimal> subtractVal { get; set; }
    public List<decimal> trip { get; set; }
    public List<decimal> fuel { get; set; }
    public List<decimal> waste { get; set; }
    public List<decimal> other { get; set; }
}