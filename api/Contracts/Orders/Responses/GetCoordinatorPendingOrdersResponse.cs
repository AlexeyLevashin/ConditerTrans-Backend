namespace Contracts.Orders.Responses;

public class GetCoordinatorPendingOrdersResponse
{
    public List<CoordinatorPendingOrderResponse> Result { get; set; } = [];
}

public class CoordinatorPendingOrderResponse
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public DateTime CreationDate { get; set; }
    public string? ProductionAddress { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? PaymentType { get; set; }
    public decimal Amount { get; set; }
    public List<CoordinatorOrderLineResponse> Lines { get; set; } = [];
}

public class CoordinatorOrderLineResponse
{
    public string ProductName { get; set; } = null!;
    public int QuantityOfUnits { get; set; }
    public string FormattedQuantity { get; set; } = null!;
    public decimal ProductPrice { get; set; }
}
