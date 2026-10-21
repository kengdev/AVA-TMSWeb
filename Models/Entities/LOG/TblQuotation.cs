using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class TblQuotation
{
    public string QuotationCode { get; set; } = null!;

    public decimal StartRate { get; set; }

    public decimal StartPrice { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly ExpireDate { get; set; }

    public string CustomerName { get; set; } = null!;

    public string ContactName { get; set; } = null!;

    public string ContactLine { get; set; } = null!;

    public string Commodity { get; set; } = null!;

    public string Scope { get; set; } = null!;

    public string Route { get; set; } = null!;

    public string ContainerSize1 { get; set; } = null!;

    public string ContainerSize2 { get; set; } = null!;

    public string ContainerSize3 { get; set; } = null!;

    public string ConditionHour { get; set; } = null!;

    public string ConditionCharged { get; set; } = null!;

    public decimal MinimumRate { get; set; }

    public decimal PriceIncrease { get; set; }

    public decimal ChargeCutchassis { get; set; }

    public decimal InsuranceRate { get; set; }

    public decimal PriceLength { get; set; }

    public string Remark { get; set; } = null!;

    public bool IsActived { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreateDate { get; set; }

    public string CreateBy { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public string UpdateBy { get; set; } = null!;

    public virtual ICollection<TblQuotationCondition> TblQuotationConditions { get; set; } = new List<TblQuotationCondition>();

    public virtual ICollection<TblQuotationDetail> TblQuotationDetails { get; set; } = new List<TblQuotationDetail>();
}
