using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class IclockTerminal
{
    public string TerminalSn { get; set; } = null!;

    public string TerminalAlias { get; set; } = null!;

    public string TerminalAddress { get; set; } = null!;

    public string AreaAlias { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;
}
