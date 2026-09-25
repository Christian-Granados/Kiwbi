using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiwbi.Infrastructure.Persistence.Configurations;

public class CustomizationConfiguration : IEntityTypeConfiguration<Customization>
{
    public void Configure(EntityTypeBuilder<Customization> builder)
    {
        builder.ToTable("customizations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TradeCategoryId).HasColumnName("trade_category_id").IsRequired();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(c => c.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne<TradeCategory>()
            .WithMany()
            .HasForeignKey(c => c.TradeCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.TradeCategoryId, c.Name }).IsUnique();

        builder.Metadata.FindNavigation(nameof(Customization.Options))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(Customization.Assignments))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(c => c.Options, options =>
        {
            options.ToTable("customization_options");
            options.WithOwner().HasForeignKey("CustomizationId");
            options.Property<Guid>("CustomizationId").HasColumnName("customization_id");
            options.HasKey(o => o.Id);

            options.Property(o => o.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            options.Property(o => o.SurchargeAmount).HasColumnName("surcharge_amount").HasColumnType("decimal(10,2)").IsRequired();
            options.Property(o => o.IsDefault).HasColumnName("is_default").IsRequired();
            options.Property(o => o.ThumbnailImagePath).HasColumnName("thumbnail_image_path").HasMaxLength(500);

            options.HasIndex("CustomizationId", nameof(CustomizationOption.Name)).IsUnique();
        });

        builder.OwnsMany(c => c.Assignments, assignments =>
        {
            assignments.ToTable("customization_assignments");
            assignments.WithOwner().HasForeignKey("CustomizationId");
            assignments.Property<Guid>("CustomizationId").HasColumnName("customization_id");
            assignments.HasKey(a => a.Id);

            assignments.Property(a => a.Scope).HasColumnName("scope").HasConversion<string>().HasMaxLength(50).IsRequired();
            assignments.Property(a => a.HousingTypologyId).HasColumnName("housing_typology_id");
            assignments.Property(a => a.HousingUnitId).HasColumnName("housing_unit_id");

            assignments.HasOne<HousingTypology>()
                .WithMany()
                .HasForeignKey(a => a.HousingTypologyId)
                .OnDelete(DeleteBehavior.Restrict);

            assignments.HasOne<HousingUnit>()
                .WithMany()
                .HasForeignKey(a => a.HousingUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            assignments.HasIndex("CustomizationId", nameof(CustomizationAssignment.HousingTypologyId))
                .IsUnique()
                .HasFilter("housing_typology_id IS NOT NULL");

            assignments.HasIndex("CustomizationId", nameof(CustomizationAssignment.HousingUnitId))
                .IsUnique()
                .HasFilter("housing_unit_id IS NOT NULL");
        });
    }
}
