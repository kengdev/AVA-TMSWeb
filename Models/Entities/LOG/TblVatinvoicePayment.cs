using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblVatinvoicePayment
{
    public string VatinvoiceCode { get; set; } = null!;

    public string PaymentCode { get; set; } = null!;

    public virtual TblVatinvoice VatinvoiceCodeNavigation { get; set; } = null!;
}
