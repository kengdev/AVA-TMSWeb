using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblJobBudgetTransaction
{
    public string TransactionCode { get; set; } = null!;

    public string JobDetailCode { get; set; } = null!;
}
