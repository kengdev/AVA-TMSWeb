using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblEmployeeDepartment
{
    public string EmpCode { get; set; } = null!;

    public string DeptCode { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public virtual TblDepartment DeptCodeNavigation { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;
}
