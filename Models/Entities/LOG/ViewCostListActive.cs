using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewCostListActive
{
    public string CustomerCode { get; set; } = null!;

    public string CustomerAcronym { get; set; } = null!;

    public string CustomerNameTh { get; set; } = null!;

    public string? CostCode { get; set; }

    public decimal? CostValue { get; set; }

    public string? CostRemark { get; set; }

    public string? MissionCode { get; set; }

    public string? MissionName { get; set; }

    public string? ReceiptName { get; set; }

    public string? ReceiptAddress { get; set; }

    public string? ReceiptTax { get; set; }

    public string? PlaceType { get; set; }

    public decimal? ExpresswayValue { get; set; }

    public bool? IsActived { get; set; }
}
