using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblBindingGenerate
{
    public string JobCode { get; set; } = null!;

    public string JobDetailCode { get; set; } = null!;

    public int IndexCode { get; set; }

    public bool Is2short { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblJobMaster JobCodeNavigation { get; set; } = null!;
}
