using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblEmployeeResign
{
    public string EmpCode { get; set; } = null!;

    public string ResigntypeCode { get; set; } = null!;

    public DateOnly ResignDate { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public string? ResignReason { get; set; }

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;

    public virtual TblResignType ResigntypeCodeNavigation { get; set; } = null!;
}
