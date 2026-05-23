namespace Domain.Entities;

public class Model
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; }
    public Guid BrandId { get; set; }
    public virtual Brand Brand { get; set; }
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}