using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblReceiptDetail
{
    public int Id { get; set; }

    public string BillingCode { get; set; } = null!;

    public string BillingType { get; set; } = null!;

    public decimal ReceiptAdvanceValue { get; set; }

    public decimal ReceiptTransportValue { get; set; }

    public decimal ReceiptValue { get; set; }

    public string ReceiptCode { get; set; } = null!;

    public virtual TblBillingMaster BillingCodeNavigation { get; set; } = null!;

    public virtual TblReceipt ReceiptCodeNavigation { get; set; } = null!;
}
