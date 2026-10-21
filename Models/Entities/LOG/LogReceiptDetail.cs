using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class LogReceiptDetail
{
    public int Id { get; set; }

    public string BillingCode { get; set; } = null!;

    public decimal ReceiptAdvanceValue { get; set; }

    public decimal ReceiptTransportValue { get; set; }

    public decimal ReceiptValue { get; set; }

    public string ReceiptCode { get; set; } = null!;

    public string Action { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;
}
