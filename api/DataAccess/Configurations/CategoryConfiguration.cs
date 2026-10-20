using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.ParentId).HasColumnName("parent_id");

        builder.Property(c => c.Path)
            .HasColumnType("ltree")
            .HasColumnName("path")
            .IsRequired();

        builder.HasIndex(c => c.Path).HasMethod("gist");

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}