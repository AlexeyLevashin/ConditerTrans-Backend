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
            .Where(o => o.ManagerId == managerId)
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

    public async Task SubmitDraftAsync(Guid orderId, Guid managerId, string address, string paymentType)
    {
        await context.Orders
            .Where(o => o.Id == orderId
                        && o.ManagerId == managerId
                        && o.Status == OrderStatus.Draft)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.DeliveryAddress, address)
                .SetProperty(o => o.PaymentType, paymentType)
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
        await context.Orders
            .Where(o => o.Id == orderId && o.Status == OrderStatus.AwaitingShipment)
            .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.Status, OrderStatus.Shipped));

        await context.OrderChangeHistories.AddAsync(new OrderChangeHistory
        {
            OrderId = orderId,
            OrderStatus = OrderStatus.Shipped,
            ChangeTime = DateTime.UtcNow
        });
    }
}
