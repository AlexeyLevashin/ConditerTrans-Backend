using Common.Enums;
using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IOrderRepository
{
    Task<Guid?> GetDraftOrderIdByManagerIdAsync(Guid managerId);
    Task<Order?> GetDraftByManagerIdAsync(Guid managerId);
    Task CreateDraftAsync(Order order, Guid productId, int quantityOfUnits);
    Task UpsertOrderLineAsync(Guid orderId, Guid productId, int quantityOfUnits);
    Task<List<Order>> GetAllByManagerIdAsync(Guid managerId);
    Task<List<Order>> GetRescheduledByManagerIdAsync(Guid managerId);
    Task<Order?> GetByIdAndManagerIdAsync(Guid orderId, Guid managerId);
    Task<bool> ExistsDraftForManagerAsync(Guid orderId, Guid managerId);
    Task<bool> HasOrderLinesAsync(Guid orderId);
    Task SubmitDraftAsync(
        Guid orderId,
        Guid managerId,
        string productionAddress,
        string deliveryAddress,
        string paymentType);
    Task<List<Order>> GetAwaitingShipmentAsync();
    Task<List<Order>> GetAwaitingShipmentWithoutCargoAsync();
    Task<Order?> GetByIdWithLinesAsync(Guid orderId);
    Task LinkCargoAsync(Guid orderId, Guid cargoId);
    Task MarkAsShippedAsync(Guid orderId);
    Task<List<Order>> GetForDispatcherAsync(Guid productionCompanyId, string? search, OrderStatus? status);
    Task<Order?> GetByIdForDispatcherAsync(Guid orderId, Guid productionCompanyId);
    Task<bool> BelongsToProductionCompanyAsync(Guid orderId, Guid productionCompanyId);
    Task UpdateStatusAsync(
        Guid orderId,
        OrderStatus status,
        Guid? dispatcherId,
        string? productionAddress,
        string? comment,
        DateTime? proposedDeliveryDate = null,
        string? rescheduleReason = null,
        bool clearRescheduleProposal = false);
}
