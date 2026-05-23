namespace Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public int Quantity { get; set; }
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; }
}