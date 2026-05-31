using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");

        builder.HasKey(ol => ol.Id);

        builder.Property(ol => ol.QuantityOfUnits)
            .IsRequired();

        builder.Property(ol => ol.ProductId)
            .IsRequired();

        builder.Property(ol => ol.OrderId)
            .IsRequired();

        builder.HasOne(ol => ol.Product)
            .WithMany()
            .HasForeignKey(ol => ol.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}