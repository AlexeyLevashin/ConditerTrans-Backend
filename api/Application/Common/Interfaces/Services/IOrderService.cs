using Contracts.Orders.Requests;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface IOrderService
{
    Task<Result> CreateAsync(Guid managerId, CreateOrderRequest request);
}