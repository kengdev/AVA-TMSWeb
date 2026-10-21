using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblCustomerPage
{
    public string CustomerCode { get; set; } = null!;

    public string MissionCode { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public virtual TblCustomer CustomerCodeNavigation { get; set; } = null!;

    public virtual TblPlace MissionCodeNavigation { get; set; } = null!;
}
