using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;

namespace Domain.Entities;

[Table("cargo_change_histories")]
public class CargoChangeHistory
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("change_time")]
    public DateTime ChangeTime { get; set; }

    [Column("cargo_status")]
    public CargoStatus CargoStatus { get; set; }

    [Column("cargo_id")]
    public Guid CargoId { get; set; }

    public virtual Cargo Cargo { get; set; } = null!;
}
