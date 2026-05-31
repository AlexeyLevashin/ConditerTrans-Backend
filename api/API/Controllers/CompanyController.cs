using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/companies")]
public class CompanyController(ICompanyService companyService) : BaseController
{
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
}