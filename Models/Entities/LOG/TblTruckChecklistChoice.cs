using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTruckChecklistChoice
{
    public string TruckCheckCode { get; set; } = null!;

    public string TruckCheckGroup { get; set; } = null!;

    public string TruckCheckName { get; set; } = null!;

    public string TrcukCheckStandard { get; set; } = null!;

    public int TruckCheckIndex { get; set; }

    public virtual ICollection<TblTruckChecklistDetail> TblTruckChecklistDetails { get; set; } = new List<TblTruckChecklistDetail>();
}
