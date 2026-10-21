using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblRequest
{
    public string RequestCode { get; set; } = null!;

    public string RequestName { get; set; } = null!;
}
