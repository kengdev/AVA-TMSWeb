using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblAllowance
{
    public string AllowanceCode { get; set; } = null!;

    public string DriverCode { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal SubtractTotal { get; set; }

    public decimal AdditionalTotal { get; set; }

    public decimal AllowanceTotal { get; set; }

    public decimal ExchangeTotal { get; set; }

    public string? Remark { get; set; }

    public bool IsPayment { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual TblDriver DriverCodeNavigation { get; set; } = null!;

    public virtual ICollection<TblAllowanceDetail> TblAllowanceDetails { get; set; } = new List<TblAllowanceDetail>();

    public virtual ICollection<TblDriverSpecialvalue> TblDriverSpecialvalues { get; set; } = new List<TblDriverSpecialvalue>();
}
