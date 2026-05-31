namespace Contracts.User.Responses;

public class DriverListItemResponse
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string EmployeeNumber { get; set; } = null!;
    public bool IsAvailable { get; set; }
}
