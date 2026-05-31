using System.Text.Json.Serialization;

namespace Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CargoStatus
{
    NotAssignedToLogisticCompany,
    AwaitingTransportation,
    PickedUpFromProduction,
    Delivered,
    Cancelled
}
