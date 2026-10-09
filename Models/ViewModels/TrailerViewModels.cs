using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMSWeb.Models.ViewModels;
public class TrailerViewModel
{
    public string trailer_code { get; set; }
    public string trailer_license { get; set; }
    public string trailer_no { get; set; }
    public string trailer_type_name { get; set; }
    public string trailer_floor_name { get; set; }
    public string truck_license { get; set; }
    public bool is_actived { get; set; }
}