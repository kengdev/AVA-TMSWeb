using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblJobadvancePayment
{
    public string JobDetailCode { get; set; } = null!;

    public bool IsPaid { get; set; }

    public decimal AdvanceValue { get; set; }

    public DateTime? ReceiveDate { get; set; }

    public virtual TblJobDetail JobDetailCodeNavigation { get; set; } = null!;
}
