namespace Contracts.Orders.Responses;

/// <summary>Анализ надёжности партнёра (производство или транспорт) для менеджера по закупкам.</summary>
public class ManagerPartnerReliabilityResponse
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = null!;
    /// <summary>Production или Transport.</summary>
    public string PartnerType { get; set; } = null!;
    public string? PeriodFrom { get; set; }
    public string? PeriodTo { get; set; }
    /// <summary>Только заказы в статусе Delivered.</summary>
    public int CompletedOrdersCount { get; set; }
    public PartnerDeadlineComplianceResponse DeadlineCompliance { get; set; } = new();
    public PartnerSupplyQualityResponse SupplyQuality { get; set; } = new();
}

public class PartnerDeadlineComplianceResponse
{
    public int OrdersWithAgreedDate { get; set; }
    public int OnTimeCount { get; set; }
    public int LateCount { get; set; }
    public decimal OnTimePercent { get; set; }
    public decimal AverageDelayDays { get; set; }
}

public class PartnerSupplyQualityResponse
{
    public int RescheduledOrdersCount { get; set; }
    public decimal ReschedulePercent { get; set; }
    /// <summary>Доля завершённых заказов без переноса сроков (качество поставок).</summary>
    public decimal QualityPercent { get; set; }
}
