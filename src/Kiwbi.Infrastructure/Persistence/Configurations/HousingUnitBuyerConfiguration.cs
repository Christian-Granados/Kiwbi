using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using Kiwbi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiwbi.Infrastructure.Persistence.Configurations;

public class HousingUnitBuyerConfiguration : IEntityTypeConfiguration<HousingUnitBuyer>
{
    public void Configure(EntityTypeBuilder<HousingUnitBuyer> builder)
    {
        builder.ToTable("home_buyer_assignments");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.HousingUnitId).HasColumnName("housing_unit_id").IsRequired();
        builder.Property(b => b.BuyerUserId).HasColumnName("buyer_user_id").HasMaxLength(450).IsRequired();
        builder.Property(b => b.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasOne<HousingUnit>()
            .WithMany()
            .HasForeignKey(b => b.HousingUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(b => b.BuyerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.HousingUnitId, b.BuyerUserId }).IsUnique();
    }
}
