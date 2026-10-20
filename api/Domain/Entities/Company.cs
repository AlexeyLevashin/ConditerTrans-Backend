using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace Domain.Entities;

[Table("companies")]
public class Company
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("inn")]
    public string Inn { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("address")]
    public string Address { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("company_type")]
    public CompanyType CompanyType { get; set; }

    public List<Employee> Employees { get; set; } = new();
    public List<RejectionReason> RejectionReasons { get; set; } = [];
}
