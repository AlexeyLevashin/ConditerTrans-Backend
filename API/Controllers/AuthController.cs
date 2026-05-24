using Application.Common.Interfaces.Services;
using Contracts.Auth.Requests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("companies/{companyId:guid}/admin")]
    public async Task<IActionResult> CreateCompanyAdmin(Guid companyId, [FromBody] CreateAdminRequest request)
    {
        var result = await _authService.CreateCompanyAdminAsync(companyId, request);

        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.IsFailed)
        {
            return Unauthorized(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result.IsFailed)
        {
            return Unauthorized(result.Errors);
        }

        return Ok(result.Value);
    }
}