using Contracts.Orders.Requests;
using Contracts.Orders.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface IOrderService
{
    Task<Result> CreateAsync(Guid managerId, CreateOrderRequest request);
    Task<Result<GetCurrentOrderResponse>> GetCurrentDraftAsync(Guid managerId);
    Task<Result<GetOrderHistoryResponse>> GetHistoryAsync(Guid managerId);
    Task<Result<GetOrderByIdResponse>> GetByIdAsync(Guid managerId, Guid orderId);
    Task<Result> SubmitAsync(Guid managerId, Guid orderId, SubmitOrderRequest request);
}