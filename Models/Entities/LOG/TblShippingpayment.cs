using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblShippingpayment
{
    public string PaymentCode { get; set; } = null!;

    public string CustomerCode { get; set; } = null!;

    public DateOnly ReceiveDate { get; set; }

    public decimal Total { get; set; }

    public int Amount { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblCustomer CustomerCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblShippingpaymentDetail> TblShippingpaymentDetails { get; set; } = new List<TblShippingpaymentDetail>();
}
