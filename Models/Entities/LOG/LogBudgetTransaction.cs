using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class LogBudgetTransaction
{
    public int Id { get; set; }

    public string JobDetailCode { get; set; } = null!;

    public string TransactionCode { get; set; } = null!;

    public string TransactionName { get; set; } = null!;

    public string PaymentType { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public virtual TblJobDetail JobDetailCodeNavigation { get; set; } = null!;
}
