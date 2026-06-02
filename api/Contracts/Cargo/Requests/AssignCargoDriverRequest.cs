namespace Contracts.Cargo.Requests;

public class AssignCargoDriverRequest
{
    public Guid DriverId { get; set; }
    public Guid TransportVehicleId { get; set; }
    public string? Comment { get; set; }
}
