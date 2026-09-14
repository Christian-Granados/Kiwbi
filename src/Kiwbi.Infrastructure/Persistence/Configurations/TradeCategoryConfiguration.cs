using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiwbi.Infrastructure.Persistence.Configurations;

public class TradeCategoryConfiguration : IEntityTypeConfiguration<TradeCategory>
{
    public void Configure(EntityTypeBuilder<TradeCategory> builder)
    {
        builder.ToTable("trade_categories");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.HousingPromotionId).HasColumnName("housing_promotion_id").IsRequired();
        builder.Property(t => t.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(t => t.SelectionCutOffDateUtc).HasColumnName("selection_cut_off_date_utc").IsRequired();
        builder.Property(t => t.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(t => t.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasOne<HousingPromotion>()
            .WithMany()
            .HasForeignKey(t => t.HousingPromotionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.HousingPromotionId, t.Name }).IsUnique();
    }
}
