using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Contracts.Orders.Requests;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/orders")]
public class OrderController(IOrderService orderService) : BaseController
{
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
}