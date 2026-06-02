using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class TransportVehicleConfiguration : IEntityTypeConfiguration<TransportVehicle>
{
    public void Configure(EntityTypeBuilder<TransportVehicle> builder)
    {
        builder.ToTable("transport_vehicles");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnType("uuid").ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");
        builder.Property(v => v.RegistrationNumber).IsRequired().HasMaxLength(32);
        builder.HasIndex(v => new { v.CompanyId, v.RegistrationNumber }).IsUnique();
        builder.Property(v => v.Capacity).HasPrecision(10, 2);
        builder.HasOne(v => v.Employee)
            .WithMany()
            .HasForeignKey(v => v.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(v => v.Model)
            .WithMany(m => m.TransportVehicles)
            .HasForeignKey(v => v.ModelId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(v => v.Company)
            .WithMany()
            .HasForeignKey(v => v.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
