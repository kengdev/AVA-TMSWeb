using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblAllowanceDetail
{
    public int Id { get; set; }

    public DateOnly AllowanceDate { get; set; }

    public string TripType { get; set; } = null!;

    public decimal TripValue { get; set; }

    public decimal FuelValue { get; set; }

    public decimal WasteValue { get; set; }

    public decimal OtherValue { get; set; }

    public string? JobDetailCode { get; set; }

    public string AllowanceCode { get; set; } = null!;

    public virtual TblAllowance AllowanceCodeNavigation { get; set; } = null!;
}
