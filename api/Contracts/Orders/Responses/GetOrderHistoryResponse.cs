using Common.Enums;

namespace Contracts.Orders.Responses;

public class GetOrderHistoryResponse
{
    public List<OrderHistoryItemResponse> Result { get; set; } = [];
}

public class OrderHistoryItemResponse
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public DateTime CreationDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Amount { get; set; }
    public RescheduleProposalResponse? Reschedule { get; set; }
}
