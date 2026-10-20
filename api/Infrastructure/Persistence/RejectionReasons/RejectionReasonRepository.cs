using Application.Common.Interfaces.Persistence.Repositories;
using DataAccess;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.RejectionReasons;

public class RejectionReasonRepository(AppDbContext context) : IRejectionReasonRepository
{
    public Task<List<RejectionReason>> GetAvailableAsync(Guid companyId) =>
        context.RejectionReasons
            .AsNoTracking()
            .Where(reason => reason.IsActive && (reason.IsDefault || reason.CompanyId == companyId))
            .OrderByDescending(reason => reason.IsDefault)
            .ThenBy(reason => reason.Name)
            .ToListAsync();

    public Task<RejectionReason?> GetAvailableByIdAsync(Guid reasonId, Guid companyId) =>
        context.RejectionReasons
            .AsNoTracking()
            .FirstOrDefaultAsync(reason =>
                reason.Id == reasonId &&
                reason.IsActive &&
                (reason.IsDefault || reason.CompanyId == companyId));

    public Task<bool> NameExistsAsync(Guid companyId, string name) =>
        context.RejectionReasons.AnyAsync(reason =>
            reason.Name.ToLower() == name.ToLower() &&
            (reason.IsDefault || reason.CompanyId == companyId));

    public Task AddAsync(RejectionReason reason) => context.RejectionReasons.AddAsync(reason).AsTask();
}
