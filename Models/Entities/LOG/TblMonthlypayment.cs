using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblMonthlypayment
{
    public string MonthlyPaymentCode { get; set; } = null!;

    public DateOnly PaymentDate { get; set; }

    public string PaymentTypeCode { get; set; } = null!;

    public decimal Total { get; set; }

    public bool IsVat { get; set; }

    public bool IsTax { get; set; }

    public decimal Vat { get; set; }

    public decimal Tax { get; set; }

    public decimal NetTotal { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string SupplierCode { get; set; } = null!;

    public string? BankName { get; set; }

    public string? AccountNo { get; set; }

    public string? BankBranchName { get; set; }

    public string? ChequeNo { get; set; }

    public string? Remark { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblPaymentType PaymentTypeCodeNavigation { get; set; } = null!;

    public virtual TblSupplier SupplierCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblMonthlypaymentDetail> TblMonthlypaymentDetails { get; set; } = new List<TblMonthlypaymentDetail>();

    public virtual ICollection<TblWithholdingTaxPayment> TblWithholdingTaxPayments { get; set; } = new List<TblWithholdingTaxPayment>();
}
