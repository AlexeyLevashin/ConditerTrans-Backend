namespace Contracts.Product.Responses;

public class GetProductsPagedResponse
{
    public List<ProductListItemResponse> Result { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
