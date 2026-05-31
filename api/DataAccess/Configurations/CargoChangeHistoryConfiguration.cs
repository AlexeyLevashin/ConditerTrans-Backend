using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class CargoChangeHistoryConfiguration : IEntityTypeConfiguration<CargoChangeHistory>
{
    public void Configure(EntityTypeBuilder<CargoChangeHistory> builder)
    {
        builder.ToTable("cargo_change_histories");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ChangeTime).IsRequired();
        builder.Property(h => h.CargoStatus).IsRequired();
    }
}
