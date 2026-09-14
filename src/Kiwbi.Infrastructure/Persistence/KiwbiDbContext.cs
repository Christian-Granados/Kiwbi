using Kiwbi.Domain.Developers;
using Kiwbi.Domain.RealEstate;
using Kiwbi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Persistence;

public class KiwbiDbContext : IdentityDbContext<ApplicationUser>
{
    public KiwbiDbContext(DbContextOptions<KiwbiDbContext> options) : base(options)
    {
    }

    public DbSet<DeveloperCompany> DeveloperCompanies => Set<DeveloperCompany>();
    public DbSet<HousingPromotion> HousingPromotions => Set<HousingPromotion>();
    public DbSet<HousingTypology> HousingTypologies => Set<HousingTypology>();
    public DbSet<HousingUnit> HousingUnits => Set<HousingUnit>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(KiwbiDbContext).Assembly);
    }
}
