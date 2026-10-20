using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("employees")]
public class Employee
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Column("employee_number")]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Column("surname")]
    public string Surname { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("patronymic")]
    public string? Patronymic { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("company_id")]
    public Guid CompanyId { get; set; }

    public virtual Company Company { get; set; } = null!;
}
