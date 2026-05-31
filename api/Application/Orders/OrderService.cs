using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Repositories;
using Application.Common.Interfaces.Services;
using Common.Enums;
using Contracts.Orders.Requests;
using Domain.Entities;
using FluentResults;

namespace Application.Orders;

public class OrderService(IOrderRepository orderRepository, IUserRepository userRepository, IProductRepository productRepository, IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<Result> CreateAsync(Guid managerId, CreateOrderRequest request)
    {
        var manager = await userRepository.GetByIdAsync(managerId);

        if (manager is null)
        {
            return Result.Fail("Пользователь не найден");
        }

        var product = await productRepository.GetProductByIdAsync(request.ProductId);
        
        if (product is null)
        {
            return Result.Fail("Товар не найден");
        }

        var order = await orderRepository.GetDraftByManagerIdAsync(managerId);

        if (order is null)
        {
            order = new Order
            {
                ManagerId = managerId,
                Status = OrderStatus.Draft,
                CreationDate = DateTime.UtcNow
            };
            
            order.Histories.Add(new OrderChangeHistory
            {
                OrderStatus = OrderStatus.Draft,
                ChangeTime = DateTime.UtcNow
            });
            
            order.OrderLines.Add(new OrderLine
            {
                ProductId = request.ProductId,
                QuantityOfUnits = request.QuantityOfUnits
            });

            await orderRepository.CreateAsync(order);
        }
        else
        {
            var existingLine = order.OrderLines.FirstOrDefault(ol => ol.ProductId == request.ProductId);

            if (existingLine != null)
            {
                existingLine.QuantityOfUnits += request.QuantityOfUnits;
            }
            else
            {
                order.OrderLines.Add(new OrderLine
                {
                    ProductId = request.ProductId,
                    QuantityOfUnits = request.QuantityOfUnits
                });
            }
            orderRepository.Update(order);
        }

        await unitOfWork.SaveChangesAsync();

        return Result.Ok();
    }
}