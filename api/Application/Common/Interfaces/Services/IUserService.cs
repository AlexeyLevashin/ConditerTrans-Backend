using Common.Enums;
using Contracts.User.Requests;
using Contracts.User.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface IUserService
{
    public Task<Result<UserResponse>> GetByIdAsync(Guid userId);
    public Task<Result<Guid>> AddByAdminAsync(CreateUserByAdminRequest request, Guid userId, Guid adminCompanyId);
    public Task<Result<List<UserResponse>>> GetCompanyEmployeesAsync(Guid userId, Guid companyId);
    public Task<Result<List<DriverListItemResponse>>> GetCompanyDriversAsync(Guid userId, UserRole userRole, Guid companyId);
    Task<Result<UserResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}