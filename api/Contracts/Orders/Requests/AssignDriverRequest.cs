namespace Contracts.Orders.Requests;

public class AssignDriverRequest
{
    public Guid DriverId { get; set; }
    public string? Comment { get; set; }
}
