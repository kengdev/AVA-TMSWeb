using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblFleetRate
{
    public string FleetCode { get; set; } = null!;

    public string PlaceCode { get; set; } = null!;

    public decimal Truck4w { get; set; }

    public decimal Truck6w { get; set; }

    public decimal Truck10w { get; set; }

    public virtual TblFleet FleetCodeNavigation { get; set; } = null!;

    public virtual TblPlace PlaceCodeNavigation { get; set; } = null!;
}
