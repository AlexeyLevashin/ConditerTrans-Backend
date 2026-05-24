using API.Controllers.Abstractions;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers;

[Route("api/companies")]
public class CompanyController(
    ICompanyService companyService,
    TokenValidationParameters tokenValidationParameters)
    : BaseController(tokenValidationParameters)
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCompany()
    {
        var company = await companyService.GetCompanyByUserIdAsync(UserId);

        if (company is null)
        {
            return NotFound();
        }

        return Ok(company);
    }
}
