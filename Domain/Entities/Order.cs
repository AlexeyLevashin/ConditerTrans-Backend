namespace Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string OrderCode { get; set; }
    public DateTime CreationDate { get; set; }
    public bool IsRegularDelivery { get; set; }
    public string ProductionAddress { get; set; }
    public string DeliveryAddress { get; set; }
    public Guid? CargoId { get; set; }
    public virtual Cargo? Cargo { get; set; }
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
}