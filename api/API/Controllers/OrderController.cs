using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Contracts.Orders.Requests;
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
}
