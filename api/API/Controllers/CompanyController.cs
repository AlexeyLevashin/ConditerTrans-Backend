using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/companies")]
public class CompanyController(ICompanyService companyService) : BaseController
{
    private const string ManagerOnlyError = "Доступно только менеджеру по закупкам";

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await companyService.GetAllShortInfoAsync();

        if (results.IsFailed)
        {
            return BadRequest(results.Errors);
        }

        return Ok(results.Value);
    }

    /// <summary>Список компаний-производителей для менеджера (фильтр company_type = ProductionDispatcher).</summary>
    [HttpGet("manager/production")]
    public async Task<IActionResult> GetProductionForManager()
    {
        var result = await companyService.GetProductionCompaniesForManagerAsync(UserRole);

        if (result.IsFailed)
        {
            return ManagerForbidOrBadRequest(result);
        }

        return Ok(result.Value);
    }

    private IActionResult ManagerForbidOrBadRequest(ResultBase result)
    {
        if (result.Errors.Any(error => error.Message == ManagerOnlyError))
        {
            return Forbid();
        }

        return BadRequest(result.Errors);
    }
}
