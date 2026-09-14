using FluentAssertions;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Domain.Tests.RealEstate;

public class HousingTypologyTests
{
    private static readonly Guid HousingPromotionId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldInitializeProperties()
    {
        var typology = HousingTypology.Create(HousingPromotionId, "Ático Tipo A");

        typology.Id.Should().NotBeEmpty();
        typology.HousingPromotionId.Should().Be(HousingPromotionId);
        typology.Name.Should().Be("Ático Tipo A");
        typology.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        typology.UpdatedAtUtc.Should().Be(typology.CreatedAtUtc);
    }

    [Fact]
    public void Create_WithEmptyHousingPromotionId_ShouldThrowDomainException()
    {
        var act = () => HousingTypology.Create(Guid.Empty, "Ático Tipo A");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowDomainException(string? name)
    {
        var act = () => HousingTypology.Create(HousingPromotionId, name!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rename_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        var typology = HousingTypology.Create(HousingPromotionId, "Ático Tipo A");
        var originalUpdatedAt = typology.UpdatedAtUtc;

        typology.Rename("Bajo Tipo B");

        typology.Name.Should().Be("Bajo Tipo B");
        typology.UpdatedAtUtc.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void Rename_WithInvalidName_ShouldThrowDomainExceptionAndKeepPreviousValue()
    {
        var typology = HousingTypology.Create(HousingPromotionId, "Ático Tipo A");

        var act = () => typology.Rename("   ");

        act.Should().Throw<DomainException>();
        typology.Name.Should().Be("Ático Tipo A");
    }
}
