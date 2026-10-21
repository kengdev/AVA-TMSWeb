using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblReceivePaymentReceipt
{
    public string ReceiptCode { get; set; } = null!;

    public string ReceivePaymentCode { get; set; } = null!;

    public DateOnly ReceiptDate { get; set; }

    public string PaymentType { get; set; } = null!;

    public string? BankName { get; set; }

    public string? BankBranchName { get; set; }

    public string? ChequeNo { get; set; }

    public string? Remark { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public virtual TblReceivePayment ReceivePaymentCodeNavigation { get; set; } = null!;
}
