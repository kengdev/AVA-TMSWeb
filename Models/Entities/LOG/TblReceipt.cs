using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblReceipt
{
    public string ReceiptCode { get; set; } = null!;

    public string ReceiptPartCode { get; set; } = null!;

    public string ReceiptName { get; set; } = null!;

    public string CustomerCode { get; set; } = null!;

    public string CustomerTax { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string CustomerAddress { get; set; } = null!;

    public int ReceiptAmount { get; set; }

    public decimal ReceiptAdvance { get; set; }

    public decimal ReceiptTransport { get; set; }

    public decimal ReceiptTotal { get; set; }

    public decimal ReceiptNetTotal { get; set; }

    public string? ReceiptRemark { get; set; }

    public DateOnly ReceiptDate { get; set; }

    public string ReceiptType { get; set; } = null!;

    public string? BankName { get; set; }

    public string? BankBranchName { get; set; }

    public string? ChequeNo { get; set; }

    public bool IsExcharge { get; set; }

    public string? ExchargeTopic { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblCustomer CustomerCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblReceiptDetail> TblReceiptDetails { get; set; } = new List<TblReceiptDetail>();
}
