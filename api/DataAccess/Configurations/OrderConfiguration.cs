using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
   public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired();

        builder.Property(o => o.CreationDate)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired(); 

        builder.Property(o => o.ProductionAddress)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(o => o.DeliveryAddress)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(o => o.PaymentType)
            .HasMaxLength(64)
            .IsRequired(false);

        builder.Property(o => o.ProposedDeliveryDate)
            .IsRequired(false);

        builder.Property(o => o.RescheduleReason)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(o => o.ManagerId)
            .IsRequired();

        builder.Property(o => o.DispatcherId);

        builder.Property(o => o.CargoId);

        builder.HasOne(o => o.Cargo)
            .WithOne(c => c.Order)
            .HasForeignKey<Order>(o => o.CargoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.CargoId).IsUnique();

        builder.HasOne(o => o.Manager)
            .WithMany()
            .HasForeignKey(o => o.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Dispatcher)
            .WithMany()
            .HasForeignKey(o => o.DispatcherId)
            .OnDelete(DeleteBehavior.SetNull); 
        
        builder.HasMany(o => o.OrderLines)
            .WithOne(ol => ol.Order) 
            .HasForeignKey(ol => ol.OrderId)
            .OnDelete(DeleteBehavior.Cascade); 

        builder.HasMany(o => o.Histories)
            .WithOne(h => h.Order)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade); 
    }
}