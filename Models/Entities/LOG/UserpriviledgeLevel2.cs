using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class UserpriviledgeLevel2
{
    public string Level02Code { get; set; } = null!;

    public string Level02Name { get; set; } = null!;

    public string Level02Link { get; set; } = null!;

    public string Level02Active { get; set; } = null!;

    public string Level01Code { get; set; } = null!;

    public int Level02Index { get; set; }

    public string AccUsername { get; set; } = null!;

    public bool IsEnabled { get; set; }
}
