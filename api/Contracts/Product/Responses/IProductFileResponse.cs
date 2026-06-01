namespace Contracts.Product.Responses;

public interface IProductFileResponse
{
    Guid? FileId { get; set; }
    string? FileUrl { get; set; }
}
