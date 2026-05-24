using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Contracts.User.Responses;
using FluentResults;

namespace Application.Users;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<Result<UserResponse>> GetByIdAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        return Result.Ok(user.DbToDto());
    }
}
