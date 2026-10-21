using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class LogCoversheetDetail
{
    public int Id { get; set; }

    public string BillingCode { get; set; } = null!;

    public decimal CoversheetValue { get; set; }

    public string CoversheetCode { get; set; } = null!;

    public string Action { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;
}
