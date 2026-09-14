using FluentAssertions;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Domain.Tests.RealEstate;

public class HousingUnitTests
{
    private static readonly Guid HousingPromotionId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldInitializePropertiesAndDefaultStatus()
    {
        var unit = HousingUnit.Create(HousingPromotionId, "1", "A", 90.5m, 80m);

        unit.Id.Should().NotBeEmpty();
        unit.HousingPromotionId.Should().Be(HousingPromotionId);
        unit.HousingTypologyId.Should().BeNull();
        unit.Floor.Should().Be("1");
        unit.Door.Should().Be("A");
        unit.BuiltAreaSqm.Should().Be(90.5m);
        unit.UsableAreaSqm.Should().Be(80m);
        unit.FloorPlanImagePath.Should().BeNull();
        unit.Status.Should().Be(HousingUnitStatus.Available);
        unit.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        unit.UpdatedAtUtc.Should().Be(unit.CreatedAtUtc);
    }

    [Fact]
    public void Create_WithHousingTypologyId_ShouldAssignIt()
    {
        var typologyId = Guid.NewGuid();

        var unit = HousingUnit.Create(HousingPromotionId, "1", "A", 90.5m, 80m, typologyId);

        unit.HousingTypologyId.Should().Be(typologyId);
    }

    [Fact]
    public void Create_WithEmptyHousingPromotionId_ShouldThrowDomainException()
    {
        var act = () => HousingUnit.Create(Guid.Empty, "1", "A", 90.5m, 80m);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "A")]
    [InlineData("1", "")]
    [InlineData(null, "A")]
    public void Create_WithMissingFloorOrDoor_ShouldThrowDomainException(string? floor, string door)
    {
        var act = () => HousingUnit.Create(HousingPromotionId, floor!, door, 90.5m, 80m);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithNonPositiveBuiltArea_ShouldThrowDomainException(decimal builtArea)
    {
        var act = () => HousingUnit.Create(HousingPromotionId, "1", "A", builtArea, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithUsableAreaGreaterThanBuiltArea_ShouldThrowDomainException()
    {
        var act = () => HousingUnit.Create(HousingPromotionId, "1", "A", 80m, 90m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateFieldsAndTouchTimestamp()
    {
        var unit = HousingUnit.Create(HousingPromotionId, "1", "A", 90.5m, 80m);
        var originalUpdatedAt = unit.UpdatedAtUtc;

        unit.UpdateDetails("2", "B", 100m, 95m);

        unit.Floor.Should().Be("2");
        unit.Door.Should().Be("B");
        unit.BuiltAreaSqm.Should().Be(100m);
        unit.UsableAreaSqm.Should().Be(95m);
        unit.UpdatedAtUtc.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void AssignTypology_ShouldUpdateHousingTypologyId()
    {
        var unit = HousingUnit.Create(HousingPromotionId, "1", "A", 90.5m, 80m);
        var typologyId = Guid.NewGuid();

        unit.AssignTypology(typologyId);

        unit.HousingTypologyId.Should().Be(typologyId);
    }

    [Fact]
    public void AssignTypology_WithNull_ShouldClearHousingTypologyId()
    {
        var unit = HousingUnit.Create(HousingPromotionId, "1", "A", 90.5m, 80m, Guid.NewGuid());

        unit.AssignTypology(null);

        unit.HousingTypologyId.Should().BeNull();
    }

    [Fact]
    public void UpdateFloorPlanImage_WithPath_ShouldSetPath()
    {
        var unit = HousingUnit.Create(HousingPromotionId, "1", "A", 90.5m, 80m);

        unit.UpdateFloorPlanImage("uploads/units/plan.png");

        unit.FloorPlanImagePath.Should().Be("uploads/units/plan.png");
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatusAndTouchTimestamp()
    {
        var unit = HousingUnit.Create(HousingPromotionId, "1", "A", 90.5m, 80m);
        var originalUpdatedAt = unit.UpdatedAtUtc;

        unit.ChangeStatus(HousingUnitStatus.Reserved);

        unit.Status.Should().Be(HousingUnitStatus.Reserved);
        unit.UpdatedAtUtc.Should().BeOnOrAfter(originalUpdatedAt);
    }
}
