using Application.Intefaces;
using Contracts.Auth.Requests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
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
        var result = await _authService.CreateCompanyAdmin(companyId, request);

        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }
}