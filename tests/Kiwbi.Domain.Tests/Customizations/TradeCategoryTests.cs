using FluentAssertions;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Tests.Customizations;

public class TradeCategoryTests
{
    private static readonly Guid HousingPromotionId = Guid.NewGuid();
    private static readonly DateTime CutOffDateUtc = DateTime.UtcNow.AddMonths(1);

    [Fact]
    public void Create_WithValidData_ShouldInitializeProperties()
    {
        var tradeCategory = TradeCategory.Create(HousingPromotionId, "Carpintería", CutOffDateUtc);

        tradeCategory.Id.Should().NotBeEmpty();
        tradeCategory.HousingPromotionId.Should().Be(HousingPromotionId);
        tradeCategory.Name.Should().Be("Carpintería");
        tradeCategory.SelectionCutOffDateUtc.Should().Be(CutOffDateUtc);
        tradeCategory.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tradeCategory.UpdatedAtUtc.Should().Be(tradeCategory.CreatedAtUtc);
    }

    [Fact]
    public void Create_WithEmptyHousingPromotionId_ShouldThrowDomainException()
    {
        var act = () => TradeCategory.Create(Guid.Empty, "Carpintería", CutOffDateUtc);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowDomainException(string? name)
    {
        var act = () => TradeCategory.Create(HousingPromotionId, name!, CutOffDateUtc);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithDefaultCutOffDate_ShouldThrowDomainException()
    {
        var act = () => TradeCategory.Create(HousingPromotionId, "Carpintería", default);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rename_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        var tradeCategory = TradeCategory.Create(HousingPromotionId, "Carpintería", CutOffDateUtc);
        var originalUpdatedAt = tradeCategory.UpdatedAtUtc;

        tradeCategory.Rename("Fontanería");

        tradeCategory.Name.Should().Be("Fontanería");
        tradeCategory.UpdatedAtUtc.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void Rename_WithInvalidName_ShouldThrowDomainExceptionAndKeepPreviousValue()
    {
        var tradeCategory = TradeCategory.Create(HousingPromotionId, "Carpintería", CutOffDateUtc);

        var act = () => tradeCategory.Rename("   ");

        act.Should().Throw<DomainException>();
        tradeCategory.Name.Should().Be("Carpintería");
    }

    [Fact]
    public void Reschedule_WithValidDate_ShouldUpdateCutOffDateAndTimestamp()
    {
        var tradeCategory = TradeCategory.Create(HousingPromotionId, "Carpintería", CutOffDateUtc);
        var newCutOffDate = CutOffDateUtc.AddDays(15);

        tradeCategory.Reschedule(newCutOffDate);

        tradeCategory.SelectionCutOffDateUtc.Should().Be(newCutOffDate);
    }

    [Fact]
    public void Reschedule_WithDefaultDate_ShouldThrowDomainExceptionAndKeepPreviousValue()
    {
        var tradeCategory = TradeCategory.Create(HousingPromotionId, "Carpintería", CutOffDateUtc);

        var act = () => tradeCategory.Reschedule(default);

        act.Should().Throw<DomainException>();
        tradeCategory.SelectionCutOffDateUtc.Should().Be(CutOffDateUtc);
    }

    [Fact]
    public void IsExpired_WhenUtcNowIsAfterCutOffDate_ShouldReturnTrue()
    {
        var tradeCategory = TradeCategory.Create(HousingPromotionId, "Carpintería", DateTime.UtcNow.AddDays(-1));

        tradeCategory.IsExpired(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenUtcNowIsBeforeCutOffDate_ShouldReturnFalse()
    {
        var tradeCategory = TradeCategory.Create(HousingPromotionId, "Carpintería", DateTime.UtcNow.AddDays(1));

        tradeCategory.IsExpired(DateTime.UtcNow).Should().BeFalse();
    }
}
