using Application.Auth;
using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Application.Companies;
using Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
