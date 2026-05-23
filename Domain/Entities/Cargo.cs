namespace Domain.Entities;

public class Cargo
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public int CargoNumber { get; set; }
    public DateTime? LoadingDate { get; set; }
    public DateTime? UnloadingDate { get; set; }
    public string DeliveryAddress { get; set; }
    public double Volume { get; set; }
    public double Weight { get; set; }
    public Guid RouteSheetId { get; set; }
    public virtual RouteSheet RouteSheet { get; set; }
    public virtual ICollection<CargoStatusHistory> StatusHistories { get; set; } = new List<CargoStatusHistory>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}