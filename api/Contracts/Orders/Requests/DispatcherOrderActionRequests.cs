namespace Contracts.Orders.Requests;

public class RejectDispatcherOrderRequest
{
    public string Reason { get; set; } = null!;
}

public class RescheduleDispatcherOrderRequest
{
    public DateTime NewDeliveryDate { get; set; }
    public string Reason { get; set; } = null!;
}

public class ReadyForShipmentDispatcherOrderRequest
{
    public DateTime ShipmentDate { get; set; }
    public decimal LengthM { get; set; }
    public decimal WidthM { get; set; }
    public decimal HeightM { get; set; }
    public decimal WeightKg { get; set; }
}

public class HandoverDispatcherOrderRequest
{
    public bool DocumentsHandedOver { get; set; }
}
