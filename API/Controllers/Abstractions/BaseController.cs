using System.Security.Claims;
using API.DependencyInjections;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers.Abstractions;

[ApiController]
public abstract class BaseController(TokenValidationParameters tokenValidationParameters) : ControllerBase
{
    private ClaimsPrincipal Principal =>
        Request.GetPrincipalFromAuthorizationHeader(tokenValidationParameters);

    protected Guid UserId => Principal.GetUserId();
    protected UserRole UserRole => Principal.GetUserRole();
    protected string Email => Principal.GetUserEmail();
}
