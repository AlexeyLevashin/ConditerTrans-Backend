namespace Contracts.Categories.Responses;

public class GetCategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}