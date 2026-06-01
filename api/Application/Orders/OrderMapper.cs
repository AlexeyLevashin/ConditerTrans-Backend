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
        OrderNumber = order.OrderNumber > 0 ? order.OrderNumber : 0,
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
        Result = orders.Select(ToManagerListItemDto).Select(ToHistoryItemDto).ToList()
    };

    public static ManagerOrderDetailResponse ToManagerDetailDto(this Order order)
    {
        var listItem = ToManagerListItemDto(order);
        return new ManagerOrderDetailResponse
        {
            Id = listItem.Id,
            OrderNumber = listItem.OrderNumber,
            CreationDate = listItem.CreationDate,
            Status = listItem.Status,
            ProductionAddress = listItem.ProductionAddress,
            DeliveryAddress = listItem.DeliveryAddress,
            PaymentType = listItem.PaymentType,
            Amount = listItem.Amount,
            Reschedule = listItem.Reschedule,
            Lines = order.OrderLines
                .Where(line => line.Product is not null)
                .Select(ToCoordinatorLineDto)
                .ToList()
        };
    }

    private static OrderHistoryItemResponse ToHistoryItemDto(ManagerOrderListItemResponse item) => new()
    {
        Id = item.Id,
        OrderNumber = item.OrderNumber,
        CreationDate = item.CreationDate,
        Status = item.Status,
        Amount = item.Amount,
        Reschedule = item.Reschedule
    };

    private static ManagerOrderListItemResponse ToManagerListItemDto(Order order)
    {
        var lines = order.OrderLines
            .Where(line => line.Product is not null)
            .Select(ToCoordinatorLineDto)
            .ToList();

        return new ManagerOrderListItemResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CreationDate = order.CreationDate,
            Status = order.Status,
            ProductionAddress = order.ProductionAddress,
            DeliveryAddress = order.DeliveryAddress,
            PaymentType = order.PaymentType,
            Amount = lines.Sum(line => line.QuantityOfUnits * line.ProductPrice),
            Reschedule = ToRescheduleProposalDto(order)
        };
    }

    private static RescheduleProposalResponse? ToRescheduleProposalDto(Order order)
    {
        if (order.Status != OrderStatus.Rescheduled ||
            !order.ProposedDeliveryDate.HasValue ||
            string.IsNullOrWhiteSpace(order.RescheduleReason))
        {
            return null;
        }

        return new RescheduleProposalResponse
        {
            ProposedDeliveryDate = order.ProposedDeliveryDate.Value,
            Reason = order.RescheduleReason
        };
    }

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

    public static GetDispatcherOrdersResponse ToDispatcherOrdersDto(this List<Order> orders) => new()
    {
        Result = orders.Select(ToDispatcherListItemDto).ToList()
    };

    public static DispatcherOrderDetailResponse ToDispatcherDetailDto(this Order order)
    {
        var listItem = ToDispatcherListItemDto(order);
        var driver = order.Cargo?.Driver?.Employee;

        return new DispatcherOrderDetailResponse
        {
            Id = listItem.Id,
            OrderNumber = listItem.OrderNumber,
            CompanyName = listItem.CompanyName,
            CreationDate = listItem.CreationDate,
            DeliveryAddress = listItem.DeliveryAddress,
            Status = listItem.Status,
            Amount = listItem.Amount,
            PaymentType = listItem.PaymentType,
            ProductionAddress = order.ProductionAddress,
            Lines = order.OrderLines
                .Where(line => line.Product is not null)
                .Select(line => ToCoordinatorLineDto(line))
                .ToList(),
            HandoverDriver = driver is null
                ? null
                : FormatEmployeeName(driver.Surname, driver.Name, driver.Patronymic),
            HandoverVehicle = null
        };
    }

    private static DispatcherOrderListItemResponse ToDispatcherListItemDto(Order order)
    {
        var lines = order.OrderLines
            .Where(line => line.Product is not null)
            .Select(ToCoordinatorLineDto)
            .ToList();

        return new DispatcherOrderListItemResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber > 0 ? order.OrderNumber : 0,
            CompanyName = order.Manager?.Employee?.Company?.Name ?? "—",
            CreationDate = order.CreationDate,
            DeliveryAddress = order.DeliveryAddress,
            Status = order.Status,
            PaymentType = order.PaymentType,
            Amount = lines.Sum(line => line.QuantityOfUnits * line.ProductPrice),
            RequestedDeliveryDate = order.RequestedDeliveryDate,
            RequiresDeadlineConfirmation = RequiresDeadlineConfirmation(order),
            DeadlineConfirmationExpiresAt = order.DeadlineConfirmationExpiresAt,
            DeadlineConfirmationPhase = order.DeadlineConfirmationPhase
        };
    }

    private static CoordinatorOrderLineResponse ToCoordinatorLineDto(OrderLine line)
    {
        var product = line.Product!;
        return new CoordinatorOrderLineResponse
        {
            ProductName = product.Name,
            QuantityOfUnits = line.QuantityOfUnits,
            ProductPrice = product.Price,
            FormattedQuantity = FormatQuantity(product.Quantity, product.UnitsOfMeasure)
        };
    }

    private static bool RequiresDeadlineConfirmation(Order order) =>
        order.Status == OrderStatus.Confirmed &&
        order.CargoId == null &&
        order.DeadlineConfirmationPhase != DeadlineConfirmationPhase.None;

    private static string FormatEmployeeName(string surname, string name, string? patronymic) =>
        string.Join(' ', new[] { surname, name, patronymic }.Where(part => !string.IsNullOrWhiteSpace(part)));

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
