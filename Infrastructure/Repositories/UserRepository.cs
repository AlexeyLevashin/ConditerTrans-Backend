using Application.Intefaces;
using DataAccess;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task AddAsync(User user)
    {
        await context.AddAsync(user);
    }
}