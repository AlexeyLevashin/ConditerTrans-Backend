using Common.Enums;
using Contracts.Auth.Requests;
using Domain.Entities;

namespace Application.Auth;

internal static class AuthMapper
{
    public static Employee ToEmployee(Guid companyId, CreateAdminRequest request) => new()
    {
        Id = Guid.CreateVersion7(),
        Phone = request.Phone,
        EmployeeNumber = request.EmployeeNumber,
        Name = request.Name,
        Surname = request.Surname,
        Patronymic = request.Patronymic,
        CompanyId = companyId,
        CreatedAt = DateTime.UtcNow
    };

    public static User ToUser(CreateAdminRequest request, Employee employee, string passwordHash, CompanyType companyType) => new()
    {
        Id = Guid.CreateVersion7(),
        Email = request.Email,
        PasswordHash = passwordHash,
        IsAdmin = true,
        UserRole = MapUserRole(companyType),
        EmployeeId = employee.Id,
        Employee = employee
    };

    private static UserRole MapUserRole(CompanyType companyType) => companyType switch
    {
        CompanyType.PurchasingCompany => UserRole.Manager,
        CompanyType.ProductionDispatcher => UserRole.Dispatcher,
        CompanyType.LogisticCompany => UserRole.Coordinator,
        _ => UserRole.Manager
    };
}
