using System.Text.Json.Serialization;

namespace Contracts.Orders.Requests;

public class SubmitOrderRequest
{
    [JsonPropertyName("production_address")]
    public string ProductionAddress { get; set; } = null!;

    [JsonPropertyName("delivery_address")]
    public string DeliveryAddress { get; set; } = null!;

    [JsonPropertyName("type_payment")]
    public string TypePayment { get; set; } = null!;
}
