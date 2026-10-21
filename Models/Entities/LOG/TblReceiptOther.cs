using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblReceiptOther
{
    public string ReceiptOtherCode { get; set; } = null!;

    public string ReceiptOtherType { get; set; } = null!;

    public string CustomerTax { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string CustomerAddress { get; set; } = null!;

    public decimal Total { get; set; }

    public decimal Vat { get; set; }

    public decimal NetTotal { get; set; }

    public DateOnly ReceiptDate { get; set; }

    public string? PaymentType { get; set; }

    public string? BankName { get; set; }

    public string? BankBranchName { get; set; }

    public string? ChequeNo { get; set; }

    public DateOnly? ChequeDate { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblReceiptType ReceiptOtherTypeNavigation { get; set; } = null!;

    public virtual ICollection<TblReceiptOtherDetail> TblReceiptOtherDetails { get; set; } = new List<TblReceiptOtherDetail>();
}
