using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("dispatcher_rejection_facts")]
public class DispatcherRejectionFact
{
    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Column("company_id")]
    public Guid CompanyId { get; set; }

    [Column("rejected_at")]
    public DateTime RejectedAt { get; set; }

    [Column("rejection_reason_id")]
    public Guid RejectionReasonId { get; set; }

    public Order Order { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public RejectionReason RejectionReason { get; set; } = null!;
}
