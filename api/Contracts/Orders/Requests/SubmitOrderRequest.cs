using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Common.Enums;

namespace Contracts.Orders.Requests;

public class SubmitOrderRequest : IValidatableObject
{
    public const int MinDeliveryDaysAhead = 2;

    [Required(ErrorMessage = "Адрес погрузки обязателен")]
    [MinLength(1, ErrorMessage = "Адрес погрузки обязателен")]
    [JsonPropertyName("production_address")]
    public string ProductionAddress { get; set; } = null!;

    [Required(ErrorMessage = "Адрес доставки обязателен")]
    [MinLength(1, ErrorMessage = "Адрес доставки обязателен")]
    [JsonPropertyName("delivery_address")]
    public string DeliveryAddress { get; set; } = null!;

    [Required(ErrorMessage = "Способ оплаты обязателен")]
    [JsonPropertyName("payment_method")]
    public PaymentMethod? PaymentMethod { get; set; }

    [Required(ErrorMessage = "Укажите желаемую дату поставки")]
    [JsonPropertyName("requested_delivery_date")]
    public DateTime? RequestedDeliveryDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PaymentMethod.HasValue && !Enum.IsDefined(PaymentMethod.Value))
        {
            yield return new ValidationResult(
                "Недопустимый способ оплаты. Допустимые значения: Cash, Card, BankTransfer",
                [nameof(PaymentMethod)]);
        }

        if (!RequestedDeliveryDate.HasValue)
        {
            yield break;
        }

        var deliveryDate = RequestedDeliveryDate.Value.Date;
        var minAllowedDate = DateTime.UtcNow.Date.AddDays(MinDeliveryDaysAhead);

        if (deliveryDate < minAllowedDate)
        {
            yield return new ValidationResult(
                $"Дата поставки должна быть не раньше {minAllowedDate:yyyy-MM-dd} (минимум через {MinDeliveryDaysAhead} дня от текущей даты)",
                [nameof(RequestedDeliveryDate)]);
        }
    }
}
