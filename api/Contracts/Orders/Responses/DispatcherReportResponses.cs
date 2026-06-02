namespace Contracts.Orders.Responses;

public class RejectionReportItemResponse
{
    public string Reason { get; set; } = null!;
    public int OrderCount { get; set; }
    public decimal SharePercent { get; set; }
}

public class ProductRatingItemResponse
{
    public int Rank { get; set; }
    public string Name { get; set; } = null!;
    public int OrderCount { get; set; }
}

public class DispatcherRejectionReportResponse
{
    public List<RejectionReportItemResponse> Result { get; set; } = [];
}

public class DispatcherProductRatingReportResponse
{
    public List<ProductRatingItemResponse> Result { get; set; } = [];
}
