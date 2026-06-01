using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Contracts.User.Requests;

public class UpdateProfileRequest
{
    [JsonPropertyName("lastName")]
    [Required(ErrorMessage = "Фамилия обязательна")]
    public string Surname { get; set; } = null!;

    [JsonPropertyName("firstName")]
    [Required(ErrorMessage = "Имя обязательно")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("middleName")]
    public string? Patronymic { get; set; }

    [Required(ErrorMessage = "Телефон обязателен")]
    [RegularExpression(@"^\+?[1-9]\d{10,14}$", ErrorMessage = "Некорректный формат телефона")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; } = null!;
}
