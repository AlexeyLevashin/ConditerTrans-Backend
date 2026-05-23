using Common.Enums;

namespace Domain.Entities;

public class Company
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Inn { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CompanyTypeId { get; set; }
    public virtual CompanyType CompanyType { get; set; }
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}