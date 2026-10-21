using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblMonthlypaymentDetail
{
    public int Id { get; set; }

    public DateOnly PaymentDocDate { get; set; }

    public string PaymentDocNo { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Amount { get; set; }

    public string MonthlyPaymentCode { get; set; } = null!;

    public virtual TblMonthlypayment MonthlyPaymentCodeNavigation { get; set; } = null!;
}
