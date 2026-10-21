using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class SeGroup
{
    public string GroupCode { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public virtual ICollection<SeCustomerAccount> SeCustomerAccounts { get; set; } = new List<SeCustomerAccount>();

    public virtual ICollection<SeGroupPriviledge02> SeGroupPriviledge02s { get; set; } = new List<SeGroupPriviledge02>();

    public virtual ICollection<SeGroupPriviledge03> SeGroupPriviledge03s { get; set; } = new List<SeGroupPriviledge03>();
}
