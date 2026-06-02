using Application.Orders;
using Common.Enums;
using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IOrderRepository
{
    Task<Guid?> GetDraftOrderIdByManagerIdAsync(Guid managerId);
    Task<Order?> GetDraftByManagerIdAsync(Guid managerId);
    Task<bool> HasBlockingManagerOrderAsync(Guid managerId);
    Task CreateDraftAsync(Order order, Guid productId, int quantityOfUnits);
    Task<bool> UpsertOrderLineAsync(Guid orderId, Guid productId, int quantityOfUnits);
    Task<List<Order>> GetAllByManagerIdAsync(Guid managerId);
    Task<Order?> GetByIdAndManagerIdAsync(Guid orderId, Guid managerId);
    Task<bool> ExistsDraftForManagerAsync(Guid orderId, Guid managerId);
    Task<bool> HasOrderLinesAsync(Guid orderId);
    Task SubmitDraftAsync(
        Guid orderId,
        Guid managerId,
        string productionAddress,
        string deliveryAddress,
        string paymentType,
        DateTime requestedDeliveryDate);

    Task<List<Order>> GetOrdersNeedingDeadlineWindowOpenAsync(DateTime utcNow, CancellationToken cancellationToken = default);

    Task<List<Order>> GetOrdersWithExpiredDeadlineConfirmationAsync(DateTime utcNow, CancellationToken cancellationToken = default);

    Task OpenDeadlineConfirmationAsync(Guid orderId, DeadlineConfirmationPhase phase, DateTime requestedAt, DateTime expiresAt, string comment);

    Task ClearDeadlineConfirmationAsync(Guid orderId);

    Task SetRequestedDeliveryDateAsync(Guid orderId, DateTime requestedDeliveryDate);
    Task<List<Order>> GetAwaitingShipmentAsync();
    Task<List<Order>> GetAwaitingShipmentWithoutCargoAsync();
    Task<Order?> GetByIdWithLinesAsync(Guid orderId);
    Task LinkCargoAsync(Guid orderId, Guid cargoId);
    Task MarkAsShippedAsync(Guid orderId);
    Task<(List<Order> Items, int TotalCount)> GetForDispatcherPagedAsync(
        Guid productionCompanyId,
        string? search,
        OrderStatus? status,
        int page,
        int pageSize);

    Task<bool> HasDispatcherOrdersRequiringDeadlineConfirmationAsync(Guid productionCompanyId);
    Task<Order?> GetByIdForDispatcherAsync(Guid orderId, Guid productionCompanyId);
    Task<bool> BelongsToProductionCompanyAsync(Guid orderId, Guid productionCompanyId);
    Task<bool> MarkReadyForShipmentAsync(
        Guid orderId,
        Guid dispatcherId,
        DateTime shipmentDate,
        decimal lengthM,
        decimal widthM,
        decimal heightM,
        decimal weightKg,
        Guid cargoId,
        string? comment);

    Task UpdateStatusAsync(
        Guid orderId,
        OrderStatus status,
        Guid? dispatcherId,
        string? productionAddress,
        string? comment,
        DateTime? proposedDeliveryDate = null,
        string? rescheduleReason = null,
        bool clearRescheduleProposal = false);

    Task<List<(string Reason, int OrderCount)>> GetRejectionStatisticsAsync(
        Guid productionCompanyId,
        DateTime? dateFromUtc,
        DateTime? dateToUtcExclusive);

    Task<List<(string ProductName, int OrderCount)>> GetProductRatingAsync(
        Guid productionCompanyId,
        DateTime? dateFromUtc,
        DateTime? dateToUtcExclusive);

    Task<List<PartnerReliabilityOrderFact>> GetPartnerReliabilityFactsAsync(
        Guid purchasingCompanyId,
        Guid partnerCompanyId,
        PartnerAnalysisKind partnerKind,
        DateTime? dateFromUtc,
        DateTime? dateToUtcExclusive);
}
