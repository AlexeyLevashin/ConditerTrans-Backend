using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("categories")]
public class CategoryController(ICategoryService categoryService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var results = await categoryService.GetAllAsync();

        if (results.IsFailed)
        {
            return NotFound();
        }

        return Ok(results.Value);
    }
}