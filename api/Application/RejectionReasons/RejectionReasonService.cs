using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Contracts.RejectionReasons;
using Domain.Entities;
using FluentResults;

namespace Application.RejectionReasons;

public class RejectionReasonService(
    IRejectionReasonRepository rejectionReasonRepository,
    IUnitOfWork unitOfWork) : IRejectionReasonService
{
    public async Task<Result<List<RejectionReasonResponse>>> GetAvailableAsync(Guid companyId)
    {
        var reasons = await rejectionReasonRepository.GetAvailableAsync(companyId);
        return Result.Ok(reasons.Select(ToResponse).ToList());
    }

    public async Task<Result<RejectionReasonResponse>> CreateAsync(
        Guid companyId,
        CreateRejectionReasonRequest request)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
        {
            return Result.Fail("Название причины должно содержать не менее 3 символов");
        }

        if (name.Length > 250)
        {
            return Result.Fail("Название причины не должно превышать 250 символов");
        }

        if (await rejectionReasonRepository.NameExistsAsync(companyId, name))
        {
            return Result.Fail("Такая причина уже существует");
        }

        var reason = new RejectionReason
        {
            CompanyId = companyId,
            Name = name,
            IsDefault = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await rejectionReasonRepository.AddAsync(reason);
        await unitOfWork.SaveChangesAsync();

        return Result.Ok(ToResponse(reason));
    }

    private static RejectionReasonResponse ToResponse(RejectionReason reason) => new()
    {
        Id = reason.Id,
        Name = reason.Name,
        IsDefault = reason.IsDefault
    };
}
