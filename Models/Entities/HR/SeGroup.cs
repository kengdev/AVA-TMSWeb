using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class SeGroup
{
    public string GroupCode { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;
}
