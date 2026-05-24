using API.Contracts.User.Requests;
using API.Contracts.User.Response;
using Common.Enums;

namespace Application.Intefaces;

public interface IUserService
{
    public Task<GetUserResponse> AddAsync(CreateUserByAdminRequest request, Guid userId, UserRole userUserRole, Guid? companyId);
}