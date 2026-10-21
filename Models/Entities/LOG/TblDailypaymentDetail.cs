using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblDailypaymentDetail
{
    public int Id { get; set; }

    public DateOnly PaymentOrderDate { get; set; }

    public string Description { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Amount { get; set; }

    public string DailypaymentCode { get; set; } = null!;

    public virtual TblDailypayment DailypaymentCodeNavigation { get; set; } = null!;
}
