using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Contracts.Transport.Requests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/transport-vehicles")]
public class TransportVehicleController(ITransportVehicleService transportVehicleService) : BaseController
{
    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands()
    {
        var result = await transportVehicleService.GetBrandsAsync(UserId, UserRole);
        return CoordinatorResult(result);
    }

    [HttpGet("models")]
    public async Task<IActionResult> GetModels([FromQuery] Guid? brandId)
    {
        var result = await transportVehicleService.GetModelsAsync(UserId, UserRole, brandId);
        return CoordinatorResult(result);
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] Guid? driverId)
    {
        var result = await transportVehicleService.GetAvailableAsync(UserId, UserRole, CompanyId, driverId);
        return CoordinatorResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTransportVehicleRequest request)
    {
        var result = await transportVehicleService.CreateAsync(UserId, UserRole, CompanyId, request);
        if (result.IsFailed)
        {
            return CoordinatorResult(result);
        }

        return Ok(result.Value);
    }

    private IActionResult CoordinatorResult<T>(FluentResults.Result<T> result)
    {
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
