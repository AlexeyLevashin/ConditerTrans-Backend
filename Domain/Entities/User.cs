using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace Domain.Entities;

[Table("users")]
public class User
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("email")]
    public string Email { get; set; }
    [Column("is_admin")]
    public bool IsAdmin { get; set; }
    [Column("password_hash")]
    public string PasswordHash { get; set; }
    [Column("role")]
    public UserRole UserRole { get; set; }

    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    public Employee? Employee { get; set; } = null!;
}