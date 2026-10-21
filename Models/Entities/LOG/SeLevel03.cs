using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class SeLevel03
{
    public string Level03Code { get; set; } = null!;

    public string Level03Name { get; set; } = null!;

    public string Level03Link { get; set; } = null!;

    public string Level03Active { get; set; } = null!;

    public string Level02Code { get; set; } = null!;

    public virtual SeLevel02 Level02CodeNavigation { get; set; } = null!;

    public virtual ICollection<SeGroupPriviledge03> SeGroupPriviledge03s { get; set; } = new List<SeGroupPriviledge03>();
}
