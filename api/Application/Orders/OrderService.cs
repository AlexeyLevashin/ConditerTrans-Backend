using Application.Common.Interfaces.Persistence;
using Common;
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
    ICargoRepository cargoRepository,
    IUserRepository userRepository,
    IProductRepository productRepository,
    ICompanyRepository companyRepository,
    IUnitOfWork unitOfWork) : IOrderService
{
    private const string DispatcherOnlyError = "Доступно только диспетчеру производства";
    private const string ManagerOnlyError = "Доступно только менеджеру по закупкам";

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

        var hasBlockingOrder = await orderRepository.HasBlockingManagerOrderAsync(managerId);
        var draftOrderId = await orderRepository.GetDraftOrderIdByManagerIdAsync(managerId);

        // Пока есть заказ на согласовании/пересогласовании — новые позиции только в новый черновик.
        if (hasBlockingOrder)
        {
            draftOrderId = null;
        }

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
        else if (!await orderRepository.UpsertOrderLineAsync(
                     draftOrderId.Value,
                     request.ProductId,
                     request.QuantityOfUnits))
        {
            return Result.Fail("Нельзя изменить заказ, который уже отправлен на согласование");
        }

        await unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<GetCurrentOrderResponse>> RepeatOrderAsync(Guid managerId, Guid sourceOrderId)
    {
        var manager = await userRepository.GetByIdAsync(managerId);
        if (manager is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        if (manager.UserRole != UserRole.Manager)
        {
            return Result.Fail(ManagerOnlyError);
        }

        var source = await orderRepository.GetByIdAndManagerIdAsync(sourceOrderId, managerId);
        if (source is null || source.Status == OrderStatus.Draft)
        {
            return Result.Fail("Заказ не найден");
        }

        var lines = source.OrderLines
            .Where(line => line.ProductId != Guid.Empty)
            .Select(line => (line.ProductId, line.QuantityOfUnits))
            .ToList();

        if (lines.Count == 0)
        {
            return Result.Fail("В заказе нет позиций для повтора");
        }

        foreach (var (productId, _) in lines)
        {
            if (!await productRepository.ExistsByIdAsync(productId))
            {
                return Result.Fail("Один из товаров больше недоступен");
            }
        }

        await orderRepository.DeleteDraftsByManagerIdAsync(managerId);

        var order = new Order
        {
            ManagerId = managerId,
            Status = OrderStatus.Draft,
            CreationDate = DateTime.UtcNow,
            DeliveryAddress = source.DeliveryAddress,
            ProductionAddress = source.ProductionAddress,
            PaymentType = source.PaymentType
        };

        await orderRepository.CreateDraftWithLinesAsync(order, lines);
        await unitOfWork.SaveChangesAsync();

        return await GetCurrentDraftAsync(managerId);
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

        var newDeliveryDate = order.ProposedDeliveryDate.Value.Date;

        await orderRepository.UpdateStatusAsync(
            orderId,
            OrderStatus.Confirmed,
            dispatcherId: null,
            productionAddress: null,
            comment: comment,
            clearRescheduleProposal: true);

        await orderRepository.SetRequestedDeliveryDateAsync(orderId, newDeliveryDate);
        await orderRepository.ClearDeadlineConfirmationAsync(orderId);

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

        if (request.RequestedDeliveryDate == default)
        {
            return Result.Fail("Укажите дату доставки");
        }

        if (request.RequestedDeliveryDate.Date < DateTime.UtcNow.Date)
        {
            return Result.Fail("Дата доставки не может быть в прошлом");
        }

        await orderRepository.SubmitDraftAsync(
            orderId,
            managerId,
            request.ProductionAddress.Trim(),
            request.DeliveryAddress.Trim(),
            request.TypePayment.Trim(),
            DateTimeUtc.FromDate(request.RequestedDeliveryDate));

        await unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<GetDispatcherOrdersResponse>> GetDispatcherOrdersAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        string? search,
        OrderStatus? status,
        int page,
        int pageSize)
    {
        var access = await EnsureDispatcherAsync(userId, userRole);
        if (access.IsFailed)
        {
            return Result.Fail(access.Errors);
        }

        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var (orders, totalCount) = await orderRepository.GetForDispatcherPagedAsync(
            productionCompanyId,
            search,
            status,
            safePage,
            safePageSize);

        var hasDeadline = await orderRepository.HasDispatcherOrdersRequiringDeadlineConfirmationAsync(
            productionCompanyId);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)safePageSize);

        return Result.Ok(orders.ToDispatcherOrdersDto(
            totalCount,
            safePage,
            safePageSize,
            totalPages,
            hasDeadline));
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

        var proposedDate = DateTimeUtc.FromDate(request.NewDeliveryDate);
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
        if (request.LengthM <= 0 || request.WidthM <= 0 || request.HeightM <= 0)
        {
            return Result.Fail("Укажите габариты: длина, ширина и высота в метрах");
        }

        if (request.WeightKg <= 0)
        {
            return Result.Fail("Укажите вес груза в килограммах");
        }

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

        if (order.Status != OrderStatus.Confirmed)
        {
            return Result.Fail("Недопустимый статус заказа для этого действия");
        }

        if (order.CargoId.HasValue)
        {
            return Result.Fail("Груз по этому заказу уже создан");
        }

        var shipmentDate = DateTimeUtc.FromDate(request.ShipmentDate);
        var volume = request.LengthM * request.WidthM * request.HeightM;
        var dimensionsText = FormatShipmentDimensions(request.LengthM, request.WidthM, request.HeightM);
        var now = DateTime.UtcNow;

        var cargo = new Cargo
        {
            LoadingDate = shipmentDate,
            UnloadingDate = shipmentDate.AddDays(7),
            DeliveryAddress = order.DeliveryAddress ?? "Адрес не указан",
            Weight = request.WeightKg,
            Volume = volume,
            Dimensions = dimensionsText,
            Status = CargoStatus.NotAssignedToLogisticCompany,
            Histories =
            [
                new CargoChangeHistory
                {
                    CargoStatus = CargoStatus.NotAssignedToLogisticCompany,
                    ChangeTime = now
                }
            ]
        };

        await cargoRepository.AddAsync(cargo);

        var comment =
            $"Готов к отправке: {shipmentDate:yyyy-MM-dd}, {dimensionsText}, {request.WeightKg:0.###} кг";

        var marked = await orderRepository.MarkReadyForShipmentAsync(
            orderId,
            userId,
            shipmentDate,
            request.LengthM,
            request.WidthM,
            request.HeightM,
            request.WeightKg,
            cargo.Id,
            comment);

        if (!marked)
        {
            return Result.Fail("Не удалось подтвердить готовность к отправке");
        }

        await unitOfWork.SaveChangesAsync();

        var updated = await orderRepository.GetByIdForDispatcherAsync(orderId, productionCompanyId);
        if (updated is null)
        {
            return Result.Fail("Заказ не найден");
        }

        return Result.Ok(updated.ToDispatcherDetailDto());
    }

    private static string FormatShipmentDimensions(decimal lengthM, decimal widthM, decimal heightM) =>
        $"{lengthM:0.##}×{widthM:0.##}×{heightM:0.##} м";

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

    public async Task<Result<DispatcherRejectionReportResponse>> GetDispatcherRejectionReportAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        string? dateFrom,
        string? dateTo)
    {
        var access = await EnsureDispatcherAsync(userId, userRole);
        if (access.IsFailed)
        {
            return Result.Fail(access.Errors);
        }

        var period = ParseReportPeriod(dateFrom, dateTo);
        if (period.IsFailed)
        {
            return Result.Fail(period.Errors);
        }

        var stats = await orderRepository.GetRejectionStatisticsAsync(
            productionCompanyId,
            period.Value.FromUtc,
            period.Value.ToUtcExclusive);

        var total = stats.Sum(item => item.OrderCount);
        var result = stats.Select(item => new RejectionReportItemResponse
        {
            Reason = item.Reason,
            OrderCount = item.OrderCount,
            SharePercent = total == 0
                ? 0
                : Math.Round(item.OrderCount * 100m / total, 1)
        }).ToList();

        return Result.Ok(new DispatcherRejectionReportResponse { Result = result });
    }

    public async Task<Result<DispatcherProductRatingReportResponse>> GetDispatcherProductRatingReportAsync(
        Guid userId,
        UserRole userRole,
        Guid productionCompanyId,
        string? dateFrom,
        string? dateTo)
    {
        var access = await EnsureDispatcherAsync(userId, userRole);
        if (access.IsFailed)
        {
            return Result.Fail(access.Errors);
        }

        var period = ParseReportPeriod(dateFrom, dateTo);
        if (period.IsFailed)
        {
            return Result.Fail(period.Errors);
        }

        var stats = await orderRepository.GetProductRatingAsync(
            productionCompanyId,
            period.Value.FromUtc,
            period.Value.ToUtcExclusive);

        var result = stats
            .Select((item, index) => new ProductRatingItemResponse
            {
                Rank = index + 1,
                Name = item.ProductName,
                OrderCount = item.OrderCount
            })
            .ToList();

        return Result.Ok(new DispatcherProductRatingReportResponse { Result = result });
    }

    public async Task<Result<ManagerPartnerReliabilityResponse>> GetManagerPartnerReliabilityAsync(
        Guid userId,
        UserRole userRole,
        Guid purchasingCompanyId,
        Guid partnerCompanyId,
        string? dateFrom,
        string? dateTo,
        string? partnerType)
    {
        var access = await EnsureManagerAsync(userId, userRole);
        if (access.IsFailed)
        {
            return Result.Fail(access.Errors);
        }

        var company = await companyRepository.GetByIdAsync(partnerCompanyId);
        if (company is null)
        {
            return Result.Fail("Компания-партнёр не найдена");
        }

        if (company.CompanyType == CompanyType.PurchasingCompany)
        {
            return Result.Fail("Нельзя анализировать компанию-заказчика как партнёра");
        }

        var kindResult = ResolvePartnerAnalysisKind(company.CompanyType, partnerType);
        if (kindResult.IsFailed)
        {
            return Result.Fail(kindResult.Errors);
        }

        var period = ParseReportPeriod(dateFrom, dateTo);
        if (period.IsFailed)
        {
            return Result.Fail(period.Errors);
        }

        var facts = await orderRepository.GetPartnerReliabilityFactsAsync(
            purchasingCompanyId,
            partnerCompanyId,
            kindResult.Value,
            period.Value.FromUtc,
            period.Value.ToUtcExclusive);

        var response = PartnerReliabilityCalculator.Build(
            company.Id,
            company.Name,
            kindResult.Value,
            dateFrom,
            dateTo,
            facts);

        return Result.Ok(response);
    }

    private static Result<PartnerAnalysisKind> ResolvePartnerAnalysisKind(
        CompanyType companyType,
        string? partnerType)
    {
        if (!string.IsNullOrWhiteSpace(partnerType))
        {
            return partnerType.Trim().ToLowerInvariant() switch
            {
                "production" or "производство" => Result.Ok(PartnerAnalysisKind.Production),
                "transport" or "транспорт" or "logistic" or "логистика" => Result.Ok(PartnerAnalysisKind.Transport),
                _ => Result.Fail("partnerType: ожидается production или transport")
            };
        }

        return companyType switch
        {
            CompanyType.LogisticCompany => Result.Ok(PartnerAnalysisKind.Transport),
            CompanyType.ProductionDispatcher => Result.Ok(PartnerAnalysisKind.Production),
            _ => Result.Fail("Не удалось определить тип партнёра. Укажите partnerType=production или transport")
        };
    }

    private static Result<(DateTime? FromUtc, DateTime? ToUtcExclusive)> ParseReportPeriod(
        string? dateFrom,
        string? dateTo)
    {
        DateTime? fromUtc = null;
        DateTime? toUtcExclusive = null;

        if (!string.IsNullOrWhiteSpace(dateFrom))
        {
            if (!DateTime.TryParse(dateFrom, out var parsedFrom))
            {
                return Result.Fail("Некорректная дата начала периода (ожидается YYYY-MM-DD)");
            }

            fromUtc = DateTimeUtc.FromDate(parsedFrom);
        }

        if (!string.IsNullOrWhiteSpace(dateTo))
        {
            if (!DateTime.TryParse(dateTo, out var parsedTo))
            {
                return Result.Fail("Некорректная дата окончания периода (ожидается YYYY-MM-DD)");
            }

            toUtcExclusive = DateTimeUtc.FromDate(parsedTo).AddDays(1);
        }

        if (fromUtc.HasValue && toUtcExclusive.HasValue && fromUtc >= toUtcExclusive)
        {
            return Result.Fail("Дата начала не может быть позже даты окончания");
        }

        return Result.Ok((fromUtc, toUtcExclusive));
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

    private async Task<Result> EnsureManagerAsync(Guid userId, UserRole userRole)
    {
        if (userRole != UserRole.Manager)
        {
            return Result.Fail(ManagerOnlyError);
        }

        if (await userRepository.GetByIdAsync(userId) is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        return Result.Ok();
    }
}
