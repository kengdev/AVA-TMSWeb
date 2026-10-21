using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblWithholdingTaxSupplierpayment
{
    public string WithholdingTaxCode { get; set; } = null!;

    public string SupplierpaymentCode { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public virtual TblSupplierpayment SupplierpaymentCodeNavigation { get; set; } = null!;

    public virtual TblWithholdingTax WithholdingTaxCodeNavigation { get; set; } = null!;
}
