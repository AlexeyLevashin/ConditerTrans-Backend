using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class RejectionReasonConfiguration : IEntityTypeConfiguration<RejectionReason>
{
    public static readonly Guid InsufficientResourcesId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid MissingRawMaterialsId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid CapacityOverloadedId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid DeadlineUnavailableId = Guid.Parse("10000000-0000-0000-0000-000000000004");

    public void Configure(EntityTypeBuilder<RejectionReason> builder)
    {
        builder.ToTable("rejection_reasons", table => table.HasCheckConstraint(
            "ck_rejection_reasons_scope",
            "(is_default = TRUE AND company_id IS NULL) OR (is_default = FALSE AND company_id IS NOT NULL)"));

        builder.HasKey(reason => reason.Id);

        builder.Property(reason => reason.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(reason => reason.IsDefault).IsRequired();
        builder.Property(reason => reason.IsActive).IsRequired();
        builder.Property(reason => reason.CreatedAt).IsRequired();

        builder.HasOne(reason => reason.Company)
            .WithMany(company => company.RejectionReasons)
            .HasForeignKey(reason => reason.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        var createdAt = new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            Default(InsufficientResourcesId, "Нехватка ресурсов", createdAt),
            Default(MissingRawMaterialsId, "Отсутствует необходимое сырьё", createdAt),
            Default(CapacityOverloadedId, "Производственные мощности загружены", createdAt),
            Default(DeadlineUnavailableId, "Невозможно выполнить заказ в указанный срок", createdAt));
    }

    private static RejectionReason Default(Guid id, string name, DateTime createdAt) => new()
    {
        Id = id,
        CompanyId = null,
        Name = name,
        IsDefault = true,
        IsActive = true,
        CreatedAt = createdAt
    };
}
