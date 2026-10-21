using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblTrailerFloor
{
    public string TrailerFloorCode { get; set; } = null!;

    public string TrailerFloorName { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblTrailer> TblTrailers { get; set; } = new List<TblTrailer>();
}
