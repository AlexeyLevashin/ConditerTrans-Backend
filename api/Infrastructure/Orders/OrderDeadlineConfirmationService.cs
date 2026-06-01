using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Common.Enums;

namespace Infrastructure.Orders;

public class OrderDeadlineConfirmationService(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : IOrderDeadlineConfirmationService
{
    private const string FirstRequestComment =
        "Запрос подтверждения готовности к сроку доставки (за 2 дня до даты)";

    private const string ReminderComment =
        "Повторный запрос подтверждения готовности к сроку (8 часов без ответа)";

    private const string AutoRejectComment = "Диспетчер не подтвердил готовность заказа к сроку";

    private static readonly TimeSpan ResponseWindow = TimeSpan.FromHours(8);

    public async Task ProcessDueConfirmationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var toOpen = await orderRepository.GetOrdersNeedingDeadlineWindowOpenAsync(now, cancellationToken);
        foreach (var order in toOpen)
        {
            await orderRepository.OpenDeadlineConfirmationAsync(
                order.Id,
                DeadlineConfirmationPhase.FirstRequest,
                now,
                now.Add(ResponseWindow),
                FirstRequestComment);
        }

        var expired = await orderRepository.GetOrdersWithExpiredDeadlineConfirmationAsync(now, cancellationToken);
        foreach (var order in expired)
        {
            if (order.CargoId.HasValue)
            {
                await orderRepository.ClearDeadlineConfirmationAsync(order.Id);
                continue;
            }

            if (order.DeadlineConfirmationPhase == DeadlineConfirmationPhase.FirstRequest)
            {
                await orderRepository.OpenDeadlineConfirmationAsync(
                    order.Id,
                    DeadlineConfirmationPhase.Reminder,
                    now,
                    now.Add(ResponseWindow),
                    ReminderComment);
                continue;
            }

            await orderRepository.UpdateStatusAsync(
                order.Id,
                OrderStatus.Rejected,
                dispatcherId: null,
                productionAddress: null,
                comment: AutoRejectComment,
                clearRescheduleProposal: true);

            await orderRepository.ClearDeadlineConfirmationAsync(order.Id);
        }

        if (toOpen.Count > 0 || expired.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
