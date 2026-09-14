using FluentAssertions;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Domain.Tests.RealEstate;

public class HousingPromotionTests
{
    private static readonly Guid DeveloperCompanyId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldInitializeProperties()
    {
        var promotion = HousingPromotion.Create(DeveloperCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");

        promotion.Id.Should().NotBeEmpty();
        promotion.DeveloperCompanyId.Should().Be(DeveloperCompanyId);
        promotion.Name.Should().Be("Residencial Acacias");
        promotion.City.Should().Be("Madrid");
        promotion.Address.Should().Be("Calle Mayor 1");
        promotion.MasterPlanImagePath.Should().BeNull();
        promotion.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        promotion.UpdatedAtUtc.Should().Be(promotion.CreatedAtUtc);
    }

    [Fact]
    public void Create_WithEmptyDeveloperCompanyId_ShouldThrowDomainException()
    {
        var act = () => HousingPromotion.Create(Guid.Empty, "Residencial Acacias", "Madrid", "Calle Mayor 1");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Madrid", "Calle Mayor 1")]
    [InlineData("Residencial Acacias", "", "Calle Mayor 1")]
    [InlineData("Residencial Acacias", "Madrid", "")]
    [InlineData(null, "Madrid", "Calle Mayor 1")]
    public void Create_WithMissingRequiredField_ShouldThrowDomainException(string? name, string city, string address)
    {
        var act = () => HousingPromotion.Create(DeveloperCompanyId, name!, city, address);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateFieldsAndTouchTimestamp()
    {
        var promotion = HousingPromotion.Create(DeveloperCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var originalUpdatedAt = promotion.UpdatedAtUtc;

        promotion.UpdateDetails("Residencial Robles", "Barcelona", "Avenida Diagonal 20");

        promotion.Name.Should().Be("Residencial Robles");
        promotion.City.Should().Be("Barcelona");
        promotion.Address.Should().Be("Avenida Diagonal 20");
        promotion.UpdatedAtUtc.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateDetails_WithInvalidData_ShouldThrowDomainExceptionAndKeepPreviousValues()
    {
        var promotion = HousingPromotion.Create(DeveloperCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");

        var act = () => promotion.UpdateDetails("   ", "Barcelona", "Avenida Diagonal 20");

        act.Should().Throw<DomainException>();
        promotion.Name.Should().Be("Residencial Acacias");
    }

    [Fact]
    public void UpdateMasterPlanImage_WithPath_ShouldSetPath()
    {
        var promotion = HousingPromotion.Create(DeveloperCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");

        promotion.UpdateMasterPlanImage("uploads/promotions/plan.png");

        promotion.MasterPlanImagePath.Should().Be("uploads/promotions/plan.png");
    }

    [Fact]
    public void UpdateMasterPlanImage_WithNullOrWhitespace_ShouldClearPath()
    {
        var promotion = HousingPromotion.Create(DeveloperCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        promotion.UpdateMasterPlanImage("uploads/promotions/plan.png");

        promotion.UpdateMasterPlanImage("   ");

        promotion.MasterPlanImagePath.Should().BeNull();
    }
}
