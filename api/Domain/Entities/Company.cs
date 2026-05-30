using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace Domain.Entities;

[Table("companies")]
public class Company
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("inn")]
    public string Inn { get; set; }

    [Column("email")]
    public string Email { get; set; }

    [Column("phone")]
    public string Phone { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("address")]
    public string Address { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("company_type")]
    public CompanyType CompanyType { get; set; }

    public List<Employee> Employees { get; set; } = new();
}
