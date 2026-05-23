using API.Contracts.User.Requests;
using API.Contracts.User.Response;
using Common.Enums;

namespace Application.Intefaces;

public interface IEmployeeService
{
    public Task<GetUserResponse> AddAsync(CreateUserByAdminRequest request, Guid userId, UserRole userUserRole, Guid? companyId);
}