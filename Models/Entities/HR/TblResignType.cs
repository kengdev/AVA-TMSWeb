using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblResignType
{
    public string ResigntypeCode { get; set; } = null!;

    public string ResigntypeName { get; set; } = null!;

    public virtual ICollection<TblEmployeeResign> TblEmployeeResigns { get; set; } = new List<TblEmployeeResign>();
}
