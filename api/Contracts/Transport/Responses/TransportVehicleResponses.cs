namespace Contracts.Transport.Responses;

public class TransportVehicleListItemResponse
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = null!;
    public decimal Capacity { get; set; }
    public Guid EmployeeId { get; set; }
    public string DriverName { get; set; } = null!;
    public string BrandName { get; set; } = null!;
    public string ModelName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public bool IsAvailable { get; set; } = true;
}

public class FreeTransportReportRowResponse
{
    public string Driver { get; set; } = null!;
    public string Vehicle { get; set; } = null!;
    public string LicensePlate { get; set; } = null!;
    public string City { get; set; } = null!;
    public string AvailableSince { get; set; } = null!;
}
