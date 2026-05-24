using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("users")]
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
}
