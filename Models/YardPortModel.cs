using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMSWeb.Models;
public class PortModel
{
    public string place_code { get; set; }
    public string place_name { get; set; }
    public string place_type { get; set; }
    public string place_remark { get; set; }
    public decimal export_fee { get; set; }
    public decimal import_fee { get; set; }
    public string[] container_size { get; set; }
    public decimal[] container_fee { get; set; }
    public bool is_enabled { get; set; }
    public bool is_actived { get; set; }
    public string create_by { get; set; }
    public string update_by { get; set; }
}

public class PortCashingModel
{
    public string container_kind_code { get; set; }
    public string container_kind_name { get; set; }
    public decimal? container_fee { get; set; }
    public string container_group { get; set; }
}

public class YardModel
{
    public string place_code { get; set; }
    public string place_name { get; set; }
    public string place_type { get; set; }
    public string place_remark { get; set; }
    public decimal shortlifton { get; set; }
    public decimal longlifton { get; set; }
    public decimal shortliftoff { get; set; }
    public decimal longliftoff { get; set; }
    public decimal export_fee { get; set; }
    public decimal import_fee { get; set; }

    public string create_by { get; set; }
    public string update_by { get; set; }
}