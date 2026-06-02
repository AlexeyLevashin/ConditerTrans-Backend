namespace Application.Common.Interfaces.Services;

using Contracts.Orders.Responses;

public interface IOrderDeadlineConfirmationService
{
    Task<DeadlineConfirmationRunResponse> ProcessDueConfirmationsAsync(
        CancellationToken cancellationToken = default);
}
