using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblCoversheet
{
    public string CoversheetCode { get; set; } = null!;

    public string CustomerCode { get; set; } = null!;

    public int CoversheetAmount { get; set; }

    public decimal CoversheetTotal { get; set; }

    public string? CoversheetRemark { get; set; }

    public DateOnly CoversheetDate { get; set; }

    public DateOnly? ClearingDate { get; set; }

    public bool IsVat { get; set; }

    public bool IsClearing { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblCoversheetDetail> TblCoversheetDetails { get; set; } = new List<TblCoversheetDetail>();
}
