using Kiwbi.Domain.RealEstate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiwbi.Infrastructure.Persistence.Configurations;

public class HousingUnitConfiguration : IEntityTypeConfiguration<HousingUnit>
{
    public void Configure(EntityTypeBuilder<HousingUnit> builder)
    {
        builder.ToTable("housing_units");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.HousingPromotionId).HasColumnName("housing_promotion_id").IsRequired();
        builder.Property(u => u.HousingTypologyId).HasColumnName("housing_typology_id");

        builder.Property(u => u.Floor).HasColumnName("floor").HasMaxLength(50).IsRequired();
        builder.Property(u => u.Door).HasColumnName("door").HasMaxLength(50).IsRequired();
        builder.Property(u => u.BuiltAreaSqm).HasColumnName("built_area_sqm").HasPrecision(8, 2).IsRequired();
        builder.Property(u => u.UsableAreaSqm).HasColumnName("usable_area_sqm").HasPrecision(8, 2);
        builder.Property(u => u.FloorPlanImagePath).HasColumnName("floor_plan_image_path").HasMaxLength(500);

        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(u => u.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(u => u.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne<HousingPromotion>()
            .WithMany()
            .HasForeignKey(u => u.HousingPromotionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<HousingTypology>()
            .WithMany()
            .HasForeignKey(u => u.HousingTypologyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => new { u.HousingPromotionId, u.Floor, u.Door }).IsUnique();
    }
}
