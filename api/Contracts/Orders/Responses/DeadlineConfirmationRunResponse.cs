namespace Contracts.Orders.Responses;

public class DeadlineConfirmationRunResponse
{
    public int OpenedCount { get; set; }
    public int ReminderCount { get; set; }
    public int RejectedCount { get; set; }
    public int SkippedAlreadyHandledCount { get; set; }
}
