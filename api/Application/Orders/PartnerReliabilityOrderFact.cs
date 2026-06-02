namespace Application.Orders;

public sealed record PartnerReliabilityOrderFact(
    Guid OrderId,
    DateTime? RequestedDeliveryDateUtc,
    DateTime ActualDeliveryUtc,
    bool HadReschedule);
