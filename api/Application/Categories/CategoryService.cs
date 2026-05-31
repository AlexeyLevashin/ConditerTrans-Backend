using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Contracts.Categories.Responses;
using FluentResults;
using Mapster;

namespace Application.Categories;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<Result<List<GetCategoryResponse>>> GetAllAsync()
    {
        var results = await categoryRepository.GetAllAsync();
        return results.Adapt<List<GetCategoryResponse>>();
    }
}