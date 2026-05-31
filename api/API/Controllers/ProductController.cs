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
            return NotFound();
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery]List<Guid>? companyIds, [FromQuery]List<Guid>? categoryIds)
    {
        var results = await productService.GetAllProductsAsync(companyIds, categoryIds);
        
        if (results.IsFailed)
        {
            return NotFound();
        }
        
        return Ok(results.Value);
    }
}