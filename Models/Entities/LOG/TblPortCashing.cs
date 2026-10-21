using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblPortCashing
{
    public string YardportCode { get; set; } = null!;

    public string ContainerSizeCode { get; set; } = null!;

    public decimal ContainerFee { get; set; }

    public virtual TblContainerSize ContainerSizeCodeNavigation { get; set; } = null!;

    public virtual TblYardport YardportCodeNavigation { get; set; } = null!;
}
