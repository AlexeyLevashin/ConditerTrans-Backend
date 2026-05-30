using Common.Enums;
using Contracts.Employee.Responses;

namespace Contracts.User.Responses;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public UserRole UserRole { get; set; }
    public bool IsAdmin { get; set; }
    public Guid EmployeeId { get; set; }
    public EmployeeResponse? Employee { get; set; }
}
