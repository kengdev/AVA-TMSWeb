using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblSalaryType
{
    public string SalaryTypeCode { get; set; } = null!;

    public string SalaryTypeName { get; set; } = null!;

    public virtual ICollection<TblEmployeeSalary> TblEmployeeSalaries { get; set; } = new List<TblEmployeeSalary>();
}
