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
    protected Guid CompanyId
    {
        get
        {
            var claim = User.FindFirstValue("CompanyId");
        
            if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var companyId))
            {
                throw new UnauthorizedAccessException("В токене отсутствует или некорректен обязательный claim 'CompanyId'.");
            }

            return companyId;
        }
    }
}
