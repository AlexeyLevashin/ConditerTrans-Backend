using Common.Enums;

namespace Common;

public static class PaymentMethodHelper
{
    public static string ToStorageValue(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => nameof(PaymentMethod.Cash),
        PaymentMethod.Card => nameof(PaymentMethod.Card),
        PaymentMethod.BankTransfer => nameof(PaymentMethod.BankTransfer),
        _ => method.ToString()
    };

    public static string ToDisplayLabel(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "Наличные",
        PaymentMethod.Card => "Карта",
        PaymentMethod.BankTransfer => "Перевод",
        _ => method.ToString()
    };

    public static bool TryParse(string? value, out PaymentMethod method)
    {
        method = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();

        if (Enum.TryParse<PaymentMethod>(normalized, true, out method))
        {
            return true;
        }

        method = normalized.ToLowerInvariant() switch
        {
            "cash" or "наличные" => PaymentMethod.Cash,
            "card" or "карта" => PaymentMethod.Card,
            "invoice" or "перевод" or "banktransfer" or "bank_transfer" => PaymentMethod.BankTransfer,
            _ => (PaymentMethod)(-1)
        };

        if (!Enum.IsDefined(method))
        {
            return false;
        }

        return true;
    }

    public static string? ToDisplayLabelOrRaw(string? stored)
    {
        return TryParse(stored, out var method) ? ToDisplayLabel(method) : stored;
    }
}
