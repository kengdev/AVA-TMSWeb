using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTruckChecklist
{
    public string ChecklistCode { get; set; } = null!;

    public string DriverCode { get; set; } = null!;

    public string TruckCode { get; set; } = null!;

    public DateOnly ChecklistDate { get; set; }

    public virtual ICollection<TblTruckChecklistDetail> TblTruckChecklistDetails { get; set; } = new List<TblTruckChecklistDetail>();

    public virtual TblTruck TruckCodeNavigation { get; set; } = null!;
}
