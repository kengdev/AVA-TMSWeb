using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblDailypaymentTransaction
{
    public string TransactionCode { get; set; } = null!;

    public string DailypaymentCode { get; set; } = null!;
}
