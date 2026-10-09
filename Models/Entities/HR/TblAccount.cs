using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblAccount
{
    public string AccUsername { get; set; } = null!;

    public string AccPassword { get; set; } = null!;

    public string AccFullname { get; set; } = null!;

    public string LineId { get; set; } = null!;

    public string GroupCode { get; set; } = null!;

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblGroup GroupCodeNavigation { get; set; } = null!;
}
