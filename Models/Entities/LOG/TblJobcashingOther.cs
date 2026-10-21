using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblJobcashingOther
{
    public int Id { get; set; }

    public string CashingOther { get; set; } = null!;

    public decimal CashingOtherValue { get; set; }

    public decimal CashingOtherValueup { get; set; }

    public string CashingCode { get; set; } = null!;

    public virtual TblJobcashing CashingCodeNavigation { get; set; } = null!;
}
