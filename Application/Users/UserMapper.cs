using Contracts.User.Requests;
using Contracts.User.Responses;
using Domain.Entities;

namespace Application.Users;

internal static class UserMapper
{
    public static UserResponse DbToDto(this User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        UserRole = user.UserRole,
        IsAdmin = user.IsAdmin,
        EmployeeId = user.EmployeeId
    };
}
