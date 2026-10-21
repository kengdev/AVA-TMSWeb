using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblJobadvanceOther
{
    public int Id { get; set; }

    public string AdvanceOther { get; set; } = null!;

    public decimal AdvanceOtherValue { get; set; }

    public decimal AdvanceOtherValueup { get; set; }

    public decimal SupplierOtherValue { get; set; }

    public string AdvanceCode { get; set; } = null!;

    public bool IsBilling { get; set; }

    public virtual TblJobadvance AdvanceCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblBillingOther> TblBillingOthers { get; set; } = new List<TblBillingOther>();
}
