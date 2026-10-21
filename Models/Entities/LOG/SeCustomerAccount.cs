using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class SeCustomerAccount
{
    public string CusUsername { get; set; } = null!;

    public string CusPassword { get; set; } = null!;

    public string CustomerCode { get; set; } = null!;

    public string GroupCode { get; set; } = null!;

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblCustomer CustomerCodeNavigation { get; set; } = null!;

    public virtual SeGroup GroupCodeNavigation { get; set; } = null!;
}
