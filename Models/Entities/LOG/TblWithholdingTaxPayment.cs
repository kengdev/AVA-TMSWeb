using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblWithholdingTaxPayment
{
    public string WithholdingTaxCode { get; set; } = null!;

    public string MonthlyPaymentCode { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public virtual TblMonthlypayment MonthlyPaymentCodeNavigation { get; set; } = null!;

    public virtual TblWithholdingTax WithholdingTaxCodeNavigation { get; set; } = null!;
}
