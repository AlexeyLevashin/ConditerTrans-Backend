using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class VehicleBrandConfiguration : IEntityTypeConfiguration<VehicleBrand>
{
    public void Configure(EntityTypeBuilder<VehicleBrand> builder)
    {
        builder.ToTable("vehicle_brands");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnType("uuid").ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");
        builder.Property(b => b.Name).IsRequired().HasMaxLength(128);
        builder.HasIndex(b => b.Name).IsUnique();
    }
}
