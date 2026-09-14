using FluentAssertions;
using Kiwbi.Domain.Developers;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Tests.Developers;

public class BrandingTests
{
    [Fact]
    public void Create_WithValidColors_ShouldSucceed()
    {
        var branding = Branding.Create("#111111", "#222222", "logos/acme.png");

        branding.PrimaryColor.Value.Should().Be("#111111");
        branding.SecondaryColor!.Value.Should().Be("#222222");
        branding.LogoPath.Should().Be("logos/acme.png");
    }

    [Fact]
    public void Create_WithoutSecondaryColorOrLogo_ShouldAllowNulls()
    {
        var branding = Branding.Create("#111111");

        branding.SecondaryColor.Should().BeNull();
        branding.LogoPath.Should().BeNull();
    }

    [Fact]
    public void Create_WithInvalidPrimaryColor_ShouldThrowDomainException()
    {
        var act = () => Branding.Create("not-a-color");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateDefault_ShouldProduceConsistentBranding()
    {
        var branding = Branding.CreateDefault();

        branding.PrimaryColor.Value.Should().Be("#000000");
        branding.SecondaryColor.Should().BeNull();
    }
}
