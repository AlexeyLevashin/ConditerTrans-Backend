using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Common.Enums;
using Contracts.Cargo.Requests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/cargo")]
public class CargoController(ICargoService cargoService) : BaseController
{
    private const string CoordinatorForbiddenMessage = "Доступно только логисту-координатору";
    private const string DriverForbiddenMessage = "Доступно только водителю";

    [HttpGet("coordinator/pending")]
    public async Task<IActionResult> GetPendingForCoordinator()
    {
        var result = await cargoService.GetPendingForCoordinatorAsync(UserId, UserRole);

        if (result.IsFailed)
        {
            return result.Errors.Any(error => error.Message == CoordinatorForbiddenMessage)
                ? Forbid()
                : BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
        }

        return Ok(result.Value);
    }

    [HttpGet("coordinator/active")]
    public async Task<IActionResult> GetActiveForCoordinator()
    {
        var result = await cargoService.GetActiveForCoordinatorAsync(UserId, UserRole, CompanyId);

        if (result.IsFailed)
        {
            return result.Errors.Any(error => error.Message == CoordinatorForbiddenMessage)
                ? Forbid()
                : BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
        }

        return Ok(result.Value);
    }

    [HttpGet("coordinator")]
    public async Task<IActionResult> GetAllForCoordinator([FromQuery] CargoStatus? status)
    {
        var result = await cargoService.GetAllForCoordinatorAsync(UserId, UserRole, CompanyId, status);

        if (result.IsFailed)
        {
            return result.Errors.Any(error => error.Message == CoordinatorForbiddenMessage)
                ? Forbid()
                : BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
        }

        return Ok(result.Value);
    }

    [HttpGet("driver/active")]
    public async Task<IActionResult> GetActiveForDriver()
    {
        var result = await cargoService.GetActiveForDriverAsync(UserId, UserRole);

        if (result.IsFailed)
        {
            return result.Errors.Any(error => error.Message == DriverForbiddenMessage)
                ? Forbid()
                : BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await cargoService.GetByIdAsync(UserId, UserRole, CompanyId, id);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        var message = result.Errors.FirstOrDefault()?.Message ?? "Груз не найден";

        if (message is CoordinatorForbiddenMessage or DriverForbiddenMessage
            or "Недостаточно прав для просмотра груза")
        {
            return Forbid();
        }

        if (message == "Груз не найден")
        {
            return NotFound(new { Error = message });
        }

        return BadRequest(new { Error = message });
    }

    [HttpPost("{id:guid}/assign-driver")]
    public async Task<IActionResult> AssignDriver(Guid id, AssignCargoDriverRequest request)
    {
        var result = await cargoService.AssignDriverAsync(UserId, UserRole, CompanyId, id, request);

        if (result.IsFailed)
        {
            var message = result.Errors.FirstOrDefault()?.Message ?? "Ошибка назначения водителя";

            if (message == CoordinatorForbiddenMessage)
            {
                return Forbid();
            }

            return BadRequest(new { Error = message });
        }

        return Ok();
    }
}
