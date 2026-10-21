using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTruckTypegroup
{
    public string TruckTypegroupCode { get; set; } = null!;

    public string TruckTypegroupName { get; set; } = null!;

    public virtual ICollection<TblFuelRate> TblFuelRates { get; set; } = new List<TblFuelRate>();

    public virtual ICollection<TblTruckType> TblTruckTypes { get; set; } = new List<TblTruckType>();
}
