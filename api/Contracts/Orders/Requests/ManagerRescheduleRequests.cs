namespace Contracts.Orders.Requests;

public class RejectManagerRescheduleRequest
{
    public string? Reason { get; set; }
}

public class AcceptManagerRescheduleRequest
{
    public string? Comment { get; set; }
}
