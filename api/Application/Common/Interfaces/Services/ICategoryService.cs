using Contracts.Categories.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface ICategoryService
{
    public Task<Result<List<GetCategoryResponse>>> GetAllAsync();
}