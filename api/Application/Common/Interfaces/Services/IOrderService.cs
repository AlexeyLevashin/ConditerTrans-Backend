using Common.Enums;
using Contracts.Orders.Requests;
using Contracts.Orders.Responses;
using FluentResults;

namespace Application.Common.Interfaces.Services;

public interface IOrderService
{
    Task<Result> CreateAsync(Guid managerId, CreateOrderRequest request);
    Task<Result<GetCurrentOrderResponse>> GetCurrentDraftAsync(Guid managerId);
    Task<Result<GetOrderHistoryResponse>> GetHistoryAsync(Guid managerId);
    Task<Result<ManagerOrderDetailResponse>> GetByIdAsync(Guid managerId, Guid orderId);
    Task<Result<ManagerOrderDetailResponse>> AcceptManagerRescheduleAsync(
        Guid managerId,
        Guid orderId,
        AcceptManagerRescheduleRequest request);
    Task<Result<ManagerOrderDetailResponse>> RejectManagerRescheduleAsync(
        Guid managerId,
        Guid orderId,
        RejectManagerRescheduleRequest request);
    Task<Result> SubmitAsync(Guid managerId, Guid orderId, SubmitOrderRequest request);

    Task<Result<GetDispatcherOrdersResponse>> GetDispatcherOrdersAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        string? search,
        OrderStatus? status,
        int page,
        int pageSize);

    Task<Result<DispatcherOrderDetailResponse>> GetDispatcherOrderByIdAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId);

    Task<Result<DispatcherOrderDetailResponse>> ConfirmDispatcherOrderAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId);

    Task<Result<DispatcherOrderDetailResponse>> RejectDispatcherOrderAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId,
        RejectDispatcherOrderRequest request);

    Task<Result<DispatcherOrderDetailResponse>> RescheduleDispatcherOrderAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId,
        RescheduleDispatcherOrderRequest request);

    Task<Result<DispatcherOrderDetailResponse>> ReadyDispatcherOrderForShipmentAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId,
        ReadyForShipmentDispatcherOrderRequest request);

    Task<Result<DispatcherOrderDetailResponse>> HandoverDispatcherOrderAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId,
        HandoverDispatcherOrderRequest request);

    Task<Result<DispatcherRejectionReportResponse>> GetDispatcherRejectionReportAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        string? dateFrom,
        string? dateTo);

    Task<Result<DispatcherProductRatingReportResponse>> GetDispatcherProductRatingReportAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        string? dateFrom,
        string? dateTo);

    Task<Result<ManagerPartnerReliabilityResponse>> GetManagerPartnerReliabilityAsync(
        Guid userId,
        UserRole userRole,
        Guid purchasingCompanyId,
        Guid partnerCompanyId,
        string? dateFrom,
        string? dateTo,
        string? partnerType);
}
