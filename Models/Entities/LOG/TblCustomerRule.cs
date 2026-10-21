using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblCustomerRule
{
    public string CustomerCode { get; set; } = null!;

    public bool IsLifton { get; set; }

    public bool IsLiftoff { get; set; }

    public bool IsPort { get; set; }

    public bool IsYard { get; set; }

    public bool IsBypassTripcost { get; set; }

    public bool IsTransportOther { get; set; }

    public bool IsLiftCashing { get; set; }

    public bool IsAdvance { get; set; }

    public virtual TblCustomer CustomerCodeNavigation { get; set; } = null!;
}
