using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblCustomerAddress
{
    public string CustomerCode { get; set; } = null!;

    public string TransportName { get; set; } = null!;

    public string TransportAddress { get; set; } = null!;

    public string AdvanceName { get; set; } = null!;

    public string AdvanceAddress { get; set; } = null!;

    public virtual TblCustomer CustomerCodeNavigation { get; set; } = null!;
}
