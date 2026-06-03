using System.Text.Json.Serialization;
using Common.Enums;

namespace Contracts.Orders.Responses;

public class GetDispatcherOrdersResponse
{
    public List<DispatcherOrderListItemResponse> Result { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasOrdersRequiringDeadlineConfirmation { get; set; }
}

public class DispatcherOrderListItemResponse
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public string CompanyName { get; set; } = null!;
    public DateTime CreationDate { get; set; }
    public string? DeliveryAddress { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentType { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentMethodLabel { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public bool RequiresDeadlineConfirmation { get; set; }
    public DateTime? DeadlineConfirmationExpiresAt { get; set; }
    public DeadlineConfirmationPhase DeadlineConfirmationPhase { get; set; }
}

public class DispatcherOrderDetailResponse : DispatcherOrderListItemResponse
{
    public string? ProductionAddress { get; set; }
    public DateTime? ProposedDeliveryDate { get; set; }
    public string? RescheduleReason { get; set; }
    public decimal? ShipmentLengthM { get; set; }
    public decimal? ShipmentWidthM { get; set; }
    public decimal? ShipmentHeightM { get; set; }
    public decimal? ShipmentWeightKg { get; set; }
    public List<CoordinatorOrderLineResponse> Lines { get; set; } = [];
    public string? HandoverVehicle { get; set; }
    public string? HandoverDriver { get; set; }
}
