using CoMind.Domain.ValueObjects;
using CoMind.Domain.Exceptions;

namespace CoMind.Domain.UnitTests.ValueObjects;

public class TagNameTests
{
    [Fact]
    public void Create_TrimsAndLowercases()
    {
        var tagName = TagName.Create("  CSharp  ");

        tagName.Value.Should().Be("csharp");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_Throws(string? name)
    {
        var act = () => TagName.Create(name!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithOver50Chars_Throws()
    {
        var act = () => TagName.Create(new string('a', 51));

        act.Should().Throw<DomainException>().WithMessage("*50*");
    }

    [Fact]
    public void Equals_ReturnsTrueForSameValue()
    {
        var name1 = TagName.Create("csharp");
        var name2 = TagName.Create("CSharp ");

        name1.Should().Be(name2);
    }
}
