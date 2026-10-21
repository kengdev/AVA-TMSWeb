using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblJobcashing
{
    public string CashingCode { get; set; } = null!;

    public string JobDetailCode { get; set; } = null!;

    public int CashingFuel { get; set; }

    public int CashingFuelup { get; set; }

    public decimal CashingExpressway { get; set; }

    public decimal CashingExpresswayup { get; set; }

    public decimal CashingWaste { get; set; }

    public decimal CashingOther { get; set; }

    public decimal CashingTotal { get; set; }

    public string? CashingRemark { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblJobDetail JobDetailCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblJobcashingOther> TblJobcashingOthers { get; set; } = new List<TblJobcashingOther>();
}
