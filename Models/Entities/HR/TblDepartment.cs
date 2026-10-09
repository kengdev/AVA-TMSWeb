using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblDepartment
{
    public string DeptCode { get; set; } = null!;

    public string DeptName { get; set; } = null!;

    public virtual ICollection<TblEmployeeDepartment> TblEmployeeDepartments { get; set; } = new List<TblEmployeeDepartment>();
}
