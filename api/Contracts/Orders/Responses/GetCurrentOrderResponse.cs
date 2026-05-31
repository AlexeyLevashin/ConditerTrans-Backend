using Common.Enums;

namespace Contracts.Orders.Responses;

public class GetCurrentOrderResponse
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public DateTime CreationDate { get; set; }
    public OrderStatus Status { get; set; }
    public string? ProductionAddress { get; set; }
    public string? DeliveryAddress { get; set; }
    public List<OrderLineResponse> Lines { get; set; } = [];
}

public class OrderLineResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityOfUnits { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal ProductPrice { get; set; }
    public string FormattedQuantity { get; set; } = null!;
}
