using Kiwbi.Domain.Developers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiwbi.Infrastructure.Persistence.Configurations;

public class DeveloperCompanyConfiguration : IEntityTypeConfiguration<DeveloperCompany>
{
    public void Configure(EntityTypeBuilder<DeveloperCompany> builder)
    {
        builder.ToTable("developer_companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(c => c.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.OwnsOne(c => c.Branding, branding =>
        {
            branding.Property(b => b.LogoPath).HasColumnName("logo_path").HasMaxLength(500);

            branding.Property(b => b.PrimaryColor)
                .HasColumnName("primary_color")
                .HasMaxLength(7)
                .HasConversion(color => color.Value, value => BrandColor.Create(value))
                .IsRequired();

            branding.Property(b => b.SecondaryColor)
                .HasColumnName("secondary_color")
                .HasMaxLength(7)
                .HasConversion(
                    color => color == null ? null : color.Value,
                    value => value == null ? null : BrandColor.Create(value));
        });

        builder.Navigation(c => c.Branding).IsRequired();
    }
}
