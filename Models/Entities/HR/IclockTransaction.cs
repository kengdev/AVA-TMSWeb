using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class IclockTransaction
{
    public long Id { get; set; }

    public string EmpCode { get; set; } = null!;

    public DateTime PunchTime { get; set; }

    public string TerminalSn { get; set; } = null!;

    public string TerminalAlias { get; set; } = null!;

    public int VerifyType { get; set; }

    public DateTime CreateDate { get; set; }
}
