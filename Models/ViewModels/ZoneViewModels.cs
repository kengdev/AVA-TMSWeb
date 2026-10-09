using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMSWeb.Models.ViewModels;
/*public class ZoneRateModel
{
    public string zone_code { get; set; }
    public string zone_name { get; set; }
    public decimal tr4 { get; set; }
    public decimal tr6 { get; set; }
    public decimal tr10 { get; set; }
}*/

public class ZoneViewModel
{
    public string zone_code { get; set; }
    public string zone_name { get; set; }
    public decimal tr4 { get; set; }
    public decimal tr6 { get; set; }
    public decimal tr10 { get; set; }
    public bool is_actived { get; set; }
}