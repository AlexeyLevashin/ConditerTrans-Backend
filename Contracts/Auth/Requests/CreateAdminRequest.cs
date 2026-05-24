using System.ComponentModel.DataAnnotations;

namespace Contracts.Auth.Requests;


public class CreateAdminRequest
{
    [EmailAddress(ErrorMessage = "Неверный email")]
    public string Email { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public int EmployeeNumber { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Patronymic { get; set; }
}