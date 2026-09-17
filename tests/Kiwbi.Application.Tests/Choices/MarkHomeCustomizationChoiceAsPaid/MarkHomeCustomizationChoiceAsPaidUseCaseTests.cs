using FluentAssertions;
using Kiwbi.Application.Choices.MarkHomeCustomizationChoiceAsPaid;
using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Choices.MarkHomeCustomizationChoiceAsPaid;

public class MarkHomeCustomizationChoiceAsPaidUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _housingPromotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _housingUnitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository = Substitute.For<IHomeCustomizationChoiceRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly MarkHomeCustomizationChoiceAsPaidUseCase _useCase;

    public MarkHomeCustomizationChoiceAsPaidUseCaseTests()
    {
        _useCase = new MarkHomeCustomizationChoiceAsPaidUseCase(
            _currentUser,
            _housingPromotionRepository,
            _housingUnitRepository,
            _homeCustomizationChoiceRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnitBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _housingUnitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _housingPromotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(new(unit.Id, Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenChoiceDoesNotExist_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        SetupOwnedUnit(promotion, developerCompanyId, unit);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((HomeCustomizationChoice?)null);

        var result = await _useCase.ExecuteAsync(new(unit.Id, Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenChoiceNotConfirmed_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var choice = HomeCustomizationChoice.Create(unit.Id, Guid.NewGuid());
        choice.SelectOption(Guid.NewGuid(), DateTime.UtcNow);
        SetupOwnedUnit(promotion, developerCompanyId, unit);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, choice.CustomizationId, Arg.Any<CancellationToken>())
            .Returns(choice);

        var result = await _useCase.ExecuteAsync(new(unit.Id, choice.CustomizationId));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenChoiceConfirmed_ShouldMarkAsPaid()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var choice = HomeCustomizationChoice.Create(unit.Id, Guid.NewGuid());
        choice.SelectOption(Guid.NewGuid(), DateTime.UtcNow);
        choice.Confirm(DateTime.UtcNow);
        SetupOwnedUnit(promotion, developerCompanyId, unit);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, choice.CustomizationId, Arg.Any<CancellationToken>())
            .Returns(choice);

        var result = await _useCase.ExecuteAsync(new(unit.Id, choice.CustomizationId));

        result.IsSuccess.Should().BeTrue();
        choice.Status.Should().Be(HomeCustomizationChoiceStatus.Paid);
        _homeCustomizationChoiceRepository.Received(1).Update(choice);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private void SetupOwnedUnit(HousingPromotion promotion, Guid developerCompanyId, HousingUnit unit)
    {
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _housingUnitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _housingPromotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
    }
}
