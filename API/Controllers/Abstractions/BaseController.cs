using API.DependencyInjections;
using Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Abstractions;

[ApiController]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected Guid UserId => User.GetUserId();

    protected UserRole UserRole => User.GetUserRole();
}