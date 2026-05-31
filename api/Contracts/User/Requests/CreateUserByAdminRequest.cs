using System.ComponentModel.DataAnnotations;
using Common.Enums;

namespace Contracts.User.Requests;

public class CreateUserByAdminRequest
{
    [Required(ErrorMessage = "Имя обязательно для заполнения.")]
    [MinLength(1, ErrorMessage = "Имя не может быть пустым.")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Фамилия обязательна для заполнения.")]
    [MinLength(1, ErrorMessage = "Фамилия не может быть пустой.")]
    public string Surname { get; set; }
    
    [MinLength(1, ErrorMessage = "Отчество не может быть пустым")]
    public string? Patronymic { get; set; }
    
    [Required(ErrorMessage = "Телефон обязателен")]
    [RegularExpression(@"^\+?[1-9]\d{10,14}$", ErrorMessage = "Некорректный формат телефона. Пример: +79991234567")]
    public string Phone { get; set; }
    
    [Required(ErrorMessage = "Номер сотрудника обязателен")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Номер сотрудника должен состоять ровно из 10 символов")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Номер сотрудника должен содержать только цифры")]
    public string EmployeeNumber { get; set; }
    
    [Required(ErrorMessage = "Email не может быть пустым")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; }

    public UserRole? UserRole { get; set; }
}