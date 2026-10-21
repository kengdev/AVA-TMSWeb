using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblQuotationDetail
{
    public int Id { get; set; }

    public string FuelRate { get; set; } = null!;

    public decimal CostValue1 { get; set; }

    public decimal CostValue2 { get; set; }

    public decimal CostValue3 { get; set; }

    public string QuotationCode { get; set; } = null!;

    public string Token { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual TblQuotation QuotationCodeNavigation { get; set; } = null!;
}
