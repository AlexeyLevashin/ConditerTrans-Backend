using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Common.Enums;
using Contracts.Orders.Requests;
using Contracts.Orders.Responses;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/orders")]
public class OrderController(
    IOrderService orderService,
    IOrderDeadlineConfirmationService deadlineConfirmationService) : BaseController
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
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await orderService.GetHistoryAsync(UserId, page, pageSize);
        return Ok(result.Value);
    }

    /// <summary>
    /// Анализ надёжности партнёра (производство / транспорт). Только завершённые заказы (Delivered).
    /// </summary>
    [HttpGet("manager/reports/partner-reliability")]
    public async Task<IActionResult> GetManagerPartnerReliability(
        [FromQuery] Guid companyId,
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo,
        [FromQuery] string? partnerType)
    {
        var result = await orderService.GetManagerPartnerReliabilityAsync(
            UserId,
            UserRole,
            CompanyId,
            companyId,
            dateFrom,
            dateTo,
            partnerType);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Errors.Any(error => error.Message == "Доступно только менеджеру по закупкам"))
        {
            return Forbid();
        }

        return BadRequest(result.Errors);
    }

    [HttpGet("dispatcher/reports/refusals")]
    public async Task<IActionResult> GetDispatcherRejectionReport(
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo)
    {
        var result = await orderService.GetDispatcherRejectionReportAsync(
            UserId,
            UserRole,
            CompanyId,
            dateFrom,
            dateTo);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return DispatcherForbidOrBadRequest(result);
    }

    [HttpGet("dispatcher/reports/product-rating")]
    public async Task<IActionResult> GetDispatcherProductRatingReport(
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo)
    {
        var result = await orderService.GetDispatcherProductRatingReportAsync(
            UserId,
            UserRole,
            CompanyId,
            dateFrom,
            dateTo);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return DispatcherForbidOrBadRequest(result);
    }

    [HttpGet("dispatcher")]
    public async Task<IActionResult> GetDispatcherOrders(
        [FromQuery] string? search,
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await orderService.GetDispatcherOrdersAsync(
            UserId,
            UserRole,
            CompanyId,
            search,
            status,
            page,
            pageSize);

        if (result.IsFailed)
        {
            return DispatcherForbidOrBadRequest(result);
        }

        return Ok(result.Value);
    }

    /// <summary>Ручной запуск проверки дедлайнов (то же, что Hangfire job). Для теста.</summary>
    [HttpPost("dispatcher/run-deadline-check")]
    public async Task<IActionResult> RunDeadlineCheck()
    {
        if (UserRole != UserRole.Dispatcher)
        {
            return Forbid();
        }

        var result = await deadlineConfirmationService.ProcessDueConfirmationsAsync(HttpContext.RequestAborted);
        return Ok(result);
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

    [HttpDelete("line")]
    public async Task<IActionResult> RemoveLine([FromBody] CreateOrderRequest request)
    {
        var result = await orderService.RemoveLineFromOrderAsync(UserId, request);

        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return Ok();
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

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

    /// <summary>Создать новый черновик с теми же позициями, что в выбранном заказе из истории.</summary>
    [HttpPost("{id:guid}/repeat")]
    public async Task<IActionResult> RepeatOrder(Guid id)
    {
        var result = await orderService.RepeatOrderAsync(UserId, id);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Errors.Any(error => error.Message == ManagerOnlyError))
        {
            return Forbid();
        }

        return result.Errors.FirstOrDefault()?.Message == "Заказ не найден"
            ? NotFound(result.Errors)
            : BadRequest(result.Errors);
    }

    private const string ManagerOnlyError = "Доступно только менеджеру по закупкам";

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
