namespace Application.Common.Interfaces.Services;

public interface IOrderDeadlineConfirmationService
{
    Task ProcessDueConfirmationsAsync(CancellationToken cancellationToken = default);
}
