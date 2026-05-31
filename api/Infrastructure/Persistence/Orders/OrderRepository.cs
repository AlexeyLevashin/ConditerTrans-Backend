using Application.Common.Interfaces.Persistence.Repositories;
using Common.Enums;
using DataAccess;
using Domain.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Orders;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task CreateAsync(Order order)
    {
        await context.Orders.AddAsync(order);
    }

    public async Task<Order?> GetDraftByManagerIdAsync(Guid managerId)
    {
        return await context.Orders
            .Where(s => s.Status == OrderStatus.Draft)
            .Include(h => h.Histories)
            .Include(o => o.OrderLines)
            .FirstOrDefaultAsync(u => u.ManagerId == managerId);
    }
    
    public void Update(Order order)
    {
        context.Orders.Update(order);
    }
}