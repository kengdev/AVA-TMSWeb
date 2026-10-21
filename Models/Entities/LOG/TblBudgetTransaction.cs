using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblBudgetTransaction
{
    public string TransactionCode { get; set; } = null!;

    public DateTime TransactionDate { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Balance { get; set; }

    public decimal Amount { get; set; }

    public decimal AmountDiff { get; set; }

    public string PaymentType { get; set; } = null!;

    public string? Remark { get; set; }

    public string BudgetCode { get; set; } = null!;

    public bool? IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime? UpdateDate { get; set; }

    public string? UpdateBy { get; set; }

    public virtual TblBudget BudgetCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblDailypayment> DailypaymentCodes { get; set; } = new List<TblDailypayment>();
}
