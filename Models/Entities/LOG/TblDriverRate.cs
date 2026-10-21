using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblDriverRate
{
    public string PlaceCode { get; set; } = null!;

    public decimal RateValue { get; set; }
}
