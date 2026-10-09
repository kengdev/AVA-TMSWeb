using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblLoan
{
    public string LoanCode { get; set; } = null!;

    public string EmpCode { get; set; } = null!;

    public decimal LoanTotalValue { get; set; }

    public decimal LoanBalanceValue { get; set; }

    public decimal LoanDedictValue { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblEmployee EmpCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblLoanDetail> TblLoanDetails { get; set; } = new List<TblLoanDetail>();
}
