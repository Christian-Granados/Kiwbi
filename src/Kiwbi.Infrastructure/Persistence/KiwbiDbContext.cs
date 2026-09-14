using Kiwbi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Persistence;

public class KiwbiDbContext : IdentityDbContext<ApplicationUser>
{
    public KiwbiDbContext(DbContextOptions<KiwbiDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(KiwbiDbContext).Assembly);
    }
}
