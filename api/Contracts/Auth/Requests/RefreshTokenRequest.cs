using System.ComponentModel.DataAnnotations;

namespace Contracts.Auth.Requests;

public class RefreshTokenRequest
{
    [Required (ErrorMessage = "Токен не может быть пустым")]
    public string RefreshToken { get; set; } = null!;
}
