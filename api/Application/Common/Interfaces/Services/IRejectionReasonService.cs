using Contracts.RejectionReasons;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface IRejectionReasonService
{
    Task<Result<List<RejectionReasonResponse>>> GetAvailableAsync(Guid companyId);
    Task<Result<RejectionReasonResponse>> CreateAsync(Guid companyId, CreateRejectionReasonRequest request);
}
