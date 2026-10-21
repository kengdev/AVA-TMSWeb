using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblReceivePayment
{
    public string ReceivePaymentCode { get; set; } = null!;

    public DateOnly PaymentDate { get; set; }

    public DateOnly DueDate { get; set; }

    public string PaymentTypeCode { get; set; } = null!;

    public string PaymentTopic { get; set; } = null!;

    public decimal Total { get; set; }

    public bool IsVat { get; set; }

    public bool IsTax { get; set; }

    public decimal Vat { get; set; }

    public decimal Tax { get; set; }

    public decimal NetTotal { get; set; }

    public string SupplierCode { get; set; } = null!;

    public string? RefDoc { get; set; }

    public string? Remark { get; set; }

    public bool IsPaid { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblSupplier SupplierCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblReceivePaymentDetail> TblReceivePaymentDetails { get; set; } = new List<TblReceivePaymentDetail>();

    public virtual ICollection<TblReceivePaymentReceipt> TblReceivePaymentReceipts { get; set; } = new List<TblReceivePaymentReceipt>();
}
