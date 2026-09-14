using Kiwbi.Domain.Developers;
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(KiwbiDbContext).Assembly);
    }
}
