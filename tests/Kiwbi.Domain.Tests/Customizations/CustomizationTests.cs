using FluentAssertions;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Tests.Customizations;

public class CustomizationTests
{
    private static readonly Guid TradeCategoryId = Guid.NewGuid();

    [Fact]
    public void CreateForWholePromotion_WithValidData_ShouldInitializeAggregate()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);

        customization.Id.Should().NotBeEmpty();
        customization.TradeCategoryId.Should().Be(TradeCategoryId);
        customization.Name.Should().Be("Suelo");
        customization.Options.Should().ContainSingle(o => o.Name == "Parquet Roble" && o.SurchargeAmount == 0m && o.IsDefault);
        customization.Assignments.Should().ContainSingle(a => a.Scope == CustomizationScope.WholePromotion);
        customization.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customization.UpdatedAtUtc.Should().Be(customization.CreatedAtUtc);
    }

    [Fact]
    public void CreateForTypologies_WithMultipleIds_ShouldCreateOneAssignmentPerTypology()
    {
        var typologyIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var customization = Customization.CreateForTypologies(TradeCategoryId, "Suelo", "Parquet Roble", 0m, typologyIds);

        customization.Assignments.Should().HaveCount(2);
        customization.Assignments.Should().OnlyContain(a => a.Scope == CustomizationScope.Typology);
        customization.Assignments.Select(a => a.HousingTypologyId).Should().BeEquivalentTo(typologyIds);
    }

    [Fact]
    public void CreateForUnits_WithMultipleIds_ShouldCreateOneAssignmentPerUnit()
    {
        var unitIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var customization = Customization.CreateForUnits(TradeCategoryId, "Suelo", "Parquet Roble", 0m, unitIds);

        customization.Assignments.Should().HaveCount(2);
        customization.Assignments.Should().OnlyContain(a => a.Scope == CustomizationScope.Unit);
        customization.Assignments.Select(a => a.HousingUnitId).Should().BeEquivalentTo(unitIds);
    }

    [Fact]
    public void CreateForTypologies_WithEmptyList_ShouldThrowDomainException()
    {
        var act = () => Customization.CreateForTypologies(TradeCategoryId, "Suelo", "Parquet Roble", 0m, Array.Empty<Guid>());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateForUnits_WithEmptyList_ShouldThrowDomainException()
    {
        var act = () => Customization.CreateForUnits(TradeCategoryId, "Suelo", "Parquet Roble", 0m, Array.Empty<Guid>());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateForTypologies_WithDuplicateIds_ShouldDeduplicate()
    {
        var typologyId = Guid.NewGuid();

        var customization = Customization.CreateForTypologies(TradeCategoryId, "Suelo", "Parquet Roble", 0m, new[] { typologyId, typologyId });

        customization.Assignments.Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateForWholePromotion_WithInvalidName_ShouldThrowDomainException(string? name)
    {
        var act = () => Customization.CreateForWholePromotion(TradeCategoryId, name!, "Parquet Roble", 0m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateForWholePromotion_WithEmptyTradeCategoryId_ShouldThrowDomainException()
    {
        var act = () => Customization.CreateForWholePromotion(Guid.Empty, "Suelo", "Parquet Roble", 0m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateForWholePromotion_WithNegativeSurcharge_ShouldThrowDomainException()
    {
        var act = () => Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", -1m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rename_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);

        customization.Rename("Suelo Cocina");

        customization.Name.Should().Be("Suelo Cocina");
    }

    [Fact]
    public void AddOption_WithValidData_ShouldAddNonDefaultOption()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);

        customization.AddOption("Porcelánico Premium", 500m);

        customization.Options.Should().HaveCount(2);
        customization.Options.Should().ContainSingle(o => o.Name == "Porcelánico Premium" && o.SurchargeAmount == 500m && !o.IsDefault);
    }

    [Fact]
    public void AddOption_WithDuplicateName_ShouldThrowDomainException()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);

        var act = () => customization.AddOption("Parquet Roble", 100m);

        act.Should().Throw<DomainException>();
        customization.Options.Should().ContainSingle();
    }

    [Fact]
    public void AddOption_WithNegativeSurcharge_ShouldThrowDomainException()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);

        var act = () => customization.AddOption("Porcelánico Premium", -1m);

        act.Should().Throw<DomainException>();
        customization.Options.Should().ContainSingle();
    }

    [Fact]
    public void UpdateOption_WithValidData_ShouldUpdateNameAndSurcharge()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);
        var optionId = customization.Options.Single().Id;

        customization.UpdateOption(optionId, "Parquet Roble Natural", 50m);

        var option = customization.Options.Single();
        option.Name.Should().Be("Parquet Roble Natural");
        option.SurchargeAmount.Should().Be(50m);
    }

    [Fact]
    public void SetDefaultOption_ShouldUnmarkPreviousDefaultAndMarkNewOne()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);
        customization.AddOption("Porcelánico Premium", 500m);
        var newDefaultId = customization.Options.Single(o => o.Name == "Porcelánico Premium").Id;

        customization.SetDefaultOption(newDefaultId);

        customization.Options.Should().ContainSingle(o => o.IsDefault && o.Id == newDefaultId);
        customization.Options.Where(o => o.Id != newDefaultId).Should().OnlyContain(o => !o.IsDefault);
    }

    [Fact]
    public void RemoveOption_WhenOnlyOptionRemains_ShouldThrowDomainException()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);
        var optionId = customization.Options.Single().Id;

        var act = () => customization.RemoveOption(optionId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveOption_WhenOptionIsDefault_ShouldThrowDomainException()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);
        customization.AddOption("Porcelánico Premium", 500m);
        var defaultOptionId = customization.Options.Single(o => o.IsDefault).Id;

        var act = () => customization.RemoveOption(defaultOptionId);

        act.Should().Throw<DomainException>();
        customization.Options.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveOption_WhenNonDefaultOptionAndMoreThanOneExists_ShouldRemoveIt()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);
        customization.AddOption("Porcelánico Premium", 500m);
        var nonDefaultOptionId = customization.Options.Single(o => !o.IsDefault).Id;

        customization.RemoveOption(nonDefaultOptionId);

        customization.Options.Should().ContainSingle();
    }

    [Fact]
    public void AssignToTypology_WhenAlreadyWholePromotion_ShouldThrowDomainException()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);

        var act = () => customization.AssignToTypology(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AssignToUnit_WhenAlreadyWholePromotion_ShouldThrowDomainException()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);

        var act = () => customization.AssignToUnit(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AssignToTypology_WithNewTypology_ShouldAddAssignment()
    {
        var typologyId = Guid.NewGuid();
        var customization = Customization.CreateForTypologies(TradeCategoryId, "Suelo", "Parquet Roble", 0m, new[] { Guid.NewGuid() });

        customization.AssignToTypology(typologyId);

        customization.Assignments.Should().HaveCount(2);
    }

    [Fact]
    public void AssignToTypology_WithAlreadyAssignedTypology_ShouldThrowDomainException()
    {
        var typologyId = Guid.NewGuid();
        var customization = Customization.CreateForTypologies(TradeCategoryId, "Suelo", "Parquet Roble", 0m, new[] { typologyId });

        var act = () => customization.AssignToTypology(typologyId);

        act.Should().Throw<DomainException>();
        customization.Assignments.Should().ContainSingle();
    }

    [Fact]
    public void AssignToUnit_WithAlreadyAssignedUnit_ShouldThrowDomainException()
    {
        var unitId = Guid.NewGuid();
        var customization = Customization.CreateForUnits(TradeCategoryId, "Suelo", "Parquet Roble", 0m, new[] { unitId });

        var act = () => customization.AssignToUnit(unitId);

        act.Should().Throw<DomainException>();
        customization.Assignments.Should().ContainSingle();
    }

    [Fact]
    public void RemoveAssignment_WhenOnlyAssignmentRemains_ShouldThrowDomainException()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);
        var assignmentId = customization.Assignments.Single().Id;

        var act = () => customization.RemoveAssignment(assignmentId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveAssignment_WhenMoreThanOneExists_ShouldRemoveIt()
    {
        var typologyIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var customization = Customization.CreateForTypologies(TradeCategoryId, "Suelo", "Parquet Roble", 0m, typologyIds);
        var assignmentToRemove = customization.Assignments.First();

        customization.RemoveAssignment(assignmentToRemove.Id);

        customization.Assignments.Should().ContainSingle();
        customization.Assignments.Should().NotContain(a => a.Id == assignmentToRemove.Id);
    }

    [Fact]
    public void AppliesToHousingUnit_WithWholePromotionScope_ShouldAlwaysApply()
    {
        var customization = Customization.CreateForWholePromotion(TradeCategoryId, "Suelo", "Parquet Roble", 0m);

        customization.AppliesToHousingUnit(Guid.NewGuid(), null).Should().BeTrue();
    }

    [Fact]
    public void AppliesToHousingUnit_WithTypologyScope_ShouldOnlyApplyToMatchingTypology()
    {
        var typologyId = Guid.NewGuid();
        var customization = Customization.CreateForTypologies(TradeCategoryId, "Suelo", "Parquet Roble", 0m, new[] { typologyId });

        customization.AppliesToHousingUnit(Guid.NewGuid(), typologyId).Should().BeTrue();
        customization.AppliesToHousingUnit(Guid.NewGuid(), Guid.NewGuid()).Should().BeFalse();
        customization.AppliesToHousingUnit(Guid.NewGuid(), null).Should().BeFalse();
    }

    [Fact]
    public void AppliesToHousingUnit_WithUnitScope_ShouldOnlyApplyToThatUnit()
    {
        var unitId = Guid.NewGuid();
        var customization = Customization.CreateForUnits(TradeCategoryId, "Suelo", "Parquet Roble", 0m, new[] { unitId });

        customization.AppliesToHousingUnit(unitId, null).Should().BeTrue();
        customization.AppliesToHousingUnit(Guid.NewGuid(), null).Should().BeFalse();
    }
}

