using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTrailerType
{
    public string TrailerTypeCode { get; set; } = null!;

    public string TrailerTypeName { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblTrailer> TblTrailers { get; set; } = new List<TblTrailer>();
}
