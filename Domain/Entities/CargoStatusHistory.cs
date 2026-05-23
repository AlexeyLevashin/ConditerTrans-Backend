using Common.Enums;

namespace Domain.Entities;

public class CargoStatusHistory
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime ChangeTime { get; set; }
    public Guid CargoId { get; set; }
    public virtual Cargo Cargo { get; set; }
    public Guid CargoStatusId { get; set; }
    public virtual CargoStatus CargoStatus { get; set; }
}