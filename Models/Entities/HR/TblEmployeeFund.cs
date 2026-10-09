using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblEmployeeFund
{
    public string EmpCode { get; set; } = null!;

    public decimal SsoDeductValue { get; set; }

    public decimal SsoBalanceValue { get; set; }

    public decimal SecureDeductValue { get; set; }

    public decimal SecureBalanceValue { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;
}
