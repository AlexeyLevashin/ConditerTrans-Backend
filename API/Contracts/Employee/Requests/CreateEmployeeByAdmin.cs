using System.ComponentModel.DataAnnotations;
using Common.Enums;

namespace API.Contracts.Employee.Requests;

public class CreateEmployeeByAdmin
{
    [Required(ErrorMessage = "Имя обязательно для заполнения.")]
    [MinLength(1, ErrorMessage = "Имя не может быть пустым.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Фамилия обязательна для заполнения.")]
    [MinLength(1, ErrorMessage = "Фамилия не может быть пустой.")]
    public string Surname { get; set; }
    
    public string Patronymic { get; set; }

    [Required(ErrorMessage = "Email не может быть пустым")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Роль обязательна для выбора")]
    [EnumDataType(typeof(UserRole), ErrorMessage = "Указана недопустимая роль.")]
    public UserRole Role { get; set; }
    public Guid? InstitutionId { get; set; }
}