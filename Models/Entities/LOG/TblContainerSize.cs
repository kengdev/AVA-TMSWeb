using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblContainerSize
{
    public string ContainerSizeCode { get; set; } = null!;

    public string ContainerSizeName { get; set; } = null!;

    public string ContainerSizeGroup { get; set; } = null!;

    public decimal ContainerLiftOff { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblJobMaster> TblJobMasters { get; set; } = new List<TblJobMaster>();

    public virtual ICollection<TblPortCashing> TblPortCashings { get; set; } = new List<TblPortCashing>();
}
