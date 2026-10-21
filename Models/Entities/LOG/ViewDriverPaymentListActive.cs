using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class ViewDriverPaymentListActive
{
    public string JobCode { get; set; } = null!;

    public string JobDetailCode { get; set; } = null!;

    public string DriverCode { get; set; } = null!;

    public string DriverFullname { get; set; } = null!;

    public DateTime JobDate { get; set; }

    public string BookingNo { get; set; } = null!;

    public string ContainerNo { get; set; } = null!;

    public string SealNo { get; set; } = null!;

    public string TruckLicense { get; set; } = null!;

    public string TrailerLicense { get; set; } = null!;

    public string StartName { get; set; } = null!;

    public string MissionName { get; set; } = null!;

    public decimal TripValue { get; set; }

    public decimal FuelBalance { get; set; }

    public int? FuelValue { get; set; }

    public int? FuelUp { get; set; }

    public decimal WasteValue { get; set; }

    public decimal OtherValue { get; set; }

    public string CostType { get; set; } = null!;

    public decimal CostValue { get; set; }

    public string ContainerSizeCode { get; set; } = null!;

    public string ContainerSizeName { get; set; } = null!;

    public string ZoneName { get; set; } = null!;

    public string TruckTypegroupCode { get; set; } = null!;

    public string? HashCode { get; set; }
}
