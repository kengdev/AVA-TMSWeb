using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTruckDriver
{
    public int Id { get; set; }

    public string TruckCode { get; set; } = null!;

    public string DriverCode { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public virtual TblDriver DriverCodeNavigation { get; set; } = null!;

    public virtual TblTruck TruckCodeNavigation { get; set; } = null!;
}
