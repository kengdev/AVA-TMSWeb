using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblZone
{
    public string ZoneCode { get; set; } = null!;

    public string ZoneName { get; set; } = null!;

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblFuelRate> TblFuelRates { get; set; } = new List<TblFuelRate>();

    public virtual ICollection<TblIndustry> TblIndustries { get; set; } = new List<TblIndustry>();
}
