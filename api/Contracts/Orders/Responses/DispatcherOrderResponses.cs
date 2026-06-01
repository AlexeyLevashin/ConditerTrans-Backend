using System.Text.Json.Serialization;
using Common.Enums;

namespace Contracts.Orders.Responses;

public class GetDispatcherOrdersResponse
{
    public List<DispatcherOrderListItemResponse> Result { get; set; } = [];
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
}

public class DispatcherOrderDetailResponse : DispatcherOrderListItemResponse
{
    public string? ProductionAddress { get; set; }
    public List<CoordinatorOrderLineResponse> Lines { get; set; } = [];
    public string? HandoverVehicle { get; set; }
    public string? HandoverDriver { get; set; }
}
