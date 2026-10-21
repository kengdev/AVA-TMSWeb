using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TmpRecept
{
    public string Code { get; set; } = null!;

    public decimal AdvanceTotal { get; set; }

    public decimal TransportTotal { get; set; }

    public decimal Total { get; set; }
}
