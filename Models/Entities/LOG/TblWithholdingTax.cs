using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblWithholdingTax
{
    public string WithholdingTaxCode { get; set; } = null!;

    public DateOnly WithholdingTaxDate { get; set; }

    public string PaymentTypeCode { get; set; } = null!;

    public string SupplierCode { get; set; } = null!;

    public decimal PaymentTotal { get; set; }

    public decimal TaxTotal { get; set; }

    public decimal PaymentNetTotal { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblPaymentType PaymentTypeCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblWithholdingTaxPayment> TblWithholdingTaxPayments { get; set; } = new List<TblWithholdingTaxPayment>();

    public virtual ICollection<TblWithholdingTaxSupplierpayment> TblWithholdingTaxSupplierpayments { get; set; } = new List<TblWithholdingTaxSupplierpayment>();
}
