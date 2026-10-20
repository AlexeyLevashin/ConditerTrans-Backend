using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Common.Enums;
using Contracts.RejectionReasons;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/rejection-reasons")]
public class RejectionReasonController(IRejectionReasonService rejectionReasonService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAvailable()
    {
        if (UserRole != UserRole.Dispatcher)
        {
            return Forbid();
        }

        var result = await rejectionReasonService.GetAvailableAsync(CompanyId);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRejectionReasonRequest request)
    {
        if (UserRole != UserRole.Dispatcher)
        {
            return Forbid();
        }

        var result = await rejectionReasonService.CreateAsync(CompanyId, request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAvailable), result.Value)
            : BadRequest(new { Error = result.Errors.First().Message });
    }
}
