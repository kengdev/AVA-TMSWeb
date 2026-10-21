using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblYardCashing
{
    public string YardportCode { get; set; } = null!;

    public decimal ShortliftonFee { get; set; }

    public decimal ShortliftoffFee { get; set; }

    public decimal LongliftonFee { get; set; }

    public decimal LongliftoffFee { get; set; }

    public virtual TblYardport YardportCodeNavigation { get; set; } = null!;
}
