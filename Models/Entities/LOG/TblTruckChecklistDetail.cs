using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTruckChecklistDetail
{
    public int Id { get; set; }

    public string TruckCheckCode { get; set; } = null!;

    public bool TruckCheckValue { get; set; }

    public string TruckCheckFixed { get; set; } = null!;

    public string ChecklistCode { get; set; } = null!;

    public virtual TblTruckChecklist ChecklistCodeNavigation { get; set; } = null!;

    public virtual TblTruckChecklistChoice TruckCheckCodeNavigation { get; set; } = null!;
}
