using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/products")]

public class ProductController(IProductService productService) : BaseController
{
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetById(Guid productId)
    {
        var result = await productService.GetProductByIdAsync(productId);

        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] List<Guid>? companyIds,
        [FromQuery] List<Guid>? categoryIds,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var paged = await productService.GetProductsPagedAsync(
            companyIds,
            categoryIds,
            page,
            pageSize);

        if (paged.IsFailed)
        {
            return BadRequest(paged.Errors);
        }

        return Ok(paged.Value);
    }
}
