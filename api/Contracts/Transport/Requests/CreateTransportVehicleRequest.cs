namespace Contracts.Transport.Requests;

public class CreateTransportVehicleRequest
{
    public string RegistrationNumber { get; set; } = null!;
    public decimal Capacity { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ModelId { get; set; }
}
