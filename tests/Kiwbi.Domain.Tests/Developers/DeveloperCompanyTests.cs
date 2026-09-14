using FluentAssertions;
using Kiwbi.Domain.Developers;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Tests.Developers;

public class DeveloperCompanyTests
{
    [Fact]
    public void Create_WithValidName_ShouldInitializeDefaults()
    {
        var company = DeveloperCompany.Create("Acme Promotora");

        company.Name.Should().Be("Acme Promotora");
        company.Branding.Should().Be(Branding.CreateDefault());
        company.Id.Should().NotBeEmpty();
        company.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        company.UpdatedAtUtc.Should().Be(company.CreatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowDomainException(string? name)
    {
        var act = () => DeveloperCompany.Create(name!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rename_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        var company = DeveloperCompany.Create("Acme Promotora");
        var originalUpdatedAt = company.UpdatedAtUtc;

        company.Rename("Nueva Promotora");

        company.Name.Should().Be("Nueva Promotora");
        company.UpdatedAtUtc.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void Rename_WithInvalidName_ShouldThrowDomainException()
    {
        var company = DeveloperCompany.Create("Acme Promotora");

        var act = () => company.Rename("   ");

        act.Should().Throw<DomainException>();
        company.Name.Should().Be("Acme Promotora");
    }

    [Fact]
    public void UpdateBranding_ShouldReplaceBrandingAndTouchTimestamp()
    {
        var company = DeveloperCompany.Create("Acme Promotora");
        var newBranding = Branding.Create("#ABCDEF");

        company.UpdateBranding(newBranding);

        company.Branding.Should().Be(newBranding);
    }
}
