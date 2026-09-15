using FluentAssertions;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Tests.Choices;

public class HomeCustomizationChoiceTests
{
    private static readonly Guid HousingUnitId = Guid.NewGuid();
    private static readonly Guid CustomizationId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldStartAsPendingWithoutSelectedOption()
    {
        var choice = HomeCustomizationChoice.Create(HousingUnitId, CustomizationId);

        choice.Id.Should().NotBeEmpty();
        choice.HousingUnitId.Should().Be(HousingUnitId);
        choice.CustomizationId.Should().Be(CustomizationId);
        choice.Status.Should().Be(HomeCustomizationChoiceStatus.Pending);
        choice.SelectedOptionId.Should().BeNull();
        choice.SelectedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyHousingUnitId_ShouldThrowDomainException()
    {
        var act = () => HomeCustomizationChoice.Create(Guid.Empty, CustomizationId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmptyCustomizationId_ShouldThrowDomainException()
    {
        var act = () => HomeCustomizationChoice.Create(HousingUnitId, Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void SelectOption_WithValidOption_ShouldUpdateSelectionAndStatus()
    {
        var choice = HomeCustomizationChoice.Create(HousingUnitId, CustomizationId);
        var optionId = Guid.NewGuid();
        var utcNow = DateTime.UtcNow;

        choice.SelectOption(optionId, utcNow);

        choice.SelectedOptionId.Should().Be(optionId);
        choice.Status.Should().Be(HomeCustomizationChoiceStatus.Selected);
        choice.SelectedAtUtc.Should().Be(utcNow);
    }

    [Fact]
    public void SelectOption_WithEmptyOptionId_ShouldThrowDomainException()
    {
        var choice = HomeCustomizationChoice.Create(HousingUnitId, CustomizationId);

        var act = () => choice.SelectOption(Guid.Empty, DateTime.UtcNow);

        act.Should().Throw<DomainException>();
    }
}
