using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblShippingpaymentDetail
{
    public int Id { get; set; }

    public string JobDetailCode { get; set; } = null!;

    public string PaymentCode { get; set; } = null!;

    public virtual TblShippingadvance JobDetailCodeNavigation { get; set; } = null!;

    public virtual TblShippingpayment PaymentCodeNavigation { get; set; } = null!;
}
