using Common.Enums;
using Contracts.User.Requests;
using Contracts.User.Responses;

namespace Application.Common.Interfaces.Services;

public interface IEmployeeService
{
    public Task<UserResponse> AddAsync(CreateUserByAdminRequest request, Guid userId, UserRole userUserRole, Guid? companyId);
}