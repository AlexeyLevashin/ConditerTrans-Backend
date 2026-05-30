using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Contracts.Auth.Requests;
using Contracts.Auth.Responses;
using Contracts.User.Requests;
using Contracts.User.Responses;
using Domain.Entities;
using FluentResults;
using Mapster;

namespace Application.Users;

public class UserService(IUserRepository userRepository, IInvitationRepository invitationRepository, IUnitOfWork unitOfWork, IEmployeeRepository employeeRepository) : IUserService
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

    public async Task<Result<string>> AddByAdminAsync(CreateUserByAdminRequest request, Guid userId, Guid adminCompanyId)
    {
        var admin = await userRepository.GetByIdAsync(userId);
        
        if (admin is null)
        {
            return Result.Fail("Админ не найден");
        }

        if (!admin.IsAdmin)
        {
            return Result.Fail("Добавлять пользователей может только администратор");
        }
        
        var emailExists = await userRepository.GetByEmailAsync(request.Email);
        if (emailExists is not null)
        {
            return Result.Fail("Пользователь с таким email уже существует");
        }

        var phoneExists = await employeeRepository.GetByPhoneAsync(request.Phone);
        if (phoneExists is not null)
        {
            return Result.Fail("Сотрудник с таким номером телефона уже существует");
        }

        var employee = request.Adapt<Employee>();
        employee.CompanyId = adminCompanyId;
        employee.CreatedAt = DateTime.UtcNow;

        var user = request.Adapt<User>();
        user.UserRole = admin.UserRole;
        user.IsAdmin = false;
        user.PasswordHash = string.Empty;
        user.Employee = employee;
        
        var invitation = new UserInvitation
        {
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            IsUsed = false
        };
        
        await employeeRepository.AddAsync(employee);
        await userRepository.AddAsync(user);
        await invitationRepository.AddAsync(invitation);
        
        await unitOfWork.SaveChangesAsync();

        var inviteLink = $"https://conditertrans.ru/set-password?inviteId={invitation.Id}";
    
        return Result.Ok(inviteLink);
    }
}
