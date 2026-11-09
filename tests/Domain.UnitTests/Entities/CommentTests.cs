using CoMind.Domain.Entities;
using CoMind.Domain.Events;
using CoMind.Domain.Exceptions;

namespace CoMind.Domain.UnitTests.Entities;

public class CommentTests
{
    // ===== Create =====

    [Fact]
    public void Create_WithValidData_ReturnsComment()
    {
        var comment = Comment.Create(noteId: 1, authorId: "user-1", content: "Привет");

        comment.NoteId.Should().Be(1);
        comment.AuthorId.Should().Be("user-1");
        comment.Content.Should().Be("Привет");
    }

    [Fact]
    public void Create_TrimsContent()
    {
        var comment = Comment.Create(1, "user-1", "   текст   ");

        comment.Content.Should().Be("текст");
    }

    [Fact]
    public void Create_RaisesCommentCreatedEvent()
    {
        var comment = Comment.Create(1, "user-1", "текст");

        comment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CommentCreatedEvent>()
            .Which.NoteId.Should().Be(1);

        var evt = comment.DomainEvents.OfType<CommentCreatedEvent>().Single();
        evt.AuthorId.Should().Be("user-1");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidNoteId_Throws(int noteId)
    {
        var act = () => Comment.Create(noteId, "user-1", "текст");

        act.Should().Throw<DomainException>()
            .WithMessage("*NoteId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyAuthor_Throws(string? authorId)
    {
        var act = () => Comment.Create(1, authorId!, "текст");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyContent_Throws(string? content)
    {
        var act = () => Comment.Create(1, "user-1", content!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithContentOver2000Chars_Throws()
    {
        var tooLong = new string('a', 2001);

        var act = () => Comment.Create(1, "user-1", tooLong);

        act.Should().Throw<DomainException>()
            .WithMessage("*2000*");
    }

    [Fact]
    public void Create_WithContentExactly2000Chars_Succeeds()
    {
        var exact = new string('a', 2000);

        var comment = Comment.Create(1, "user-1", exact);

        comment.Content.Should().HaveLength(2000);
    }

    // ===== UpdateContent =====

    [Fact]
    public void UpdateContent_WithNewValue_UpdatesContent()
    {
        var comment = Comment.Create(1, "user-1", "старое");

        comment.UpdateContent("новое");

        comment.Content.Should().Be("новое");
    }

    [Fact]
    public void UpdateContent_TrimsNewValue()
    {
        var comment = Comment.Create(1, "user-1", "старое");

        comment.UpdateContent("   новое   ");

        comment.Content.Should().Be("новое");
    }

    [Fact]
    public void UpdateContent_WithSameValue_Throws()
    {
        var comment = Comment.Create(1, "user-1", "текст");

        var act = () => comment.UpdateContent("текст");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateContent_WithSameValueAfterTrim_Throws()
    {
        var comment = Comment.Create(1, "user-1", "текст");

        var act = () => comment.UpdateContent("текст ");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateContent_WithEmpty_Throws(string? content)
    {
        var comment = Comment.Create(1, "user-1", "старое");

        var act = () => comment.UpdateContent(content!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateContent_WithOver2000Chars_Throws()
    {
        var comment = Comment.Create(1, "user-1", "старое");

        var act = () => comment.UpdateContent(new string('a', 2001));

        act.Should().Throw<DomainException>()
            .WithMessage("*2000*");
    }
}
