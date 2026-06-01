using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Common.Enums;
using Contracts.Orders.Requests;
using Contracts.Orders.Responses;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/orders")]
public class OrderController(IOrderService orderService) : BaseController
{
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent()
    {
        var result = await orderService.GetCurrentDraftAsync(UserId);

        if (result.IsFailed)
        {
            return NotFound(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var result = await orderService.GetHistoryAsync(UserId);
        return Ok(result.Value);
    }

    [HttpGet("rescheduled")]
    public async Task<IActionResult> GetRescheduledOrders()
    {
        var result = await orderService.GetRescheduledOrdersAsync(UserId);

        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpGet("dispatcher")]
    public async Task<IActionResult> GetDispatcherOrders([FromQuery] string? search, [FromQuery] OrderStatus? status)
    {
        var result = await orderService.GetDispatcherOrdersAsync(UserId, UserRole, CompanyId, search, status);

        if (result.IsFailed)
        {
            return DispatcherForbidOrBadRequest(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("dispatcher/{id:guid}")]
    public async Task<IActionResult> GetDispatcherOrderById(Guid id)
    {
        var result = await orderService.GetDispatcherOrderByIdAsync(UserId, UserRole, CompanyId, id);

        if (result.IsFailed)
        {
            return DispatcherNotFoundOrForbid(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await orderService.GetByIdAsync(UserId, id);

        if (result.IsFailed)
        {
            return NotFound(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request)
    {
        var result = await orderService.CreateAsync(UserId, request);

        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return Ok();
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, SubmitOrderRequest request)
    {
        var result = await orderService.SubmitAsync(UserId, id, request);

        if (result.IsFailed)
        {
            return result.Errors.FirstOrDefault()?.Message == "Заказ не найден"
                ? NotFound(result.Errors)
                : BadRequest(result.Errors);
        }

        return Ok();
    }

    [HttpPost("{id:guid}/reschedule/accept")]
    public async Task<IActionResult> AcceptManagerReschedule(Guid id, AcceptManagerRescheduleRequest request)
    {
        var result = await orderService.AcceptManagerRescheduleAsync(UserId, id, request);
        return ManagerDetailResult(result);
    }

    [HttpPost("{id:guid}/reschedule/reject")]
    public async Task<IActionResult> RejectManagerReschedule(Guid id, RejectManagerRescheduleRequest request)
    {
        var result = await orderService.RejectManagerRescheduleAsync(UserId, id, request);
        return ManagerDetailResult(result);
    }

    [HttpPost("dispatcher/{id:guid}/confirm")]
    public async Task<IActionResult> ConfirmDispatcherOrder(Guid id)
    {
        var result = await orderService.ConfirmDispatcherOrderAsync(UserId, UserRole, CompanyId, id);
        return DispatcherDetailResult(result);
    }

    [HttpPost("dispatcher/{id:guid}/reject")]
    public async Task<IActionResult> RejectDispatcherOrder(Guid id, RejectDispatcherOrderRequest request)
    {
        var result = await orderService.RejectDispatcherOrderAsync(UserId, UserRole, CompanyId, id, request);
        return DispatcherDetailResult(result);
    }

    [HttpPost("dispatcher/{id:guid}/reschedule")]
    public async Task<IActionResult> RescheduleDispatcherOrder(Guid id, RescheduleDispatcherOrderRequest request)
    {
        var result = await orderService.RescheduleDispatcherOrderAsync(UserId, UserRole, CompanyId, id, request);
        return DispatcherDetailResult(result);
    }

    [HttpPost("dispatcher/{id:guid}/ready-for-shipment")]
    public async Task<IActionResult> ReadyDispatcherOrderForShipment(
        Guid id,
        ReadyForShipmentDispatcherOrderRequest request)
    {
        var result = await orderService.ReadyDispatcherOrderForShipmentAsync(
            UserId,
            UserRole,
            CompanyId,
            id,
            request);

        return DispatcherDetailResult(result);
    }

    [HttpPost("dispatcher/{id:guid}/handover")]
    public async Task<IActionResult> HandoverDispatcherOrder(Guid id, HandoverDispatcherOrderRequest request)
    {
        var result = await orderService.HandoverDispatcherOrderAsync(UserId, UserRole, CompanyId, id, request);
        return DispatcherDetailResult(result);
    }

    private IActionResult ManagerDetailResult(Result<ManagerOrderDetailResponse> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Errors.Any(error => error.Message.Contains("не найден", StringComparison.OrdinalIgnoreCase)))
        {
            return NotFound(new { Error = result.Errors.First().Message });
        }

        return BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
    }

    private IActionResult DispatcherDetailResult(Result<DispatcherOrderDetailResponse> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return DispatcherNotFoundOrForbid(result);
    }

    private IActionResult DispatcherNotFoundOrForbid(ResultBase result)
    {
        if (result.Errors.Any(error => error.Message == "Доступно только диспетчеру производства"))
        {
            return Forbid();
        }

        if (result.Errors.Any(error => error.Message == "Заказ не найден"))
        {
            return NotFound(new { Error = result.Errors.First().Message });
        }

        return BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
    }

    private IActionResult DispatcherForbidOrBadRequest(ResultBase result)
    {
        if (result.Errors.Any(error => error.Message == "Доступно только диспетчеру производства"))
        {
            return Forbid();
        }

        return BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
    }
}
