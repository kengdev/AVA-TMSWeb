using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblBillingOther
{
    public int Id { get; set; }

    public string AdvanceCode { get; set; } = null!;

    public int AdvanceId { get; set; }

    public decimal BillingValue { get; set; }

    public string BillingCode { get; set; } = null!;

    public virtual TblJobadvanceOther Advance { get; set; } = null!;

    public virtual TblJobadvance AdvanceCodeNavigation { get; set; } = null!;

    public virtual TblBillingMaster BillingCodeNavigation { get; set; } = null!;
}
