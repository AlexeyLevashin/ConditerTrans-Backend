using Contracts.User.Requests;
using Contracts.User.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface IUserService
{
    public Task<Result<UserResponse>> GetByIdAsync(Guid userId);
    public Task<Result<string>> AddByAdminAsync(CreateUserByAdminRequest request, Guid userId, Guid adminCompanyId);
}