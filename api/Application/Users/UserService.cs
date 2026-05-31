using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Common.Enums;
using Contracts.User.Requests;
using Contracts.User.Responses;
using Domain.Entities;
using FluentResults;
using Mapster;

namespace Application.Users;

public class UserService(
    IUserRepository userRepository,
    IInvitationRepository invitationRepository,
    IUnitOfWork unitOfWork,
    IEmployeeRepository employeeRepository,
    ICargoRepository cargoRepository) : IUserService
{
    private const string AdminOnlyError = "Просматривать сотрудников может только администратор";
    private const string LogisticRoleError =
        "Для логистической компании можно назначить только роли Coordinator или Driver";
    private const string DriversForbiddenMessage =
        "Просматривать водителей может только логист-координатор";

    public async Task<Result<UserResponse>> GetByIdAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        return Result.Ok(user.DbToDto());
    }

    public async Task<Result<List<UserResponse>>> GetCompanyEmployeesAsync(Guid userId, Guid companyId)
    {
        var admin = await userRepository.GetByIdAsync(userId);

        if (admin is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        if (!admin.IsAdmin)
        {
            return Result.Fail(AdminOnlyError);
        }

        var users = await userRepository.GetByCompanyIdAsync(companyId);
        return Result.Ok(users.Select(user => user.DbToDto()).ToList());
    }

    public async Task<Result<List<DriverListItemResponse>>> GetCompanyDriversAsync(
        Guid userId,
        UserRole userRole,
        Guid companyId)
    {
        if (userRole != UserRole.Coordinator)
        {
            return Result.Fail(DriversForbiddenMessage);
        }

        var coordinator = await userRepository.GetByIdAsync(userId);
        if (coordinator is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var drivers = await userRepository.GetDriversByCompanyIdAsync(companyId);
        var result = new List<DriverListItemResponse>();

        foreach (var driver in drivers)
        {
            var employee = driver.Employee!;
            var isAvailable = !await cargoRepository.HasActiveCargoByDriverIdAsync(driver.Id);

            result.Add(new DriverListItemResponse
            {
                Id = driver.Id,
                EmployeeId = driver.EmployeeId,
                FullName = FormatEmployeeName(employee.Surname, employee.Name, employee.Patronymic),
                Phone = employee.Phone,
                EmployeeNumber = employee.EmployeeNumber,
                IsAvailable = isAvailable
            });
        }

        return Result.Ok(result);
    }

    public async Task<Result<Guid>> AddByAdminAsync(CreateUserByAdminRequest request, Guid userId, Guid adminCompanyId)
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

        var roleResult = ResolveUserRole(admin, request.UserRole);
        if (roleResult.IsFailed)
        {
            return Result.Fail(roleResult.Errors);
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
        user.UserRole = roleResult.Value;
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

        return Result.Ok(invitation.Id);
    }

    private static Result<UserRole> ResolveUserRole(User admin, UserRole? requestedRole)
    {
        if (admin.UserRole == UserRole.Coordinator)
        {
            var role = requestedRole ?? UserRole.Coordinator;

            if (role is UserRole.Coordinator or UserRole.Driver)
            {
                return Result.Ok(role);
            }

            return Result.Fail(LogisticRoleError);
        }

        if (requestedRole.HasValue && requestedRole.Value != admin.UserRole)
        {
            return Result.Fail("Нельзя назначить роль, отличную от роли администратора компании");
        }

        return Result.Ok(admin.UserRole);
    }

    private static string FormatEmployeeName(string surname, string name, string? patronymic) =>
        string.Join(' ', new[] { surname, name, patronymic }.Where(part => !string.IsNullOrWhiteSpace(part)));
}
