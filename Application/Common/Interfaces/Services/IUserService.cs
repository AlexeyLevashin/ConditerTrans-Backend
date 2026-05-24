using Contracts.User.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface IUserService
{
    public Task<Result<UserResponse>> GetByIdAsync(Guid userId);
}