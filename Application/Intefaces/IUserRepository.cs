using Domain.Entities;

namespace Application.Intefaces;

public interface IUserRepository
{
    public Task AddAsync(User user);
}