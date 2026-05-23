using Common.Enums;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Email { get; set; }
    public bool IsAdmin { get; set; }
    public string PasswordHash { get; set; }
    public Guid UserRoleId { get; set; }
    public virtual UserRole UserRole { get; set; }
    public Guid? EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}