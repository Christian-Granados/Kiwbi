using Kiwbi.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kiwbi.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.DeveloperCompanyId).HasColumnName("developer_company_id");

        builder.HasOne(u => u.DeveloperCompany)
            .WithMany()
            .HasForeignKey(u => u.DeveloperCompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
