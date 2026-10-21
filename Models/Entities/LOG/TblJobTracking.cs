using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblJobTracking
{
    public int Id { get; set; }

    public string JobDetailCode { get; set; } = null!;

    public string TrackingStatus { get; set; } = null!;

    public DateTime TrackingDate { get; set; }

    public string? Lattitude { get; set; }

    public string? Longitude { get; set; }

    public string? Speed { get; set; }

    public virtual TblJobDetail JobDetailCodeNavigation { get; set; } = null!;
}
