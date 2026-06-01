using Application.Common.Interfaces.Services;
using Hangfire;
using Hangfire.PostgreSql;

namespace API.Hangfire;

public static class HangfireServiceCollectionExtensions
{
    public static IServiceCollection AddOrderDeadlineHangfire(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

        services.AddHangfireServer();

        return services;
    }

    public static WebApplication UseOrderDeadlineHangfire(this WebApplication app)
    {
        app.UseHangfireDashboard(
            "/api/hangfire",
            new DashboardOptions { Authorization = [new HangfireAuthorizationFilter()] });

        RecurringJob.AddOrUpdate<IOrderDeadlineConfirmationService>(
            "order-deadline-confirmation",
            service => service.ProcessDueConfirmationsAsync(CancellationToken.None),
            "*/5 * * * *");

        return app;
    }
}
