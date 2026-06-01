using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Contracts.User.Requests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/users")]
public class UserController(IUserService userService) : BaseController
{
    private const string AdminEmployeesForbiddenMessage = "Просматривать сотрудников может только администратор";

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await userService.GetByIdAsync(UserId);

        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees()
    {
        var result = await userService.GetCompanyEmployeesAsync(UserId, CompanyId);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Errors.Any(error => error.Message == AdminEmployeesForbiddenMessage))
        {
            return Forbid();
        }

        return BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
    }

    [HttpGet("drivers")]
    public async Task<IActionResult> GetDrivers()
    {
        var result = await userService.GetCompanyDriversAsync(UserId, UserRole, CompanyId);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Errors.Any(error =>
                error.Message == "Просматривать водителей может только логист-координатор"))
        {
            return Forbid();
        }

        return BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
    }

    [HttpPost("admin-invite")]
    public async Task<IActionResult> InviteUser(CreateUserByAdminRequest request)
    {
        var result = await userService.AddByAdminAsync(request, UserId, CompanyId);

        if (result.IsSuccess)
        {
            return Ok(new { InviteId = result.Value });
        }

        return BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request)
    {
        var result = await userService.UpdateProfileAsync(UserId, request);

        if (result.IsFailed)
        {
            return BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
        }

        return Ok(result.Value);
    }

    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var result = await userService.ChangePasswordAsync(UserId, request);

        if (result.IsFailed)
        {
            return BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
        }

        return Ok();
    }
}
