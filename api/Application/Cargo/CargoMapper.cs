using System.Globalization;
using Common.Enums;
using Contracts.Cargo.Responses;
using Domain.Entities;

namespace Application.CargoHandling;

internal static class CargoMapper
{
    public static CargoListResponse ToListDto(this IEnumerable<Domain.Entities.Cargo> cargos) =>
        new()
        {
            Result = cargos.Select(MapItem).ToList()
        };

    public static CargoItemResponse ToItemDto(this Domain.Entities.Cargo cargo) => MapItem(cargo);

    private static CargoItemResponse MapItem(Domain.Entities.Cargo cargo)
    {
        var order = cargo.Order;
        var lines = order?.OrderLines
            .Where(line => line.Product is not null)
            .Select(line =>
            {
                var product = line.Product!;
                return new CargoOrderLineResponse
                {
                    ProductName = product.Name,
                    QuantityOfUnits = line.QuantityOfUnits,
                    ProductPrice = product.Price,
                    FormattedQuantity = FormatQuantity(product.Quantity, product.UnitsOfMeasure)
                };
            })
            .ToList() ?? [];

        return new CargoItemResponse
        {
            Id = cargo.Id,
            OrderId = order?.Id,
            OrderNumber = order is { OrderNumber: > 0 } ? order.OrderNumber : null,
            LoadingDate = cargo.LoadingDate,
            UnloadingDate = cargo.UnloadingDate,
            DeliveryAddress = cargo.DeliveryAddress,
            ProductionAddress = order?.ProductionAddress,
            Volume = cargo.Volume,
            Weight = cargo.Weight,
            Dimensions = cargo.Dimensions,
            Status = cargo.Status,
            DriverId = cargo.DriverId,
            DriverName = FormatDriverName(cargo.Driver),
            OrderAmount = lines.Count > 0 ? lines.Sum(line => line.QuantityOfUnits * line.ProductPrice) : null,
            PaymentType = order?.PaymentType,
            OrderLines = lines
        };
    }

    private static string? FormatDriverName(User? driver)
    {
        if (driver?.Employee is null)
        {
            return null;
        }

        return string.Join(
            ' ',
            new[] { driver.Employee.Surname, driver.Employee.Name, driver.Employee.Patronymic }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
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
