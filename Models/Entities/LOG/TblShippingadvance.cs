using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblShippingadvance
{
    public string JobDetailCode { get; set; } = null!;

    public bool IsPaid { get; set; }

    public decimal AdvanceValue { get; set; }

    public virtual TblJobDetail JobDetailCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblShippingpaymentDetail> TblShippingpaymentDetails { get; set; } = new List<TblShippingpaymentDetail>();
}
