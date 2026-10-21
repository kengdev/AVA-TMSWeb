using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblQuotationCondition
{
    public int Id { get; set; }

    public string Condition { get; set; } = null!;

    public string QuotationCode { get; set; } = null!;

    public virtual TblQuotation QuotationCodeNavigation { get; set; } = null!;
}
