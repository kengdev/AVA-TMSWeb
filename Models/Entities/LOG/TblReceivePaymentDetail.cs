using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblReceivePaymentDetail
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public int Amount { get; set; }

    public string Unit { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal Total { get; set; }

    public string ReceivePaymentCode { get; set; } = null!;

    public virtual TblReceivePayment ReceivePaymentCodeNavigation { get; set; } = null!;
}
