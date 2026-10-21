using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblDriverSpecialvalue
{
    public int Id { get; set; }

    public string SpecialvalueType { get; set; } = null!;

    public string SpecialvalueName { get; set; } = null!;

    public decimal Specialvalue { get; set; }

    public string DriverCode { get; set; } = null!;

    public string AllowanceCode { get; set; } = null!;

    public virtual TblAllowance AllowanceCodeNavigation { get; set; } = null!;

    public virtual TblDriver DriverCodeNavigation { get; set; } = null!;
}
