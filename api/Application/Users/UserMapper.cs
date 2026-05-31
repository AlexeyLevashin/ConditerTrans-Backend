using Contracts.Employee.Responses;
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
        EmployeeId = user.EmployeeId,
    
        Employee = user.Employee != null ? new EmployeeResponse
        {
            Name = user.Employee.Name,
            Surname = user.Employee.Surname,
            Patronymic = user.Employee.Patronymic,
            Phone = user.Employee.Phone,
            EmployeeNumber = user.Employee.EmployeeNumber,
            CompanyId = user.Employee.CompanyId,
            CreatedAt = user.Employee.CreatedAt,
        } : null
    };
}
