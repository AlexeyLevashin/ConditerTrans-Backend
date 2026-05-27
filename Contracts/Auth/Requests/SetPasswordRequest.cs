using System.ComponentModel.DataAnnotations;

namespace Contracts.Auth.Requests;

public class SetPasswordRequest
{
    [Required(ErrorMessage = "Отсутствует идентификатор приглашения")]
    public Guid InviteId { get; set; }

    [Required(ErrorMessage = "Пароль обязателен для заполнения")]
    [MinLength(6, ErrorMessage = "Длина пароля должна быть не меньше 6 символов")]
    public string Password { get; set; }
}