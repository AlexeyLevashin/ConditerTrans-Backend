using Application.Common.Interfaces.Persistence.Repositories;
using DataAccess;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Users;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task AddAsync(User user)
    {
        await context.AddAsync(user);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<User?> GetByIdAsync(Guid userId)
    {
        return context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
}