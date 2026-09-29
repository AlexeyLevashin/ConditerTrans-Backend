using API;
using API.Hangfire;
using API.Hubs;
using API.Middlewares;
using API.Reports;
using Application;
using DataAccess;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379");
builder.Services.AddSignalR();
builder.Services.AddSingleton<RejectionReportCache>();
builder.Services.AddScoped<ProductRatingMartRefreshJob>();
builder.Services.AddOrderDeadlineHangfire(builder.Configuration);
builder.Services.AddTransient<ExceptionHandlingMiddlewares>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

await app.ApplyMigrationsAsync();

app.UseSwagger(options =>
{
    options.RouteTemplate = "api/swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/api/swagger/v1/swagger.json", "ConditerTrans API v1");
    options.RoutePrefix = "api/swagger";
});

app.UseMiddleware<ExceptionHandlingMiddlewares>();

app.UseCors("AllowAll");
app.UseOrderDeadlineHangfire();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ReportsHub>("/api/hubs/reports");
app.Run();
