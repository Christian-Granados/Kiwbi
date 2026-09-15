using FluentAssertions;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.Onboarding;

namespace Kiwbi.Domain.Tests.Onboarding;

public class HousingUnitBuyerTests
{
    private static readonly Guid HousingUnitId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldInitializeProperties()
    {
        var link = HousingUnitBuyer.Create(HousingUnitId, "user-1");

        link.Id.Should().NotBeEmpty();
        link.HousingUnitId.Should().Be(HousingUnitId);
        link.BuyerUserId.Should().Be("user-1");
        link.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithEmptyHousingUnitId_ShouldThrowDomainException()
    {
        var act = () => HousingUnitBuyer.Create(Guid.Empty, "user-1");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidBuyerUserId_ShouldThrowDomainException(string? buyerUserId)
    {
        var act = () => HousingUnitBuyer.Create(HousingUnitId, buyerUserId!);

        act.Should().Throw<DomainException>();
    }
}
