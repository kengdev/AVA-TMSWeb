using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.HR;

public partial class TblGroup
{
    public string GroupCode { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<SeGroupPriviledge01> SeGroupPriviledge01s { get; set; } = new List<SeGroupPriviledge01>();

    public virtual ICollection<SeGroupPriviledge02> SeGroupPriviledge02s { get; set; } = new List<SeGroupPriviledge02>();

    public virtual ICollection<SeGroupPriviledge03> SeGroupPriviledge03s { get; set; } = new List<SeGroupPriviledge03>();

    public virtual ICollection<TblAccount> TblAccounts { get; set; } = new List<TblAccount>();
}
