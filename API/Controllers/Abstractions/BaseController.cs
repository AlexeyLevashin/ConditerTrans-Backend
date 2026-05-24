using System.Security.Claims;
using API.Extensions;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Abstractions;

[ApiController]
public abstract class BaseController : ControllerBase
{
    private List<Claim> Claims => Request.GetPrincipalFromAuthorizationHeader().ToList();

    protected Guid UserId => Claims.GetUserId();
    protected UserRole UserRole => Claims.GetUserRole();
    protected string Email => Claims.GetUserEmail();
}
