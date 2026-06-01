using Application.Common.Interfaces.Services;
using Contracts.Product.Responses;

namespace Application.Products;

internal static class ProductFileUrlEnricher
{
    public static async Task EnrichAsync(
        IEnumerable<IProductFileResponse> products,
        IFileServiceClient fileServiceClient,
        CancellationToken cancellationToken = default)
    {
        var items = products.ToList();
        var fileIds = items
            .Where(p => p.FileId.HasValue)
            .Select(p => p.FileId!.Value)
            .ToList();

        if (fileIds.Count == 0)
        {
            return;
        }

        var urls = await fileServiceClient.GetUrlsByIdsAsync(fileIds, cancellationToken);

        foreach (var product in items)
        {
            if (product.FileId is { } fileId && urls.TryGetValue(fileId, out var url))
            {
                product.FileUrl = url;
            }
        }
    }
}
