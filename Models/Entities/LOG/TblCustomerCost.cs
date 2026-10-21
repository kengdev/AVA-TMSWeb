using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblCustomerCost
{
    public string CostCode { get; set; } = null!;

    public string CustomerCode { get; set; } = null!;

    public decimal CostValue { get; set; }

    public string CostRemark { get; set; } = null!;

    public string MissionCode { get; set; } = null!;

    public string ReceiptName { get; set; } = null!;

    public string ReceiptAddress { get; set; } = null!;

    public string ReceiptTax { get; set; } = null!;

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblCustomer CustomerCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblJobMaster> TblJobMasters { get; set; } = new List<TblJobMaster>();
}
