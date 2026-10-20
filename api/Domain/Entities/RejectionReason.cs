using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("rejection_reasons")]
public class RejectionReason
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("company_id")]
    public Guid? CompanyId { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Company? Company { get; set; }
    public List<OrderChangeHistory> OrderChangeHistories { get; set; } = [];
}
