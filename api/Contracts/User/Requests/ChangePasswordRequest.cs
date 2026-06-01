using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Contracts.User.Requests;

public class ChangePasswordRequest
{
    [JsonPropertyName("currentPassword")]
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [JsonPropertyName("newPassword")]
    [Required]
    [MinLength(6, ErrorMessage = "Новый пароль должен быть не короче 6 символов")]
    public string NewPassword { get; set; } = null!;

    [JsonPropertyName("confirmPassword")]
    [Required]
    public string ConfirmPassword { get; set; } = null!;
}
