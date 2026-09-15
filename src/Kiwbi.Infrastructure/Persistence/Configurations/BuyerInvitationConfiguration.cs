using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiwbi.Infrastructure.Persistence.Configurations;

public class BuyerInvitationConfiguration : IEntityTypeConfiguration<BuyerInvitation>
{
    public void Configure(EntityTypeBuilder<BuyerInvitation> builder)
    {
        builder.ToTable("buyer_invitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.HousingUnitId).HasColumnName("housing_unit_id").IsRequired();
        builder.Property(i => i.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(i => i.Token).HasColumnName("token").HasMaxLength(100).IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(i => i.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(i => i.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.Property(i => i.ExpiresAtUtc).HasColumnName("expires_at_utc");
        builder.Property(i => i.AcceptedAtUtc).HasColumnName("accepted_at_utc");
        builder.Property(i => i.AcceptedByUserId).HasColumnName("accepted_by_user_id").HasMaxLength(450);

        builder.HasOne<HousingUnit>()
            .WithMany()
            .HasForeignKey(i => i.HousingUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.Token).IsUnique();
        builder.HasIndex(i => new { i.HousingUnitId, i.Email });
    }
}
