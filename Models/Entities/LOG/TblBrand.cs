using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblBrand
{
    public string BrandCode { get; set; } = null!;

    public string BrandName { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblTruckDetail> TblTruckDetails { get; set; } = new List<TblTruckDetail>();
}
