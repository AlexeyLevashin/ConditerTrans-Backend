using Domain.Entities;
using FluentResults;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IOrderRepository
{
    Task CreateAsync(Order order);
    Task<Order?> GetDraftByManagerIdAsync(Guid managerId);
    void Update(Order order);
}