using Common;
using Common.Enums;
using Contracts.Orders.Requests;
using FluentResults;

namespace Application.Orders;

public static class SubmitOrderRequestValidator
{
    public static Result Validate(SubmitOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductionAddress))
        {
            return Result.Fail("Адрес погрузки обязателен");
        }

        if (string.IsNullOrWhiteSpace(request.DeliveryAddress))
        {
            return Result.Fail("Адрес доставки обязателен");
        }

        if (!request.PaymentMethod.HasValue)
        {
            return Result.Fail("Способ оплаты обязателен");
        }

        if (!Enum.IsDefined(request.PaymentMethod.Value))
        {
            return Result.Fail("Недопустимый способ оплаты. Допустимые значения: Cash, Card, BankTransfer");
        }

        if (!request.RequestedDeliveryDate.HasValue)
        {
            return Result.Fail("Укажите желаемую дату поставки");
        }

        var deliveryDate = request.RequestedDeliveryDate.Value.Date;
        var minAllowedDate = DateTime.UtcNow.Date.AddDays(SubmitOrderRequest.MinDeliveryDaysAhead);

        if (deliveryDate < minAllowedDate)
        {
            return Result.Fail(
                $"Дата поставки должна быть не раньше {minAllowedDate:yyyy-MM-dd} (минимум через {SubmitOrderRequest.MinDeliveryDaysAhead} дня от текущей даты)");
        }

        return Result.Ok();
    }

    public static string ToPaymentStorage(SubmitOrderRequest request) =>
        PaymentMethodHelper.ToStorageValue(request.PaymentMethod!.Value);

    public static DateTime ToRequestedDeliveryUtc(SubmitOrderRequest request) =>
        DateTimeUtc.FromDate(request.RequestedDeliveryDate!.Value);
}
