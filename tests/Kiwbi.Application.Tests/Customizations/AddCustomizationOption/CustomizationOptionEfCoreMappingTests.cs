using FluentAssertions;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Developers;
using Kiwbi.Domain.RealEstate;
using Kiwbi.Infrastructure.Persistence;
using Kiwbi.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Application.Tests.Customizations.AddCustomizationOption;

/// <summary>
/// Regression test for Epic 8b: exercises the real EF Core mapping (SQLite-backed KiwbiDbContext,
/// not a mocked repository) because the original bug only manifested in EF Core's change tracking
/// of the Customization.Options OwnsMany collection, which mocked-repository tests can never catch.
/// </summary>
public class CustomizationOptionEfCoreMappingTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<KiwbiDbContext> _contextOptions;

    public CustomizationOptionEfCoreMappingTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<KiwbiDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new KiwbiDbContext(_contextOptions);
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task AddingAnOptionToAnAlreadyPersistedCustomization_ShouldInsertIt_NotThrowConcurrencyException()
    {
        var developerCompany = DeveloperCompany.Create("Promotora de prueba");
        var promotion = HousingPromotion.Create(developerCompany.Id, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);

        await using (var seedContext = new KiwbiDbContext(_contextOptions))
        {
            seedContext.DeveloperCompanies.Add(developerCompany);
            seedContext.HousingPromotions.Add(promotion);
            seedContext.TradeCategories.Add(tradeCategory);
            seedContext.Customizations.Add(customization);
            await seedContext.SaveChangesAsync();
        }

        // A fresh DbContext simulates a new request/unit-of-work loading the aggregate from scratch.
        await using var requestContext = new KiwbiDbContext(_contextOptions);
        var repository = new CustomizationRepository(requestContext);
        var loaded = await repository.GetByIdAsync(customization.Id);

        loaded.Should().NotBeNull();
        loaded!.AddOption("Porcelánico Premium", 500m);
        repository.Update(loaded);

        var act = async () => await requestContext.SaveChangesAsync();

        await act.Should().NotThrowAsync();

        await using var verifyContext = new KiwbiDbContext(_contextOptions);
        var reloaded = await verifyContext.Customizations.FirstAsync(c => c.Id == customization.Id);
        reloaded.Options.Should().HaveCount(2);
    }
}
