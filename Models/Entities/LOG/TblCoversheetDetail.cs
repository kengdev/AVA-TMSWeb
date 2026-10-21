using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblCoversheetDetail
{
    public int Id { get; set; }

    public string BillingCode { get; set; } = null!;

    public decimal CoversheetValue { get; set; }

    public string CoversheetCode { get; set; } = null!;

    public virtual TblBillingMaster BillingCodeNavigation { get; set; } = null!;

    public virtual TblCoversheet CoversheetCodeNavigation { get; set; } = null!;
}
