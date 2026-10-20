using Domain.Entities;

namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IRejectionReasonRepository
{
    Task<List<RejectionReason>> GetAvailableAsync(Guid companyId);
    Task<RejectionReason?> GetAvailableByIdAsync(Guid reasonId, Guid companyId);
    Task<bool> NameExistsAsync(Guid companyId, string name);
    Task AddAsync(RejectionReason reason);
}
