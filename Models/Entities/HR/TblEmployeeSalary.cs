using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblEmployeeSalary
{
    public string SalaryCode { get; set; } = null!;

    public string EmpCode { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string SalaryTypeCode { get; set; } = null!;

    public decimal SalaryValue { get; set; }

    public string SalaryDetail { get; set; } = null!;

    public DateOnly SalaryDate { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;

    public virtual TblSalaryType SalaryTypeCodeNavigation { get; set; } = null!;
}
