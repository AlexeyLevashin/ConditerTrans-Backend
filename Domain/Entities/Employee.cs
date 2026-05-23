namespace Domain.Entities;

public class Employee
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Phone { get; set; }
    public int EmployeeNumber { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string? Patronymic { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CompanyId { get; set; }
    public virtual Company Company { get; set; }
}