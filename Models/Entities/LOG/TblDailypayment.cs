using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblDailypayment
{
    public string DailypaymentCode { get; set; } = null!;

    public DateOnly PaymentDate { get; set; }

    public decimal NetTotal { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? Remark { get; set; }

    public string ApproveStatus { get; set; } = null!;

    public bool IsPaid { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblDailypaymentDetail> TblDailypaymentDetails { get; set; } = new List<TblDailypaymentDetail>();

    public virtual ICollection<TblBudgetTransaction> TransactionCodes { get; set; } = new List<TblBudgetTransaction>();
}
