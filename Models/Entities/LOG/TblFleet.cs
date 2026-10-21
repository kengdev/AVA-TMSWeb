using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblFleet
{
    public string FleetCode { get; set; } = null!;

    public string FleetName { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblFleetInternal> TblFleetInternals { get; set; } = new List<TblFleetInternal>();

    public virtual ICollection<TblFleetRate> TblFleetRates { get; set; } = new List<TblFleetRate>();
}
