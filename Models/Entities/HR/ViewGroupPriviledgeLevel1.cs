using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class ViewGroupPriviledgeLevel1
{
    public string Level01Code { get; set; } = null!;

    public string Level01Name { get; set; } = null!;

    public string Level01Icon { get; set; } = null!;

    public string Level01Link { get; set; } = null!;

    public string Level01Active { get; set; } = null!;

    public int Level01Index { get; set; }

    public string GroupCode { get; set; } = null!;
}
