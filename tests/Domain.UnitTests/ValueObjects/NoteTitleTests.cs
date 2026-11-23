using CoMind.Domain.Exceptions;
using CoMind.Domain.ValueObjects;

namespace CoMind.Domain.UnitTests.ValueObjects;

public class NoteTitleTests
{
    [Fact]
    public void Create_WithValidTitle_ReturnsTitle()
    {
        var title = NoteTitle.Create("My first note");

        title.Value.Should().Be("My first note");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var title = NoteTitle.Create("  My note  ");

        title.Value.Should().Be("My note");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_Throws(string? value)
    {
        var act = () => NoteTitle.Create(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithTitleOver200Chars_Throws()
    {
        var value = new string('a', 201);

        var act = () => NoteTitle.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage("*200*");
    }

    [Fact]
    public void Create_WithTitleExactly200Chars_Succeeds()
    {
        var value = new string('a', 200);

        var title = NoteTitle.Create(value);

        title.Value.Should().HaveLength(200);
    }
    
    [Fact]
    public void Create_WithWhitespaceAroundTitle_TrimsBeforeCheckingLength()
    {
        var value = $"  {new string('a', 200)}  ";

        var title = NoteTitle.Create(value);

        title.Value.Should().HaveLength(200);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var first = NoteTitle.Create("My note");
        var second = NoteTitle.Create(" My note ");

        first.Should().Be(second);
    }

    [Fact]
    public void Create_PreservesCase()
    {
        var title = NoteTitle.Create("My NOTE");

        title.Value.Should().Be("My NOTE");
    }
}
