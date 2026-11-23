using CoMind.Domain.Exceptions;
using CoMind.Domain.ValueObjects;

namespace CoMind.Domain.UnitTests.ValueObjects;

public class ColorTests
{
    [Theory]
    [InlineData("#E05C4D")]
    [InlineData("#D98B2B")]
    [InlineData("#4CAF50")]
    [InlineData("#26A69A")]
    [InlineData("#5C6BC0")]
    [InlineData("#AB47BC")]
    [InlineData("#78909C")]
    public void From_WithSupportedCode_ReturnsColor(string code)
    {
        var color = Color.From(code);

        color.Code.Should().Be(code);
    }

    [Fact]
    public void From_TrimsAndNormalizesCase()
    {
        var color = Color.From("  #e05c4d  ");

        color.Code.Should().Be("#E05C4D");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void From_WithEmptyCode_Throws(string? code)
    {
        var act = () => Color.From(code!);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("#FFFFFF")]
    [InlineData("#000000")]
    [InlineData("red")]
    [InlineData("#123")]
    public void From_WithUnsupportedCode_Throws(string code)
    {
        var act = () => Color.From(code);

        act.Should().Throw<UnsupportedColorException>();
    }

    [Fact]
    public void From_WithSameCode_ReturnsEqualColors()
    {
        var first = Color.From("#E05C4D");
        var second = Color.From("#e05c4d");

        first.Should().Be(second);
    }

    [Fact]
    public void PredefinedColors_ReturnExpectedCodes()
    {
        Color.Red.Code.Should().Be("#E05C4D");
        Color.Orange.Code.Should().Be("#D98B2B");
        Color.Green.Code.Should().Be("#4CAF50");
        Color.Teal.Code.Should().Be("#26A69A");
        Color.Blue.Code.Should().Be("#5C6BC0");
        Color.Purple.Code.Should().Be("#AB47BC");
        Color.Grey.Code.Should().Be("#78909C");
    }
}
