using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class LogSupplyerpaymentDetail
{
    public int Id { get; set; }

    public string JobDetailCode { get; set; } = null!;

    public decimal SupplyerpaymentValue { get; set; }

    public string SupplyerpaymentCode { get; set; } = null!;

    public string Action { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;
}
