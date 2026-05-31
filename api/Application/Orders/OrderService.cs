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
    IUnitOfWork unitOfWork) : IOrderService
{
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

    public async Task<Result<GetOrderByIdResponse>> GetByIdAsync(Guid managerId, Guid orderId)
    {
        var order = await orderRepository.GetByIdAndManagerIdAsync(orderId, managerId);

        if (order is null)
        {
            return Result.Fail("Заказ не найден");
        }

        if (order.OrderLines.Any(line => line.Product is null))
        {
            return Result.Fail("Не удалось загрузить данные товаров в заказе");
        }

        return Result.Ok(order.ToOrderByIdDto());
    }

    public async Task<Result> SubmitAsync(Guid managerId, Guid orderId, SubmitOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Address))
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
            request.Address.Trim(),
            request.TypePayment.Trim());

        await unitOfWork.SaveChangesAsync();
        return Result.Ok();
    }
}
