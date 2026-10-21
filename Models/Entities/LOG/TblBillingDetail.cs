using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblBillingDetail
{
    public int Id { get; set; }

    public string JobDetailCode { get; set; } = null!;

    public decimal BillingValue { get; set; }

    public decimal AdvanceValue { get; set; }

    public string BillingCode { get; set; } = null!;

    public virtual TblBillingMaster BillingCodeNavigation { get; set; } = null!;

    public virtual TblJobDetail JobDetailCodeNavigation { get; set; } = null!;
}
