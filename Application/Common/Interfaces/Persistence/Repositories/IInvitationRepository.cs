using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IInvitationRepository
{
    public Task AddAsync(UserInvitation invitation);
    public Task<UserInvitation?> GetByIdWithUserAsync(Guid id);
}