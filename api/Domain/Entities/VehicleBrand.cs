using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("vehicle_brands")]
public class VehicleBrand
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("name")]
    public string Name { get; set; } = null!;

    public List<VehicleModel> Models { get; set; } = [];
}
