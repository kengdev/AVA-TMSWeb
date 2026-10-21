using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class UserpriviledgeLevel1
{
    public string Level01Code { get; set; } = null!;

    public string Level01Name { get; set; } = null!;

    public string Level01Icon { get; set; } = null!;

    public string Level01Link { get; set; } = null!;

    public string Level01Active { get; set; } = null!;

    public string AccUsername { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public int Level01Index { get; set; }
}
