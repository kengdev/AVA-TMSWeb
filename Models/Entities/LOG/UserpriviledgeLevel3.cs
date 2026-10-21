using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class UserpriviledgeLevel3
{
    public string Level03Code { get; set; } = null!;

    public string Level03Name { get; set; } = null!;

    public string Level03Link { get; set; } = null!;

    public string Level03Active { get; set; } = null!;

    public string AccUsername { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public string Level02Code { get; set; } = null!;
}
