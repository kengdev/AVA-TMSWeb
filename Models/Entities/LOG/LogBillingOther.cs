using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class LogBillingOther
{
    public int Id { get; set; }

    public string AdvanceCode { get; set; } = null!;

    public int AdvanceId { get; set; }

    public decimal BillingValue { get; set; }

    public string BillingCode { get; set; } = null!;

    public string Action { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;
}
