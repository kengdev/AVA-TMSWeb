using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class SeLevel02
{
    public string Level02Code { get; set; } = null!;

    public string Level02Name { get; set; } = null!;

    public string Level02Link { get; set; } = null!;

    public string Level02Active { get; set; } = null!;

    public int Level02Index { get; set; }

    public string Level01Code { get; set; } = null!;

    public virtual SeLevel01 Level01CodeNavigation { get; set; } = null!;

    public virtual ICollection<SeGroupPriviledge02> SeGroupPriviledge02s { get; set; } = new List<SeGroupPriviledge02>();

    public virtual ICollection<SeLevel03> SeLevel03s { get; set; } = new List<SeLevel03>();
}
