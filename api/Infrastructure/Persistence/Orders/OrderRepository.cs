using Application.Common.Interfaces.Persistence.Repositories;
using Common.Enums;
using DataAccess;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Orders;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public Task<Guid?> GetDraftOrderIdByManagerIdAsync(Guid managerId)
    {
        return context.Orders
            .Where(o => o.Status == OrderStatus.Draft && o.ManagerId == managerId)
            .Select(o => (Guid?)o.Id)
            .FirstOrDefaultAsync();
    }

    public Task<Order?> GetDraftByManagerIdAsync(Guid managerId)
    {
        return context.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Draft && o.ManagerId == managerId)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .FirstOrDefaultAsync();
    }

    public async Task CreateDraftAsync(Order order, Guid productId, int quantityOfUnits)
    {
        order.Histories.Add(new OrderChangeHistory
        {
            OrderStatus = OrderStatus.Draft,
            ChangeTime = DateTime.UtcNow
        });

        order.OrderLines.Add(new OrderLine
        {
            ProductId = productId,
            QuantityOfUnits = quantityOfUnits
        });

        await context.Orders.AddAsync(order);
    }

    public async Task UpsertOrderLineAsync(Guid orderId, Guid productId, int quantityOfUnits)
    {
        var rowsUpdated = await context.OrderLines
            .Where(ol => ol.OrderId == orderId && ol.ProductId == productId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(ol => ol.QuantityOfUnits, quantityOfUnits));

        if (rowsUpdated > 0)
        {
            return;
        }

        await context.OrderLines.AddAsync(new OrderLine
        {
            OrderId = orderId,
            ProductId = productId,
            QuantityOfUnits = quantityOfUnits
        });
    }

    public Task<List<Order>> GetAllByManagerIdAsync(Guid managerId)
    {
        return context.Orders
            .AsNoTracking()
            .Where(o => o.ManagerId == managerId && o.Status != OrderStatus.Draft)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .OrderByDescending(o => o.CreationDate)
            .ToListAsync();
    }

    public Task<List<Order>> GetRescheduledByManagerIdAsync(Guid managerId)
    {
        return context.Orders
            .AsNoTracking()
            .Where(o => o.ManagerId == managerId && o.Status == OrderStatus.Rescheduled)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .OrderByDescending(o => o.CreationDate)
            .ToListAsync();
    }

    public Task<Order?> GetByIdAndManagerIdAsync(Guid orderId, Guid managerId)
    {
        return context.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId && o.ManagerId == managerId)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .FirstOrDefaultAsync();
    }

    public Task<bool> ExistsDraftForManagerAsync(Guid orderId, Guid managerId)
    {
        return context.Orders.AnyAsync(o =>
            o.Id == orderId && o.ManagerId == managerId && o.Status == OrderStatus.Draft);
    }

    public Task<bool> HasOrderLinesAsync(Guid orderId)
    {
        return context.OrderLines.AnyAsync(ol => ol.OrderId == orderId);
    }

    public async Task SubmitDraftAsync(
        Guid orderId,
        Guid managerId,
        string productionAddress,
        string deliveryAddress,
        string paymentType)
    {
        var maxOrderNumber = await context.Orders.MaxAsync(o => (int?)o.OrderNumber) ?? 0;
        var nextOrderNumber = maxOrderNumber + 1;

        await context.Orders
            .Where(o => o.Id == orderId
                        && o.ManagerId == managerId
                        && o.Status == OrderStatus.Draft)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.ProductionAddress, productionAddress)
                .SetProperty(o => o.DeliveryAddress, deliveryAddress)
                .SetProperty(o => o.PaymentType, paymentType)
                .SetProperty(o => o.OrderNumber, nextOrderNumber)
                .SetProperty(o => o.Status, OrderStatus.PendingApproval));

        await context.OrderChangeHistories.AddAsync(new OrderChangeHistory
        {
            OrderId = orderId,
            OrderStatus = OrderStatus.PendingApproval,
            ChangeTime = DateTime.UtcNow
        });
    }

    public Task<List<Order>> GetAwaitingShipmentAsync()
    {
        return context.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.AwaitingShipment)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .OrderByDescending(o => o.CreationDate)
            .ToListAsync();
    }

    public Task<Order?> GetByIdWithLinesAsync(Guid orderId)
    {
        return context.Orders
            .Where(o => o.Id == orderId)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .FirstOrDefaultAsync();
    }

    public Task<List<Order>> GetAwaitingShipmentWithoutCargoAsync()
    {
        return context.Orders
            .Where(o => o.Status == OrderStatus.AwaitingShipment && o.CargoId == null)
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .OrderByDescending(o => o.CreationDate)
            .ToListAsync();
    }

    public async Task LinkCargoAsync(Guid orderId, Guid cargoId)
    {
        await context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.CargoId, cargoId));
    }

    public async Task MarkAsShippedAsync(Guid orderId)
    {
        var order = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Status == OrderStatus.AwaitingShipment);

        if (order is null)
        {
            return;
        }

        await UpdateStatusAsync(orderId, OrderStatus.Shipped, null, null, null);
    }

    public Task<List<Order>> GetForDispatcherAsync(Guid productionCompanyId, string? search, OrderStatus? status)
    {
        var query = BuildDispatcherOrdersQuery(productionCompanyId)
            .Where(o => o.Status != OrderStatus.Draft);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(o =>
                o.OrderNumber.ToString().Contains(term) ||
                (o.DeliveryAddress != null && o.DeliveryAddress.Contains(term)) ||
                (o.Manager != null &&
                 o.Manager.Employee != null &&
                 o.Manager.Employee.Company != null &&
                 o.Manager.Employee.Company.Name.Contains(term)));
        }

        return query
            .OrderByDescending(o => o.CreationDate)
            .ToListAsync();
    }

    public Task<Order?> GetByIdForDispatcherAsync(Guid orderId, Guid productionCompanyId)
    {
        return BuildDispatcherOrdersQuery(productionCompanyId)
            .Where(o => o.Id == orderId && o.Status != OrderStatus.Draft)
            .FirstOrDefaultAsync();
    }

    public Task<bool> BelongsToProductionCompanyAsync(Guid orderId, Guid productionCompanyId)
    {
        return BuildDispatcherOrdersQuery(productionCompanyId)
            .AnyAsync(o => o.Id == orderId);
    }

    public async Task UpdateStatusAsync(
        Guid orderId,
        OrderStatus status,
        Guid? dispatcherId,
        string? productionAddress,
        string? comment,
        DateTime? proposedDeliveryDate = null,
        string? rescheduleReason = null,
        bool clearRescheduleProposal = false)
    {
        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return;
        }

        order.Status = status;

        if (dispatcherId.HasValue)
        {
            order.DispatcherId = dispatcherId.Value;
        }

        if (productionAddress is not null)
        {
            order.ProductionAddress = productionAddress;
        }

        if (clearRescheduleProposal)
        {
            order.ProposedDeliveryDate = null;
            order.RescheduleReason = null;
        }
        else if (proposedDeliveryDate.HasValue)
        {
            order.ProposedDeliveryDate = proposedDeliveryDate;
            order.RescheduleReason = rescheduleReason;
        }

        await context.OrderChangeHistories.AddAsync(new OrderChangeHistory
        {
            OrderId = orderId,
            OrderStatus = status,
            ChangeTime = DateTime.UtcNow,
            Comment = comment
        });
    }

    private IQueryable<Order> BuildDispatcherOrdersQuery(Guid productionCompanyId)
    {
        return context.Orders
            .AsNoTracking()
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .Include(o => o.Manager!)
                .ThenInclude(m => m.Employee!)
                    .ThenInclude(e => e.Company)
            .Include(o => o.Cargo)
                .ThenInclude(c => c!.Driver)
                    .ThenInclude(d => d!.Employee)
            .Where(o =>
                o.OrderLines.Count > 0 &&
                o.OrderLines.All(ol => ol.Product != null && ol.Product.CompanyId == productionCompanyId));
    }
}
