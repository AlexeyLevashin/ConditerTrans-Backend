using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class OrderChangeHistoryConfiguration : IEntityTypeConfiguration<OrderChangeHistory>
{
    public void Configure(EntityTypeBuilder<OrderChangeHistory> builder)
    {
        builder.ToTable("order_change_histories");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ChangeTime)
            .IsRequired();

        builder.Property(h => h.OrderStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(h => h.OrderId)
            .IsRequired();
    }
}