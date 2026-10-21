using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblPaymentType
{
    public string PaymentTypeCode { get; set; } = null!;

    public string PaymentTypeName { get; set; } = null!;

    public virtual ICollection<TblMonthlypayment> TblMonthlypayments { get; set; } = new List<TblMonthlypayment>();

    public virtual ICollection<TblWithholdingTax> TblWithholdingTaxes { get; set; } = new List<TblWithholdingTax>();
}
