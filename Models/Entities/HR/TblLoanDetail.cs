using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblLoanDetail
{
    public int Id { get; set; }

    public decimal DedictValue { get; set; }

    public string Remark { get; set; } = null!;

    public string LoanCode { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public virtual TblLoan LoanCodeNavigation { get; set; } = null!;
}
