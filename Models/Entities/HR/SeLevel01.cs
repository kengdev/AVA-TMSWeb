using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class SeLevel01
{
    public string Level01Code { get; set; } = null!;

    public string Level01Name { get; set; } = null!;

    public string Level01Icon { get; set; } = null!;

    public string Level01Link { get; set; } = null!;

    public string Level01Active { get; set; } = null!;

    public int Level01Index { get; set; }

    public virtual ICollection<SeGroupPriviledge01> SeGroupPriviledge01s { get; set; } = new List<SeGroupPriviledge01>();

    public virtual ICollection<SeLevel02> SeLevel02s { get; set; } = new List<SeLevel02>();
}
