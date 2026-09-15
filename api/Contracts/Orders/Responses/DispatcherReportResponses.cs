namespace Contracts.Orders.Responses;

public class RejectionReportItemResponse
{
    public DateTime Date { get; set; }
    public int RejectionCount { get; set; }
}

public class RejectionDetailItemResponse
{
    public Guid OrderId { get; set; }
    public int OrderNumber { get; set; }
    public DateTime RejectedAt { get; set; }
    public string Reason { get; set; } = null!;
}

public class DispatcherRejectionDayResponse
{
    public DateTime Date { get; set; }
    public int Total { get; set; }
    public List<RejectionDetailItemResponse> Items { get; set; } = [];
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
