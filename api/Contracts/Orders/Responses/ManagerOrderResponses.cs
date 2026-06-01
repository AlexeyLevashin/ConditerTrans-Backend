using Common.Enums;

namespace Contracts.Orders.Responses;

public class ManagerOrderListItemResponse
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public DateTime CreationDate { get; set; }
    public OrderStatus Status { get; set; }
    public string? ProductionAddress { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? PaymentType { get; set; }
    public decimal Amount { get; set; }
    public RescheduleProposalResponse? Reschedule { get; set; }
}

public class ManagerOrderDetailResponse : ManagerOrderListItemResponse
{
    public List<CoordinatorOrderLineResponse> Lines { get; set; } = [];
}

public class RescheduleProposalResponse
{
    public DateTime ProposedDeliveryDate { get; set; }
    public string Reason { get; set; } = null!;
}
