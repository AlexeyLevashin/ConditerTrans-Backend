using System.ComponentModel.DataAnnotations;

namespace Contracts.Auth.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
