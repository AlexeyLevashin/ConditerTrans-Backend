using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("user_invitations")]
public class UserInvitation
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("is_used")]
    public bool IsUsed { get; set; } = false;
    public virtual User User { get; set; }
}