using System.ComponentModel.DataAnnotations;

namespace Contracts.Auth.Requests;

public class LoginRequest
{
    [EmailAddress]
    public string Email { get; set; }
    public string Password { get; set; }
}