using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblCodeGenerate
{
    public int Id { get; set; }

    public string CodeName { get; set; } = null!;

    public string CodeType { get; set; } = null!;

    public DateTime CreateDate { get; set; }
}
