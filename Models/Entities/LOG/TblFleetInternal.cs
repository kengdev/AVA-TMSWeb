using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblFleetInternal
{
    public string SupplierCode { get; set; } = null!;

    public string FleetCode { get; set; } = null!;

    public string FleetTypeCode { get; set; } = null!;

    public virtual TblFleet FleetCodeNavigation { get; set; } = null!;

    public virtual TblSupplier SupplierCodeNavigation { get; set; } = null!;
}
