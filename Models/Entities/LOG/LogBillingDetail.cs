using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class LogBillingDetail
{
    public int Id { get; set; }

    public string JobDetailCode { get; set; } = null!;

    public decimal BillingValue { get; set; }

    public decimal AdvanceValue { get; set; }

    public string BillingCode { get; set; } = null!;

    public string Action { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;
}
