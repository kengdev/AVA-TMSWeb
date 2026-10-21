using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewTrailerListActive
{
    public string TrailerNo { get; set; } = null!;

    public string TrailerLicense { get; set; } = null!;

    public string TrailerTypeName { get; set; } = null!;

    public string TrailerFloorName { get; set; } = null!;

    public string TrailerCode { get; set; } = null!;

    public string TruckLicense { get; set; } = null!;

    public bool IsActived { get; set; }
}
