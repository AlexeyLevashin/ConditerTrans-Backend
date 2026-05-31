using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Contracts.User.Requests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/users")]
public class UserController(IUserService userService) : BaseController
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await userService.GetByIdAsync(UserId);

        if (result.IsFailed)
        {
            return NotFound();
        }

        return Ok(result.Value);
    }
    
    [HttpPost("admin-invite")]
    public async Task<IActionResult> InviteUser(CreateUserByAdminRequest request)
    {
        var result = await userService.AddByAdminAsync(request, UserId, CompanyId);

        if (result.IsSuccess)
        {
            return Ok(new { InviteId = result.Value });
        }

        return BadRequest(new { Error = result.Errors.FirstOrDefault() });
    }
}
