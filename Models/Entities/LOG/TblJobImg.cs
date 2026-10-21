using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblJobImg
{
    public int Id { get; set; }

    public string JobDetailCode { get; set; } = null!;

    public string JobType { get; set; } = null!;

    public byte[] JobDetailImg { get; set; } = null!;

    public virtual TblJobDetail JobDetailCodeNavigation { get; set; } = null!;
}
