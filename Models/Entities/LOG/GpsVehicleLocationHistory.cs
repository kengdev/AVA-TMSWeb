using System;
using System.Collections.Generic;

namespace TMSWeb.Models.Entities.LOG;

public partial class GpsVehicleLocationHistory
{
    public long Id { get; set; }

    public string Provider { get; set; } = null!;

    public string? VehicleId { get; set; }

    public string? Registration { get; set; }

    public string? ChassisNumber { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? Speed { get; set; }

    public decimal? Bearing { get; set; }

    public bool? Ignition { get; set; }

    public bool? Idling { get; set; }

    public decimal? Odometer { get; set; }

    public decimal? Rpm { get; set; }

    public decimal? RoadSpeed { get; set; }

    public decimal? ExternalVoltage { get; set; }

    public decimal? FuelLevel { get; set; }

    public decimal? FuelPercent { get; set; }

    public string? DriverId { get; set; }

    public string? DriverName { get; set; }

    public string? DriverTag { get; set; }

    public string? PositionDescription { get; set; }

    public int? GpsFixType { get; set; }

    public DateTime? GpsTime { get; set; }

    public DateTime? LocationUpdated { get; set; }

    public DateTime? FuelUpdated { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? RawJson { get; set; }
}
