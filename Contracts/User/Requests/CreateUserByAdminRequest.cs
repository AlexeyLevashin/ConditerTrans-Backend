using System.ComponentModel.DataAnnotations;
using Common.Enums;

namespace Contracts.User.Requests;

public class CreateUserByAdminRequest
{
    [Required(ErrorMessage = "Имя обязательно")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Фамилия обязательна")]
    public string Surname { get; set; }
    
    public string? Patronymic { get; set; }
    
    [Required(ErrorMessage = "Телефон обязателен")]
    public string Phone { get; set; }
    
    [Required(ErrorMessage = "Email не может быть пустым")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; }
}