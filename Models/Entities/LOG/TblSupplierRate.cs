using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblSupplierRate
{
    public string SupplierCode { get; set; } = null!;

    public string PlaceCode { get; set; } = null!;

    public decimal CostRate { get; set; }

    public virtual TblPlace PlaceCodeNavigation { get; set; } = null!;

    public virtual TblSupplier SupplierCodeNavigation { get; set; } = null!;
}
