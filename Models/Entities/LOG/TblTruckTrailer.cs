using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTruckTrailer
{
    public int Id { get; set; }

    public string TruckCode { get; set; } = null!;

    public string TrailerCode { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblTrailer TrailerCodeNavigation { get; set; } = null!;

    public virtual TblTruck TruckCodeNavigation { get; set; } = null!;
}
