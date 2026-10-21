using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblFuelRate
{
    public string ZoneCode { get; set; } = null!;

    public string TruckTypegroupCode { get; set; } = null!;

    public int FuelRateValue { get; set; }

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblTruckTypegroup TruckTypegroupCodeNavigation { get; set; } = null!;

    public virtual TblZone ZoneCodeNavigation { get; set; } = null!;
}
