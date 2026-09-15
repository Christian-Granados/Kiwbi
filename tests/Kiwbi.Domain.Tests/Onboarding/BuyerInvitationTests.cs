using FluentAssertions;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.Onboarding;

namespace Kiwbi.Domain.Tests.Onboarding;

public class BuyerInvitationTests
{
    private static readonly Guid HousingUnitId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldInitializeProperties()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");

        invitation.Id.Should().NotBeEmpty();
        invitation.HousingUnitId.Should().Be(HousingUnitId);
        invitation.Email.Should().Be("buyer@example.com");
        invitation.Status.Should().Be(BuyerInvitationStatus.Pending);
        invitation.Token.Should().NotBeNullOrWhiteSpace();
        invitation.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        invitation.ExpiresAtUtc.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));
        invitation.AcceptedAtUtc.Should().BeNull();
        invitation.AcceptedByUserId.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyHousingUnitId_ShouldThrowDomainException()
    {
        var act = () => BuyerInvitation.Create(Guid.Empty, "buyer@example.com");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("buyer@")]
    public void Create_WithInvalidEmail_ShouldThrowDomainException(string? email)
    {
        var act = () => BuyerInvitation.Create(HousingUnitId, email!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Resend_WhenPending_ShouldRegenerateTokenAndExpiration()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");
        var originalToken = invitation.Token;
        var originalExpiration = invitation.ExpiresAtUtc;

        invitation.Resend();

        invitation.Token.Should().NotBe(originalToken);
        invitation.ExpiresAtUtc.Should().BeOnOrAfter(originalExpiration);
        invitation.Status.Should().Be(BuyerInvitationStatus.Pending);
    }

    [Fact]
    public void Resend_WhenCancelled_ShouldThrowDomainException()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");
        invitation.Cancel();

        var act = () => invitation.Resend();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Resend_WhenAccepted_ShouldThrowDomainException()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");
        invitation.MarkAccepted("user-1", DateTime.UtcNow);

        var act = () => invitation.Resend();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WhenPending_ShouldSetStatusToCancelled()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");

        invitation.Cancel();

        invitation.Status.Should().Be(BuyerInvitationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowDomainException()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");
        invitation.Cancel();

        var act = () => invitation.Cancel();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAccepted_WhenPendingAndNotExpired_ShouldSetStatusToAcceptedWithMetadata()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");
        var utcNow = DateTime.UtcNow;

        invitation.MarkAccepted("user-1", utcNow);

        invitation.Status.Should().Be(BuyerInvitationStatus.Accepted);
        invitation.AcceptedAtUtc.Should().Be(utcNow);
        invitation.AcceptedByUserId.Should().Be("user-1");
    }

    [Fact]
    public void MarkAccepted_WhenExpired_ShouldThrowDomainException()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");

        var act = () => invitation.MarkAccepted("user-1", invitation.ExpiresAtUtc.AddSeconds(1));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAccepted_WhenAlreadyAccepted_ShouldThrowDomainException()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");
        invitation.MarkAccepted("user-1", DateTime.UtcNow);

        var act = () => invitation.MarkAccepted("user-2", DateTime.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAccepted_WhenCancelled_ShouldThrowDomainException()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");
        invitation.Cancel();

        var act = () => invitation.MarkAccepted("user-1", DateTime.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IsExpired_WhenUtcNowIsAfterExpiration_ShouldReturnTrue()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");

        invitation.IsExpired(invitation.ExpiresAtUtc.AddSeconds(1)).Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenUtcNowIsBeforeExpiration_ShouldReturnFalse()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");

        invitation.IsExpired(invitation.ExpiresAtUtc.AddSeconds(-1)).Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenAccepted_ShouldReturnFalseEvenAfterExpiration()
    {
        var invitation = BuyerInvitation.Create(HousingUnitId, "buyer@example.com");
        invitation.MarkAccepted("user-1", DateTime.UtcNow);

        invitation.IsExpired(invitation.ExpiresAtUtc.AddDays(1)).Should().BeFalse();
    }
}
