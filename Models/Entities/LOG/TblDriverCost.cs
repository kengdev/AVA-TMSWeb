using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblDriverCost
{
    public int Id { get; set; }

    public string DriverCode { get; set; } = null!;

    public string JobDetailCode { get; set; } = null!;

    public decimal TripValue { get; set; }

    public decimal FuelValue { get; set; }

    public decimal WasteValue { get; set; }

    public decimal ExpresswayValue { get; set; }

    public decimal OtherValue { get; set; }

    public string CostType { get; set; } = null!;

    public bool IsWeeklypayment { get; set; }

    public virtual TblDriver DriverCodeNavigation { get; set; } = null!;

    public virtual TblJobDetail JobDetailCodeNavigation { get; set; } = null!;
}
