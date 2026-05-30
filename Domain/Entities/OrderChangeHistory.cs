using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace Domain.Entities;

[Table("order_change_histories")]
public class OrderChangeHistory
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("change_time")]
    public DateTime ChangeTime { get; set; }

    [Column("order_status")]
    public OrderStatus OrderStatus { get; set; }

    [Column("order_id")]
    public Guid OrderId { get; set; }
}