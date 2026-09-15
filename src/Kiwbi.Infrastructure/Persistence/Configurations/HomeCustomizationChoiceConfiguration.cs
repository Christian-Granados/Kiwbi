using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiwbi.Infrastructure.Persistence.Configurations;

public class HomeCustomizationChoiceConfiguration : IEntityTypeConfiguration<HomeCustomizationChoice>
{
    public void Configure(EntityTypeBuilder<HomeCustomizationChoice> builder)
    {
        builder.ToTable("home_customization_choices");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.HousingUnitId).HasColumnName("housing_unit_id").IsRequired();
        builder.Property(c => c.CustomizationId).HasColumnName("customization_id").IsRequired();
        builder.Property(c => c.SelectedOptionId).HasColumnName("selected_option_id");

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(c => c.SelectedAtUtc).HasColumnName("selected_at_utc");
        builder.Property(c => c.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(c => c.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne<HousingUnit>()
            .WithMany()
            .HasForeignKey(c => c.HousingUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Customization>()
            .WithMany()
            .HasForeignKey(c => c.CustomizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // selected_option_id intentionally has no DB-level FK: CustomizationOption is an OwnsMany table of the Customization aggregate.
        builder.HasIndex(c => new { c.HousingUnitId, c.CustomizationId }).IsUnique();
    }
}
