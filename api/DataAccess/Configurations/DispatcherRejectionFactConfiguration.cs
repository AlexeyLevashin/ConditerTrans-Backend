using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class DispatcherRejectionFactConfiguration : IEntityTypeConfiguration<DispatcherRejectionFact>
{
    public void Configure(EntityTypeBuilder<DispatcherRejectionFact> builder)
    {
        builder.ToTable("dispatcher_rejection_facts");
        builder.HasKey(f => f.OrderId);
        builder.Property(f => f.CompanyId).IsRequired();
        builder.Property(f => f.RejectedAt).IsRequired();
        builder.Property(f => f.RejectionReasonId).IsRequired();
        builder.HasOne(f => f.Order).WithOne().HasForeignKey<DispatcherRejectionFact>(f => f.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(f => f.Company).WithMany().HasForeignKey(f => f.CompanyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(f => f.RejectionReason).WithMany().HasForeignKey(f => f.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(f => new { f.CompanyId, f.RejectedAt });
    }
}
