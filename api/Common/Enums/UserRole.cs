using System.Text.Json.Serialization;

namespace Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Manager,
    Dispatcher,
    Coordinator
}