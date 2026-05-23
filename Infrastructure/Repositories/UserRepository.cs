using Application.Intefaces;
using DataAccess;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    public async Task AddAsync(User user)
    {
        await _context.AddAsync(user);
    }
}