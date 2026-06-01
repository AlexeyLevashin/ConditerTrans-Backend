using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Common.Enums;
using Contracts.Orders.Requests;
using Contracts.Orders.Responses;
using Domain.Entities;
using FluentResults;

namespace Application.Orders;

public class OrderService(
    IOrderRepository orderRepository,
    IUserRepository userRepository,
    IProductRepository productRepository,
    ICompanyRepository companyRepository,
    IUnitOfWork unitOfWork) : IOrderService
{
    private const string DispatcherOnlyError = "Доступно только диспетчеру производства";

    public async Task<Result> CreateAsync(Guid managerId, CreateOrderRequest request)
    {
        var manager = await userRepository.GetByIdAsync(managerId);

        if (manager is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        if (!await productRepository.ExistsByIdAsync(request.ProductId))
        {
            return Result.Fail("Товар не найден");
        }

        var draftOrderId = await orderRepository.GetDraftOrderIdByManagerIdAsync(managerId);

        if (draftOrderId is null)
        {
            var order = new Order
            {
                ManagerId = managerId,
                Status = OrderStatus.Draft,
                CreationDate = DateTime.UtcNow
            };

            await orderRepository.CreateDraftAsync(order, request.ProductId, request.QuantityOfUnits);
        }
        else
        {
            await orderRepository.UpsertOrderLineAsync(
                draftOrderId.Value,
                request.ProductId,
                request.QuantityOfUnits);
        }

        await unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<GetCurrentOrderResponse>> GetCurrentDraftAsync(Guid managerId)
    {
        var order = await orderRepository.GetDraftByManagerIdAsync(managerId);

        if (order is null)
        {
            return Result.Fail("Черновик заказа не найден");
        }

        if (order.OrderLines.Any(line => line.Product is null))
        {
            return Result.Fail("Не удалось загрузить данные товаров в заказе");
        }

        return Result.Ok(order.ToCurrentOrderDto());
    }

    public async Task<Result<GetOrderHistoryResponse>> GetHistoryAsync(Guid managerId)
    {
        var orders = await orderRepository.GetAllByManagerIdAsync(managerId);
        return Result.Ok(orders.ToHistoryDto());
    }

    public async Task<Result<ManagerOrderDetailResponse>> GetByIdAsync(Guid managerId, Guid orderId)
    {
        var order = await orderRepository.GetByIdAndManagerIdAsync(orderId, managerId);

        if (order is null)
        {
            return Result.Fail("Заказ не найден");
        }

        if (order.Status == OrderStatus.Draft)
        {
            return Result.Fail("Заказ не найден");
        }

        if (order.OrderLines.Any(line => line.Product is null))
        {
            return Result.Fail("Не удалось загрузить данные товаров в заказе");
        }

        return Result.Ok(order.ToManagerDetailDto());
    }

    public async Task<Result<GetManagerRescheduledOrdersResponse>> GetRescheduledOrdersAsync(Guid managerId)
    {
        if (await userRepository.GetByIdAsync(managerId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var orders = await orderRepository.GetRescheduledByManagerIdAsync(managerId);
        return Result.Ok(orders.ToManagerRescheduledOrdersDto());
    }

    public async Task<Result<ManagerOrderDetailResponse>> AcceptManagerRescheduleAsync(
        Guid managerId,
        Guid orderId,
        AcceptManagerRescheduleRequest request)
    {
        var order = await orderRepository.GetByIdAndManagerIdAsync(orderId, managerId);
        if (order is null || order.Status != OrderStatus.Rescheduled)
        {
            return Result.Fail("Заказ не найден или не ожидает пересогласования");
        }

        if (!order.ProposedDeliveryDate.HasValue)
        {
            return Result.Fail("Нет предложения по срокам от производства");
        }

        var dateText = order.ProposedDeliveryDate.Value.ToString("yyyy-MM-dd");
        var comment = string.IsNullOrWhiteSpace(request.Comment)
            ? $"Менеджер согласовал перенос сроков на {dateText}"
            : $"{request.Comment.Trim()} (дата: {dateText})";

        await orderRepository.UpdateStatusAsync(
            orderId,
            OrderStatus.Confirmed,
            dispatcherId: null,
            productionAddress: null,
            comment: comment,
            clearRescheduleProposal: true);

        await unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(managerId, orderId);
    }

    public async Task<Result<ManagerOrderDetailResponse>> RejectManagerRescheduleAsync(
        Guid managerId,
        Guid orderId,
        RejectManagerRescheduleRequest request)
    {
        var order = await orderRepository.GetByIdAndManagerIdAsync(orderId, managerId);
        if (order is null || order.Status != OrderStatus.Rescheduled)
        {
            return Result.Fail("Заказ не найден или не ожидает пересогласования");
        }

        var comment = string.IsNullOrWhiteSpace(request.Reason)
            ? "Менеджер отклонил перенос сроков"
            : $"Менеджер отклонил перенос сроков: {request.Reason.Trim()}";

        await orderRepository.UpdateStatusAsync(
            orderId,
            OrderStatus.Rejected,
            dispatcherId: null,
            productionAddress: null,
            comment: comment,
            clearRescheduleProposal: true);

        await unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(managerId, orderId);
    }

    public async Task<Result> SubmitAsync(Guid managerId, Guid orderId, SubmitOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductionAddress))
        {
            return Result.Fail("Адрес погрузки обязателен");
        }

        if (string.IsNullOrWhiteSpace(request.DeliveryAddress))
        {
            return Result.Fail("Адрес доставки обязателен");
        }

        if (string.IsNullOrWhiteSpace(request.TypePayment))
        {
            return Result.Fail("Способ оплаты обязателен");
        }

        if (!await orderRepository.ExistsDraftForManagerAsync(orderId, managerId))
        {
            return Result.Fail("Заказ не найден");
        }

        if (!await orderRepository.HasOrderLinesAsync(orderId))
        {
            return Result.Fail("Нельзя отправить пустой заказ");
        }

        await orderRepository.SubmitDraftAsync(
            orderId,
            managerId,
            request.ProductionAddress.Trim(),
            request.DeliveryAddress.Trim(),
            request.TypePayment.Trim());

        await unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<GetDispatcherOrdersResponse>> GetDispatcherOrdersAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        string? search,
        OrderStatus? status)
    {
        var access = await EnsureDispatcherAsync(userId, userRole);
        if (access.IsFailed)
        {
            return Result.Fail(access.Errors);
        }

        var orders = await orderRepository.GetForDispatcherAsync(productionCompanyId, search, status);
        return Result.Ok(orders.ToDispatcherOrdersDto());
    }

    public async Task<Result<DispatcherOrderDetailResponse>> GetDispatcherOrderByIdAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId)
    {
        var access = await EnsureDispatcherAsync(userId, userRole);
        if (access.IsFailed)
        {
            return Result.Fail(access.Errors);
        }

        var order = await orderRepository.GetByIdForDispatcherAsync(orderId, productionCompanyId);
        if (order is null)
        {
            return Result.Fail("Заказ не найден");
        }

        if (order.OrderLines.Any(line => line.Product is null))
        {
            return Result.Fail("Не удалось загрузить данные товаров в заказе");
        }

        return Result.Ok(order.ToDispatcherDetailDto());
    }

    public Task<Result<DispatcherOrderDetailResponse>> ConfirmDispatcherOrderAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId) =>
        ChangeDispatcherOrderStatusAsync(
            userId,
            userRole,
            productionCompanyId,
            orderId,
            allowedStatuses: [OrderStatus.PendingApproval],
            newStatus: OrderStatus.Confirmed,
            comment: null,
            setProductionAddress: true);

    public async Task<Result<DispatcherOrderDetailResponse>> RejectDispatcherOrderAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId,
        RejectDispatcherOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Result.Fail("Причина отказа обязательна");
        }

        return await ChangeDispatcherOrderStatusAsync(
            userId,
            userRole,
            productionCompanyId,
            orderId,
            allowedStatuses: [OrderStatus.PendingApproval, OrderStatus.Rescheduled],
            newStatus: OrderStatus.Rejected,
            comment: request.Reason.Trim(),
            setProductionAddress: false);
    }

    public async Task<Result<DispatcherOrderDetailResponse>> RescheduleDispatcherOrderAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId,
        RescheduleDispatcherOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Result.Fail("Причина срыва сроков обязательна");
        }

        var proposedDate = request.NewDeliveryDate.Date;
        var reason = request.Reason.Trim();
        var comment = $"Предложен перенос на {proposedDate:yyyy-MM-dd}. {reason}";

        return await ChangeDispatcherOrderStatusAsync(
            userId,
            userRole,
            productionCompanyId,
            orderId,
            allowedStatuses: [OrderStatus.PendingApproval, OrderStatus.Confirmed, OrderStatus.Rescheduled],
            newStatus: OrderStatus.Rescheduled,
            comment: comment,
            setProductionAddress: false,
            proposedDeliveryDate: proposedDate,
            rescheduleReason: reason);
    }

    public async Task<Result<DispatcherOrderDetailResponse>> ReadyDispatcherOrderForShipmentAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId,
        ReadyForShipmentDispatcherOrderRequest request)
    {
        var comment = $"Готов к отправке: {request.ShipmentDate:yyyy-MM-dd}";

        return await ChangeDispatcherOrderStatusAsync(
            userId,
            userRole,
            productionCompanyId,
            orderId,
            allowedStatuses: [OrderStatus.Confirmed],
            newStatus: OrderStatus.AwaitingShipment,
            comment: comment,
            setProductionAddress: false);
    }

    public async Task<Result<DispatcherOrderDetailResponse>> HandoverDispatcherOrderAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId,
        HandoverDispatcherOrderRequest request)
    {
        if (!request.DocumentsHandedOver)
        {
            return Result.Fail("Подтвердите передачу документов");
        }

        return await ChangeDispatcherOrderStatusAsync(
            userId,
            userRole,
            productionCompanyId,
            orderId,
            allowedStatuses: [OrderStatus.AwaitingShipment],
            newStatus: OrderStatus.Shipped,
            comment: "Отгрузка со склада, документы переданы",
            setProductionAddress: false);
    }

    private async Task<Result<DispatcherOrderDetailResponse>> ChangeDispatcherOrderStatusAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        Guid orderId,
        OrderStatus[] allowedStatuses,
        OrderStatus newStatus,
        string? comment,
        bool setProductionAddress,
        DateTime? proposedDeliveryDate = null,
        string? rescheduleReason = null)
    {
        var access = await EnsureDispatcherAsync(userId, userRole);
        if (access.IsFailed)
        {
            return Result.Fail(access.Errors);
        }

        if (!await orderRepository.BelongsToProductionCompanyAsync(orderId, productionCompanyId))
        {
            return Result.Fail("Заказ не найден");
        }

        var order = await orderRepository.GetByIdForDispatcherAsync(orderId, productionCompanyId);
        if (order is null)
        {
            return Result.Fail("Заказ не найден");
        }

        if (!allowedStatuses.Contains(order.Status))
        {
            return Result.Fail("Недопустимый статус заказа для этого действия");
        }

        string? productionAddress = null;
        if (setProductionAddress)
        {
            var company = await companyRepository.GetByIdAsync(productionCompanyId);
            productionAddress = company?.Address;
        }

        await orderRepository.UpdateStatusAsync(
            orderId,
            newStatus,
            userId,
            productionAddress,
            comment,
            proposedDeliveryDate,
            rescheduleReason,
            clearRescheduleProposal: false);

        await unitOfWork.SaveChangesAsync();

        var updated = await orderRepository.GetByIdForDispatcherAsync(orderId, productionCompanyId);
        if (updated is null)
        {
            return Result.Fail("Заказ не найден");
        }

        return Result.Ok(updated.ToDispatcherDetailDto());
    }

    private async Task<Result> EnsureDispatcherAsync(Guid userId, UserRole userRole)
    {
        if (userRole != UserRole.Dispatcher)
        {
            return Result.Fail(DispatcherOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        return Result.Ok();
    }
}
