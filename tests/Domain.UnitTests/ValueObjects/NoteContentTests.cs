using CoMind.Domain.Exceptions;
using CoMind.Domain.ValueObjects;

namespace CoMind.Domain.UnitTests.ValueObjects;

public class NoteContentTests
{
    [Fact]
    public void Create_WithValidMarkdown_ReturnsContent()
    {
        const string markdown = "# Hello\n\n**World**";

        var content = NoteContent.Create(markdown);

        content.Markdown.Should().Be(markdown);
    }

    [Fact]
    public void Create_WithEmptyContent_Succeeds()
    {
        var content = NoteContent.Create("");

        content.Markdown.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithNull_Throws()
    {
        var act = () => NoteContent.Create(null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_NormalizesWindowsLineEndings()
    {
        var content = NoteContent.Create("Line 1\r\nLine 2");

        content.Markdown.Should().Be("Line 1\nLine 2");
    }

    [Fact]
    public void Create_NormalizesMacLineEndings()
    {
        var content = NoteContent.Create("Line 1\rLine 2");

        content.Markdown.Should().Be("Line 1\nLine 2");
    }

    [Fact]
    public void Create_PreservesMarkdownFormatting()
    {
        const string markdown = """
                                # Heading

                                - Item 1
                                - Item 2

                                **Bold text**
                                """;

        var content = NoteContent.Create(markdown);

        content.Markdown.Should().Be(markdown);
    }

    [Fact]
    public void Create_PreservesWhitespace()
    {
        var content = NoteContent.Create("  text  \n\n");

        content.Markdown.Should().Be("  text  \n\n");
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var first = NoteContent.Create("# Hello");
        var second = NoteContent.Create("# Hello");

        first.Should().Be(second);
    }
}
