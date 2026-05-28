using Application.Common.Interfaces.Persistence.Repositories;
using DataAccess;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Invitation;

public class InvitationRepository(AppDbContext context) : IInvitationRepository
{
    public async Task AddAsync(UserInvitation invitation)
    {
        await context.AddAsync(invitation);
    }
    
    public async Task<UserInvitation?> GetByIdWithUserAsync(Guid id)
    {
        return await context.UserInvitations
            .Include(i => i.User) 
                .ThenInclude(e => e.Employee)
            .FirstOrDefaultAsync(i => i.Id == id); 
    }
}