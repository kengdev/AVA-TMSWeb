using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewJobCashingListActive
{
    public string JobCode { get; set; } = null!;

    public string JobType { get; set; } = null!;

    public string CustomerNameTh { get; set; } = null!;

    public string CustomerNameEn { get; set; } = null!;

    public string CustomerAcronym { get; set; } = null!;

    public string BookingNo { get; set; } = null!;

    public string ContainerType { get; set; } = null!;

    public string ContainerSize { get; set; } = null!;

    public string AgentCode { get; set; } = null!;

    public DateTime JobDate { get; set; }

    public string? ContainerNo { get; set; }

    public string? SealNo { get; set; }

    public string? TruckLicense { get; set; }

    public string? TrailerLicense { get; set; }

    public string? SupplierName { get; set; }

    public string? DriverFullname { get; set; }

    public string? DriverPhone { get; set; }

    public string? MissionName { get; set; }

    public string JobRemark { get; set; } = null!;

    public decimal? CostValue { get; set; }

    public decimal? AdvanceLifton { get; set; }

    public decimal? AdvanceLiftoff { get; set; }

    public decimal? AdvanceYard { get; set; }

    public decimal? AdvancePort { get; set; }

    public decimal? AdvanceTotal { get; set; }

    public string? JobDetailCode { get; set; }

    public DateOnly? BillingDate { get; set; }

    public DateOnly? ClearingDate { get; set; }

    public bool? IsDocument { get; set; }

    public bool? IsBilling { get; set; }

    public bool? IsClearing { get; set; }
}
