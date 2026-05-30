using System.Text.Json.Serialization;

namespace Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Draft,
    PendingApproval,
    Confirmed,
    Rescheduled,
    Rejected ,
    AwaitingShipment,
    Shipped,
    Delivered,
    Cancelled
}