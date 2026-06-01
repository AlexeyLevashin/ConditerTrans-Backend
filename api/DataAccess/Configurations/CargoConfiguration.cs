using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class CargoConfiguration : IEntityTypeConfiguration<Cargo>
{
    public void Configure(EntityTypeBuilder<Cargo> builder)
    {
        builder.ToTable("cargos");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.LoadingDate).IsRequired();
        builder.Property(c => c.UnloadingDate).IsRequired();

        builder.Property(c => c.DeliveryAddress)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(c => c.Volume)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(c => c.Weight)
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(c => c.Dimensions)
            .HasMaxLength(64)
            .IsRequired(false);

        builder.Property(c => c.Status).IsRequired();

        builder.HasOne(c => c.LogisticCompany)
            .WithMany()
            .HasForeignKey(c => c.LogisticCompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Driver)
            .WithMany()
            .HasForeignKey(c => c.DriverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Histories)
            .WithOne(h => h.Cargo)
            .HasForeignKey(h => h.CargoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.LogisticCompanyId);
        builder.HasIndex(c => c.DriverId);
    }
}
