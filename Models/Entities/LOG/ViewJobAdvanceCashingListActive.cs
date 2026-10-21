using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewJobAdvanceCashingListActive
{
    public string JobDetailCode { get; set; } = null!;

    public string CustomerNameEn { get; set; } = null!;

    public string CustomerAcronym { get; set; } = null!;

    public string JobType { get; set; } = null!;

    public string BookingNo { get; set; } = null!;

    public string ContainerNo { get; set; } = null!;

    public string SupplierName { get; set; } = null!;

    public string SealNo { get; set; } = null!;

    public decimal TareWeight { get; set; }

    public string ContainerSizeName { get; set; } = null!;

    public string AgentCode { get; set; } = null!;

    public string JobInvoice { get; set; } = null!;

    public string TrailerLicense { get; set; } = null!;

    public DateTime JobDate { get; set; }

    public string TruckCode { get; set; } = null!;

    public string TruckLicense { get; set; } = null!;

    public string? TruckNo { get; set; }

    public string DriverFullname { get; set; } = null!;

    public string SupplierCode { get; set; } = null!;

    public decimal CostValue { get; set; }

    public string? AdvanceCode { get; set; }

    public decimal? AdvanceLifton { get; set; }

    public decimal? AdvanceLiftoff { get; set; }

    public decimal? AdvanceYard { get; set; }

    public decimal? AdvancePort { get; set; }

    public decimal? AdvanceTotal { get; set; }

    public decimal? AdvanceOtherValue { get; set; }

    public decimal Shore { get; set; }

    public string? CashingCode { get; set; }

    public int? CashingFuel { get; set; }

    public int? CashingFuelup { get; set; }

    public decimal? CashingExpressway { get; set; }

    public decimal? CashingExpresswayup { get; set; }

    public decimal? CashingWaste { get; set; }

    public decimal? CashingOther { get; set; }

    public decimal? CashingTotal { get; set; }

    public decimal? CashingOtherValue { get; set; }

    public bool IsBilling { get; set; }

    public string? BillingType { get; set; }

    public bool? IsClearing { get; set; }

    public DateOnly? ClearingDate { get; set; }

    public bool IsDocument { get; set; }

    public bool IsSingleTrip { get; set; }

    public bool IsCanceled { get; set; }

    public string Start { get; set; } = null!;

    public string Industrial { get; set; } = null!;
}
