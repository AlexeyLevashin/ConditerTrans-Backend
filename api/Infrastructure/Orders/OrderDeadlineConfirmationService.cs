using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Common.Enums;
using Contracts.Orders.Responses;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Orders;

public class OrderDeadlineConfirmationService(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ILogger<OrderDeadlineConfirmationService> logger) : IOrderDeadlineConfirmationService
{
    private const string FirstRequestComment =
        "Запрос подтверждения готовности к сроку доставки (за 2 дня до даты)";

    private const string ReminderComment =
        "Повторный запрос подтверждения готовности к сроку (8 часов без ответа)";

    private const string AutoRejectComment = "Диспетчер не подтвердил готовность заказа к сроку";

    private static readonly TimeSpan ResponseWindow = TimeSpan.FromHours(8);

    public async Task<DeadlineConfirmationRunResponse> ProcessDueConfirmationsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = new DeadlineConfirmationRunResponse();
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
            result.OpenedCount++;
        }

        var expired = await orderRepository.GetOrdersWithExpiredDeadlineConfirmationAsync(now, cancellationToken);
        foreach (var order in expired)
        {
            if (order.CargoId.HasValue)
            {
                await orderRepository.ClearDeadlineConfirmationAsync(order.Id);
                result.SkippedAlreadyHandledCount++;
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
                result.ReminderCount++;
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
            result.RejectedCount++;
        }

        if (result.OpenedCount > 0 || result.ReminderCount > 0 || result.RejectedCount > 0 ||
            result.SkippedAlreadyHandledCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Deadline confirmation job (UTC {UtcNow:O}): opened={Opened}, reminder={Reminder}, rejected={Rejected}, skipped={Skipped}",
            now,
            result.OpenedCount,
            result.ReminderCount,
            result.RejectedCount,
            result.SkippedAlreadyHandledCount);

        if (result.OpenedCount == 0 && result.ReminderCount == 0 && result.RejectedCount == 0)
        {
            logger.LogInformation(
                "No orders matched deadline rules. Need: status=Confirmed, cargo_id IS NULL, requested_delivery_date set, " +
                "UTC today >= delivery_date - 2 days, deadline_confirmation_phase=0 (or expired window for reminder/reject).");
        }

        return result;
    }
}
