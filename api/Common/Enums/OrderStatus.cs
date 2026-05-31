using System.Text.Json.Serialization;

namespace Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Draft,            // черновик
    PendingApproval,  // в ожидании ответа производства 
    Confirmed,        // производство готово взять заказ
    Rescheduled,      // перенесены сроки
    Rejected,         // отклонено 
    AwaitingShipment, // ожидает забора
    Shipped,          // Груз забрал логист
    Delivered        // Груз успешно досталвен манагеру
}