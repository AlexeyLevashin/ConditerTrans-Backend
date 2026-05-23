namespace Domain.Entities;

public class RouteSheet
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string? Name { get; set; }
    public Guid VehicleId { get; set; }
    public virtual Vehicle Vehicle { get; set; }
    public virtual ICollection<Cargo> Cargoes { get; set; } = new List<Cargo>();
}