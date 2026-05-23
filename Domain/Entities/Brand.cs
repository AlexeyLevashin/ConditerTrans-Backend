namespace Domain.Entities;

public class Brand
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; }
    public virtual ICollection<Model> Models { get; set; } = new List<Model>();
}