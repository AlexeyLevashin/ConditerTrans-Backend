using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("vehicle_models")]
public class VehicleModel
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("brand_id")]
    public Guid BrandId { get; set; }

    public VehicleBrand Brand { get; set; } = null!;
    public List<TransportVehicle> TransportVehicles { get; set; } = [];
}
