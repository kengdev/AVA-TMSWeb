using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewGeneralBudgetTransaction
{
    public string BudgetCode { get; set; } = null!;

    public string BudgetType { get; set; } = null!;

    public DateTime TransactionDate { get; set; }

    public string? DailypaymentCode { get; set; }

    public decimal Amount { get; set; }

    public decimal AmountDiff { get; set; }

    public string? Remark { get; set; }

    public string TransactionType { get; set; } = null!;

    public string PaymentType { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public string? EmpFullname { get; set; }

    public string? ApproveStatus { get; set; }

    public string TransactionCode { get; set; } = null!;
}
