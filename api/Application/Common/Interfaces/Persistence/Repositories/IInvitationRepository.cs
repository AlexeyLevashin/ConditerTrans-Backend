using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IInvitationRepository
{
    Task AddAsync(UserInvitation invitation);
    Task<UserInvitation?> GetByIdWithUserAsync(Guid id);
}