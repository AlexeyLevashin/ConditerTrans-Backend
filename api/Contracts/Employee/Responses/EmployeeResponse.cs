namespace Contracts.Employee.Responses;

public class EmployeeResponse
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string Phone { get; set; } = null!;
    public string EmployeeNumber { get; set; } = null!;
    public Guid CompanyId { get; set; } 
}