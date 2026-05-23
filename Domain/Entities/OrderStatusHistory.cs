using Common.Enums;

namespace Domain.Entities;

public class OrderStatusHistory
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime ChangeTime { get; set; }
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; }
    public Guid OrderStatusId { get; set; }
    public virtual OrderStatus OrderStatus { get; set; }
}