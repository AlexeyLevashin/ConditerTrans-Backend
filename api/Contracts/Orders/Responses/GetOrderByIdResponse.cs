using System.Text.Json.Serialization;

namespace Contracts.Orders.Responses;

public class GetOrderByIdResponse
{
    public Guid Id { get; set; }

    [JsonPropertyName("order_items")]
    public List<OrderItemResponse> OrderItems { get; set; } = [];

    public decimal Amount { get; set; }
}

public class OrderItemResponse
{
    public Guid Id { get; set; }

    [JsonPropertyName("product_id")]
    public Guid ProductId { get; set; }

    public int Count { get; set; }
    public decimal Price { get; set; }
}
