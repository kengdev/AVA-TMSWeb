using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewTransportBudgetTransaction
{
    public string BudgetCode { get; set; } = null!;

    public string BudgetType { get; set; } = null!;

    public string TransactionCode { get; set; } = null!;

    public DateTime TransactionDate { get; set; }

    public decimal Amount { get; set; }

    public decimal AmountDiff { get; set; }

    public string? Remark { get; set; }

    public string? JobDetailCode { get; set; }

    public string? Start { get; set; }

    public string? Mission { get; set; }

    public string? CustomerAcronym { get; set; }

    public string? JobType { get; set; }

    public string TransactionType { get; set; } = null!;

    public string PaymentType { get; set; } = null!;

    public string? BookingNo { get; set; }

    public string? ContainerNo { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;
}
