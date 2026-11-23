
using CoMind.Domain.Entities;
using CoMind.Domain.Events;
using CoMind.Domain.Exceptions;

namespace CoMind.Domain.UnitTests.Entities;

public class CommentTests
{
    private static Note CreateNote()
        => Note.Create("user-1", "Заголовок", "Содержимое");

    private static Comment CreateComment(
        string content = "Привет",
        Note? note = null)
    {
        note ??= CreateNote();
        return Comment.Create(note, "user-1", content);
    }

    // ===== Create =====

    [Fact]
    public void Create_WithValidData_ReturnsComment()
    {
        var note = CreateNote();

        var comment = Comment.Create(note, "user-1", "Привет");

        comment.NoteId.Should().Be(note.Id);
        comment.AuthorId.Should().Be("user-1");
        comment.Content.Should().Be("Привет");
    }

    [Fact]
    public void Create_TrimsContent()
    {
        var comment = CreateComment("   текст   ");

        comment.Content.Should().Be("текст");
    }

    [Fact]
    public void Create_RaisesCommentCreatedEvent()
    {
        var note = CreateNote();

        var comment = Comment.Create(note, "user-1", "текст");

        comment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CommentCreatedEvent>()
            .Which.NoteId.Should().Be(note.Id);

        var evt = comment.DomainEvents
            .OfType<CommentCreatedEvent>()
            .Single();

        evt.AuthorId.Should().Be("user-1");
    }

    [Fact]
    public void Create_WithNullNote_Throws()
    {
        var act = () => Comment.Create(null!, "user-1", "текст");

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyAuthor_Throws(string? authorId)
    {
        var note = CreateNote();

        var act = () => Comment.Create(note, authorId!, "текст");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyContent_Throws(string? content)
    {
        var note = CreateNote();

        var act = () => Comment.Create(note, "user-1", content!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithContentOver2000Chars_Throws()
    {
        var note = CreateNote();
        var tooLong = new string('a', 2001);

        var act = () => Comment.Create(note, "user-1", tooLong);

        act.Should().Throw<DomainException>()
            .WithMessage("*2000*");
    }

    [Fact]
    public void Create_WithContentExactly2000Chars_Succeeds()
    {
        var note = CreateNote();
        var exact = new string('a', 2000);

        var comment = Comment.Create(note, "user-1", exact);

        comment.Content.Should().HaveLength(2000);
    }

    [Fact]
    public void Create_WithWhitespaceAroundContent_ValidatesTrimmedLength()
    {
        var note = CreateNote();
        var content = $" {new string('a', 2000)} ";

        var comment = Comment.Create(note, "user-1", content);

        comment.Content.Should().HaveLength(2000);
    }

    [Fact]
    public void Create_ForDeletedNote_Throws()
    {
        var note = CreateNote();
        note.Delete("user-1");

        var act = () => Comment.Create(note, "user-1", "текст");

        act.Should().Throw<DomainException>();
    }

    // ===== UpdateContent =====

    [Fact]
    public void UpdateContent_WithNewValue_UpdatesContent()
    {
        var note = CreateNote();
        var comment = CreateComment(note: note);

        comment.UpdateContent("новое", "user-1", note);

        comment.Content.Should().Be("новое");
    }

    [Fact]
    public void UpdateContent_TrimsNewValue()
    {
        var note = CreateNote();
        var comment = CreateComment("старое", note);

        comment.UpdateContent("   новое   ", "user-1", note);

        comment.Content.Should().Be("новое");
    }

    [Fact]
    public void UpdateContent_WithSameValue_Throws()
    {
        var note = CreateNote();
        var comment = CreateComment("текст", note);

        var act = () => comment.UpdateContent("текст", "user-1", note);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateContent_FromNotAuthor_Throws()
    {
        var note = CreateNote();
        var comment = CreateComment("текст1", note);

        var act = () => comment.UpdateContent("текст2", "user-2", note);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateContent_WithSameValueAfterTrim_Throws()
    {
        var note = CreateNote();
        var comment = CreateComment("текст", note);

        var act = () => comment.UpdateContent("текст ", "user-1", note);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateContent_WithEmpty_Throws(string? content)
    {
        var note = CreateNote();
        var comment = CreateComment(note: note);

        var act = () => comment.UpdateContent(content!, "user-1", note);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateContent_WithOver2000Chars_Throws()
    {
        var note = CreateNote();
        var comment = CreateComment("старое", note);

        var act = () => comment.UpdateContent(
            new string('a', 2001),
            "user-1",
            note);

        act.Should().Throw<DomainException>()
            .WithMessage("*2000*");
    }

    [Fact]
    public void UpdateContent_WithWhitespaceAroundContent_ValidatesTrimmedLength()
    {
        var note = CreateNote();
        var comment = CreateComment("старое", note);
        var content = $" {new string('a', 2000)} ";

        var act = () => comment.UpdateContent(content, "user-1", note);

        act.Should().NotThrow();
        comment.Content.Should().HaveLength(2000);
    }

    [Fact]
    public void UpdateContent_ForDeletedNote_Throws()
    {
        var note = CreateNote();
        var comment = CreateComment(note: note);

        note.Delete("user-1");

        var act = () => comment.UpdateContent("новое", "user-1", note);

        act.Should().Throw<DomainException>()
            .WithMessage("*удалённой заметки*");
    }

    [Fact]
    public void UpdateContent_WithNullNote_Throws()
    {
        var note = CreateNote();
        var comment = CreateComment(note: note);

        var act = () => comment.UpdateContent("новое", "user-1", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateContent_WithDifferentNote_Throws()
    {
        var note = CreateNote();
        var anotherNote = CreateNote();
        var comment = CreateComment(note: note);

        var act = () => comment.UpdateContent(
            "новое",
            "user-1",
            anotherNote);

        act.Should().Throw<DomainException>();
    }
}
