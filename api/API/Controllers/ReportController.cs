using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/reports")]
public class ReportController(ITransportVehicleService transportVehicleService) : BaseController
{
    [HttpGet("coordinator/free-transport")]
    public async Task<IActionResult> GetFreeTransport()
    {
        var result = await transportVehicleService.GetFreeTransportReportAsync(UserId, UserRole, CompanyId);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Errors.Any(error => error.Message == "Доступно только логисту-координатору"))
        {
            return Forbid();
        }

        return BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
    }
}
