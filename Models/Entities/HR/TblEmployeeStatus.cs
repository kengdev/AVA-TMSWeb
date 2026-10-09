using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblEmployeeStatus
{
    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public virtual ICollection<TblEmployee> TblEmployees { get; set; } = new List<TblEmployee>();
}
