using System.Globalization;
using Common.Enums;
using Contracts.Orders.Responses;
using Domain.Entities;

namespace Application.Orders;

public static class OrderMapper
{
    public static GetCurrentOrderResponse ToCurrentOrderDto(this Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        CreationDate = order.CreationDate,
        Status = order.Status,
        ProductionAddress = order.ProductionAddress,
        DeliveryAddress = order.DeliveryAddress,
        Lines = order.OrderLines.Select(ToLineDto).ToList()
    };

    private static OrderLineResponse ToLineDto(this OrderLine line)
    {
        var product = line.Product!;

        return new OrderLineResponse
        {
            Id = line.Id,
            ProductId = line.ProductId,
            QuantityOfUnits = line.QuantityOfUnits,
            ProductName = product.Name,
            ProductPrice = product.Price,
            FormattedQuantity = FormatQuantity(product.Quantity, product.UnitsOfMeasure)
        };
    }

    public static GetOrderHistoryResponse ToHistoryDto(this List<Order> orders) => new()
    {
        Result = orders.Select(o => new OrderHistoryItemResponse
        {
            Id = o.Id,
            Status = o.Status
        }).ToList()
    };

    public static GetCoordinatorPendingOrdersResponse ToCoordinatorPendingOrdersDto(this List<Order> orders) =>
        new()
        {
            Result = orders.Select(ToCoordinatorPendingOrderDto).ToList()
        };

    private static CoordinatorPendingOrderResponse ToCoordinatorPendingOrderDto(Order order)
    {
        var lines = order.OrderLines
            .Where(line => line.Product is not null)
            .Select(line =>
            {
                var product = line.Product!;
                return new CoordinatorOrderLineResponse
                {
                    ProductName = product.Name,
                    QuantityOfUnits = line.QuantityOfUnits,
                    ProductPrice = product.Price,
                    FormattedQuantity = FormatQuantity(product.Quantity, product.UnitsOfMeasure)
                };
            })
            .ToList();

        return new CoordinatorPendingOrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CreationDate = order.CreationDate,
            ProductionAddress = order.ProductionAddress,
            DeliveryAddress = order.DeliveryAddress,
            PaymentType = order.PaymentType,
            Amount = lines.Sum(line => line.QuantityOfUnits * line.ProductPrice),
            Lines = lines
        };
    }

    public static GetOrderByIdResponse ToOrderByIdDto(this Order order)
    {
        var items = order.OrderLines.Select(line =>
        {
            var product = line.Product!;
            return new OrderItemResponse
            {
                Id = line.Id,
                ProductId = line.ProductId,
                Count = line.QuantityOfUnits,
                Price = product.Price
            };
        }).ToList();

        return new GetOrderByIdResponse
        {
            Id = order.Id,
            OrderItems = items,
            Amount = items.Sum(i => i.Count * i.Price)
        };
    }

    private static string FormatQuantity(float quantity, UnitsOfMeasure unit)
    {
        var culture = CultureInfo.InvariantCulture;

        return unit switch
        {
            UnitsOfMeasure.Pieces => quantity.ToString(culture) + " шт",
            UnitsOfMeasure.Grams => quantity.ToString(culture) + " г",
            UnitsOfMeasure.Milliliters => quantity.ToString(culture) + " мл",
            _ => quantity.ToString(culture)
        };
    }
}
