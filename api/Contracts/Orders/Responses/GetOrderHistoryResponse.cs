using Common.Enums;

namespace Contracts.Orders.Responses;

public class GetOrderHistoryResponse
{
    public List<OrderHistoryItemResponse> Result { get; set; } = [];
}

public class OrderHistoryItemResponse
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
}
