using FluentAssertions;
using Kiwbi.Domain.Developers;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Tests.Developers;

public class BrandColorTests
{
    [Theory]
    [InlineData("#FFFFFF")]
    [InlineData("#000000")]
    [InlineData("#1a2b3c")]
    public void Create_WithValidHexColor_ShouldSucceed(string value)
    {
        var color = BrandColor.Create(value);

        color.Value.Should().Be(value.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("FFFFFF")]
    [InlineData("#FFF")]
    [InlineData("#GGGGGG")]
    [InlineData(null)]
    public void Create_WithInvalidValue_ShouldThrowDomainException(string? value)
    {
        var act = () => BrandColor.Create(value!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        var first = BrandColor.Create("#123ABC");
        var second = BrandColor.Create("#123abc");

        first.Should().Be(second);
    }
}
