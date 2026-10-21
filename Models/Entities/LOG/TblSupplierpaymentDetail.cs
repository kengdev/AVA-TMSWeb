using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblSupplierpaymentDetail
{
    public int Id { get; set; }

    public string JobDetailCode { get; set; } = null!;

    public decimal SupplierpaymentValue { get; set; }

    public decimal AdvanceValue { get; set; }

    public decimal PortValue { get; set; }

    public decimal YardValue { get; set; }

    public decimal LiftonValue { get; set; }

    public decimal LiftoffValue { get; set; }

    public decimal TotalValue { get; set; }

    public decimal AdditionalchargeValue { get; set; }

    public string SupplierpaymentCode { get; set; } = null!;

    public virtual TblJobDetail JobDetailCodeNavigation { get; set; } = null!;

    public virtual TblSupplierpayment SupplierpaymentCodeNavigation { get; set; } = null!;
}
