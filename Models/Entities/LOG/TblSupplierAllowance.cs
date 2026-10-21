using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblSupplierAllowance
{
    public string SupplierCode { get; set; } = null!;

    public string JobDetailCode { get; set; } = null!;

    public decimal CostValue { get; set; }

    public decimal CostReserve { get; set; }

    public bool IsPayment { get; set; }

    public virtual TblJobDetail JobDetailCodeNavigation { get; set; } = null!;

    public virtual TblSupplier SupplierCodeNavigation { get; set; } = null!;
}
