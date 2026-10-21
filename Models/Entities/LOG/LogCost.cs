using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class LogCost
{
    public int Id { get; set; }

    public string JobDetailCode { get; set; } = null!;

    public decimal CustomerCost { get; set; }

    public decimal SupplierCost { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;
}
