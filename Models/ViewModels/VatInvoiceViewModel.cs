using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMSWeb.Models;
public class VatInvoiceViewModel
{
    public string code { get; set; }
    public string create_date { get; set; }
    public string fleet_type { get; set; }
    public string payment_name { get; set; }
    public string license { get; set; }
    public string truck_no { get; set; }
    public string supplier_name { get; set; }
    public string payment_total { get; set; }
    public decimal vat_total { get; set; }
    public decimal net_total { get; set; }
    public string remark { get; set; }
}