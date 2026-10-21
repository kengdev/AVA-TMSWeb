using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class SeGroupPriviledge02
{
    public string GroupCode { get; set; } = null!;

    public string Level02Code { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual SeGroup GroupCodeNavigation { get; set; } = null!;

    public virtual SeLevel02 Level02CodeNavigation { get; set; } = null!;
}
