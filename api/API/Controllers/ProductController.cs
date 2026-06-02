using API.Controllers.Abstractions;
using Application.Common.Interfaces.Services;
using FluentResults;
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
        [FromQuery] int? page,
        [FromQuery] int pageSize = 20)
    {
        if (page.HasValue)
        {
            var paged = await productService.GetProductsPagedAsync(
                companyIds,
                categoryIds,
                page.Value,
                pageSize);

            if (paged.IsFailed)
            {
                return BadRequest(paged.Errors);
            }

            return Ok(paged.Value);
        }

        var results = await productService.GetAllProductsAsync(companyIds, categoryIds);

        if (results.IsFailed)
        {
            return BadRequest(results.Errors);
        }

        return Ok(results.Value);
    }
}