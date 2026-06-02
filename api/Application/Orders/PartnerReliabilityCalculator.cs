using Contracts.Orders.Responses;

namespace Application.Orders;

public static class PartnerReliabilityCalculator
{
    public static ManagerPartnerReliabilityResponse Build(
        Guid companyId,
        string companyName,
        PartnerAnalysisKind partnerType,
        string? periodFrom,
        string? periodTo,
        IReadOnlyList<PartnerReliabilityOrderFact> facts)
    {
        var completed = facts.Count;
        var withAgreedDate = facts.Where(f => f.RequestedDeliveryDateUtc.HasValue).ToList();

        var onTime = 0;
        var late = 0;
        decimal delaySum = 0;

        foreach (var fact in withAgreedDate)
        {
            var requestedDate = fact.RequestedDeliveryDateUtc!.Value.Date;
            var actualDate = fact.ActualDeliveryUtc.Date;

            if (actualDate <= requestedDate)
            {
                onTime++;
            }
            else
            {
                late++;
                delaySum += (actualDate - requestedDate).Days;
            }
        }

        var withDateCount = withAgreedDate.Count;
        var rescheduled = facts.Count(f => f.HadReschedule);

        return new ManagerPartnerReliabilityResponse
        {
            CompanyId = companyId,
            CompanyName = companyName,
            PartnerType = partnerType == PartnerAnalysisKind.Transport ? "Transport" : "Production",
            PeriodFrom = periodFrom,
            PeriodTo = periodTo,
            CompletedOrdersCount = completed,
            DeadlineCompliance = new PartnerDeadlineComplianceResponse
            {
                OrdersWithAgreedDate = withDateCount,
                OnTimeCount = onTime,
                LateCount = late,
                OnTimePercent = withDateCount == 0
                    ? 0
                    : Math.Round(onTime * 100m / withDateCount, 1),
                AverageDelayDays = late == 0
                    ? 0
                    : Math.Round(delaySum / late, 1)
            },
            SupplyQuality = new PartnerSupplyQualityResponse
            {
                RescheduledOrdersCount = rescheduled,
                ReschedulePercent = completed == 0
                    ? 0
                    : Math.Round(rescheduled * 100m / completed, 1),
                QualityPercent = completed == 0
                    ? 0
                    : Math.Round((completed - rescheduled) * 100m / completed, 1)
            }
        };
    }
}
