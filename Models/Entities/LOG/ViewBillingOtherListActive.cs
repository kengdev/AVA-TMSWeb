using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewBillingOtherListActive
{
    public int? Id { get; set; }

    public string? BillingCode { get; set; }

    public string? BillingType { get; set; }

    public string JobCode { get; set; } = null!;

    public string JobType { get; set; } = null!;

    public string CustomerCode { get; set; } = null!;

    public string CustomerNameTh { get; set; } = null!;

    public string CustomerTypeCode { get; set; } = null!;

    public string CustomerTypeName { get; set; } = null!;

    public string BookingNo { get; set; } = null!;

    public string ContainerType { get; set; } = null!;

    public string ContainerSize { get; set; } = null!;

    public string AgentCode { get; set; } = null!;

    public string? JobDate { get; set; }

    public DateTime JobDateValue { get; set; }

    public string? JobStatus { get; set; }

    public string? ContainerNo { get; set; }

    public string? SealNo { get; set; }

    public string? TruckLicense { get; set; }

    public string? TrailerLicense { get; set; }

    public string? DriverFullname { get; set; }

    public string? JobDetailCode { get; set; }

    public decimal? CostValue { get; set; }

    public bool? IsBilling { get; set; }

    public bool? IsEnabled { get; set; }

    public decimal? BillingTotal { get; set; }

    public int? BillingAmount { get; set; }

    public string CustomerAddress { get; set; } = null!;

    public bool? IsClearing { get; set; }

    public DateOnly? ClearingDate { get; set; }

    public DateOnly? BillingDate { get; set; }

    public string JobRemark { get; set; } = null!;

    public string? BillingRemark { get; set; }

    public decimal? CashingTotal { get; set; }

    public string? StartName { get; set; }

    public string? MissionName { get; set; }

    public string? FinishName { get; set; }

    public decimal? AdvanceLifton { get; set; }

    public decimal? AdvanceLiftoff { get; set; }

    public decimal? AdvanceYard { get; set; }

    public decimal? AdvancePort { get; set; }

    public decimal? AdvanceTotal { get; set; }

    public decimal? AdvanceLiftonup { get; set; }

    public decimal? AdvanceLiftoffup { get; set; }

    public decimal? AdvanceYardup { get; set; }

    public decimal? AdvancePortup { get; set; }

    public decimal? AdvanceTotalup { get; set; }

    public bool? IsAdvance { get; set; }

    public string JobInvoice { get; set; } = null!;

    public bool? IsVat { get; set; }

    public string? AdvanceOther { get; set; }

    public decimal? AdvanceOtherValue { get; set; }

    public decimal? AdvanceOtherValueup { get; set; }

    public string? TaxId { get; set; }

    public string? TaxName { get; set; }

    public string? TaxAddress { get; set; }
}
