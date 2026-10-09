namespace TMSWeb.Models;
public record Vehicle(string Id, string Plate, string FleetType, string Partner, string Kind, string Driver, string Location, string Status);
public record DispatchJob(string Id, string Customer, string Origin, string Destination, string Kind,
    DateTimeOffset Pickup, DateTimeOffset Delivery, bool Urgent);
public record Assignment(string Id, string JobId, string VehicleId, string Driver,
    DateTimeOffset OccupiedFrom, DateTimeOffset PickupAt, DateTimeOffset DeliveryAt,
    DateTimeOffset AvailableAt, string Status, DateTimeOffset CreatedAt);
public record DispatchSnapshot(IReadOnlyList<Vehicle> Vehicles, IReadOnlyList<DispatchJob> Jobs,
    IReadOnlyList<Assignment> Assignments, int PreparationMinutes, int TurnaroundMinutes, string TimeZone);
public record AssignRequest(string JobId, string VehicleId, DateTimeOffset PickupAt, DateTimeOffset DeliveryAt);
public record MoveRequest(string AssignmentId, string VehicleId, DateTimeOffset PickupAt, DateTimeOffset DeliveryAt);
public record DispatchResult(bool Success, string Message);
