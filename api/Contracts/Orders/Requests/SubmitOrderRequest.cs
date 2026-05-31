using System.Text.Json.Serialization;

namespace Contracts.Orders.Requests;

public class SubmitOrderRequest
{
    public string Address { get; set; } = null!;

    [JsonPropertyName("type_payment")]
    public string TypePayment { get; set; } = null!;
}
