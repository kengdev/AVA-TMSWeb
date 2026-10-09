using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblRestday
{
    public int Id { get; set; }

    public string RestdayName { get; set; } = null!;

    public DateOnly RestdayStart { get; set; }

    public DateOnly RestdayEnd { get; set; }

    public string? Remark { get; set; }

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;
}
