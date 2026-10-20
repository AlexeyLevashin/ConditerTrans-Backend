namespace Contracts.Orders.Responses;

public class RejectionDetailsResponse
{
    public Guid? ReasonId { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime RejectedAt { get; set; }
}
