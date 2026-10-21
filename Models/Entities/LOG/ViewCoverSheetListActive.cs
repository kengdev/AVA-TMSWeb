using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewCoverSheetListActive
{
    public string CoversheetCode { get; set; } = null!;

    public decimal CoversheetValue { get; set; }

    public string BillingCode { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public int CoversheetAmount { get; set; }

    public DateTime CreateDate { get; set; }
}
