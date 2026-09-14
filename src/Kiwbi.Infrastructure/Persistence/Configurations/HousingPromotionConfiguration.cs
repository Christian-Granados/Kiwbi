using Kiwbi.Domain.Developers;
using Kiwbi.Domain.RealEstate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiwbi.Infrastructure.Persistence.Configurations;

public class HousingPromotionConfiguration : IEntityTypeConfiguration<HousingPromotion>
{
    public void Configure(EntityTypeBuilder<HousingPromotion> builder)
    {
        builder.ToTable("housing_promotions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.DeveloperCompanyId).HasColumnName("developer_company_id").IsRequired();

        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.City).HasColumnName("city").HasMaxLength(150).IsRequired();
        builder.Property(p => p.Address).HasColumnName("address").HasMaxLength(300).IsRequired();
        builder.Property(p => p.MasterPlanImagePath).HasColumnName("master_plan_image_path").HasMaxLength(500);

        builder.Property(p => p.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(p => p.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne<DeveloperCompany>()
            .WithMany()
            .HasForeignKey(p => p.DeveloperCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.DeveloperCompanyId);
    }
}
