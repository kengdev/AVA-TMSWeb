using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewJobListActive
{
    public string JobCode { get; set; } = null!;

    public string? JobDetailCode { get; set; }

    public string JobType { get; set; } = null!;

    public string CustomerCode { get; set; } = null!;

    public string CustomerNameTh { get; set; } = null!;

    public string CustomerAcronym { get; set; } = null!;

    public string CustomerTypeCode { get; set; } = null!;

    public string BookingNo { get; set; } = null!;

    public string ContainerType { get; set; } = null!;

    public string ContainerSizeCode { get; set; } = null!;

    public string ContainerSizeName { get; set; } = null!;

    public int ContainerAmount { get; set; }

    public int LiftedAmount { get; set; }

    public string AgentCode { get; set; } = null!;

    public DateTime JobDate { get; set; }

    public DateTime JobFinish { get; set; }

    public string? JobStatus { get; set; }

    public string? ContainerNo { get; set; }

    public string? SealNo { get; set; }

    public string? TruckCode { get; set; }

    public string? TruckNo { get; set; }

    public string? TruckLicense { get; set; }

    public string? TrailerLicense { get; set; }

    public string? SupplierCode { get; set; }

    public string? SupplierName { get; set; }

    public string? DriverFullname { get; set; }

    public string? DriverPhone { get; set; }

    public decimal? CostValue { get; set; }

    public decimal? SupplyerCostValue { get; set; }

    public string JobRemark { get; set; } = null!;

    public string? TrackingStatus { get; set; }

    public string? DriverCode { get; set; }

    public decimal? CashingTotal { get; set; }

    public string ClosingTime { get; set; } = null!;

    public string JobInvoice { get; set; } = null!;

    public decimal? TareWeight { get; set; }

    public bool? IsApprove { get; set; }

    public bool? IsDocument { get; set; }

    public bool? IsBilling { get; set; }

    public bool? IsCanceled { get; set; }

    public bool? IsSingleTrip { get; set; }

    public string? StartCode { get; set; }

    public string? StartName { get; set; }

    public string? MissionCode { get; set; }

    public string? MissionName { get; set; }

    public string? FinishCode { get; set; }

    public string? FinishName { get; set; }

    public bool TripcostRule { get; set; }

    public bool IsLiftCashing { get; set; }

    public string? SupplierInvoice { get; set; }

    public string? HashCode { get; set; }

    public bool? IsWeeklypayment { get; set; }

    public bool? IsAdvancePaid { get; set; }

    public bool? IsAdvanceProve { get; set; }
}
