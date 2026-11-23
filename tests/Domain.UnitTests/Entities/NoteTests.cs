using CoMind.Domain.Entities;
using CoMind.Domain.Events;
using CoMind.Domain.Exceptions;

namespace CoMind.Domain.UnitTests.Entities;

public class NoteTests
{
    [Fact]
    public void Create_WithValidData_ReturnsNote()
    {
        var note = Note.Create(
            "user-1",
            "My note",
            "# Hello");

        note.UserId.Should().Be("user-1");
        note.Title.Value.Should().Be("My note");
        note.Content.Markdown.Should().Be("# Hello");
        note.TagId.Should().BeNull();
        note.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyUserId_Throws(string? userId)
    {
        var act = () => Note.Create(userId!, "Title", "Content");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmptyContent_Succeeds()
    {
        var note = Note.Create("user-1", "Title", "");

        note.Content.Markdown.Should().BeEmpty();
    }
    
    [Fact]
    public void Create_GeneratesNonEmptyId()
    {
        var note = Note.Create("user-1", "Title", "");

        note.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_RaisesNoteCreatedEvent()
    {
        var note = Note.Create("user-1", "Title", "");

        note.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<NoteCreatedEvent>()
            .Which.NoteId.Should().Be(note.Id);

        var evt = note.DomainEvents
            .OfType<NoteCreatedEvent>()
            .Single();

        evt.UserId.Should().Be("user-1");
    }

    [Fact]
    public void Create_WithEmptyTitle_Throws()
    {
        var act = () => Note.Create("user-1", "", "");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateTitle_WithNewValue_UpdatesTitle()
    {
        var note = Note.Create("user-1", "Old title", "");

        note.UpdateTitle("New title");

        note.Title.Value.Should().Be("New title");
    }

    [Fact]
    public void UpdateTitle_WithSameValue_Throws()
    {
        var note = Note.Create("user-1", "Title", "");

        var act = () => note.UpdateTitle(" Title ");

        act.Should().Throw<DomainException>();
    }
    
    [Fact]
    public void UpdateTitle_WhenDeleted_Throws()
    {
        var note = Note.Create("user-1", "Title", "");
        note.Delete("user-1");

        var act = () => note.UpdateTitle("New title");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateContent_WithNewValue_UpdatesContent()
    {
        var note = Note.Create("user-1", "Title", "Old content");

        note.UpdateContent("# New content");

        note.Content.Markdown.Should().Be("# New content");
    }

    [Fact]
    public void UpdateContent_WithSameValue_Throws()
    {
        var note = Note.Create("user-1", "Title", "# Content");

        var act = () => note.UpdateContent("# Content");

        act.Should().Throw<DomainException>();
    }
    
    [Fact]
    public void UpdateContent_WhenDeleted_Throws()
    {
        var note = Note.Create("user-1", "Title", "");
        note.Delete("user-1");

        var act = () => note.UpdateContent("New content");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AssignTag_WithValidTag_AssignsTag()
    {
        var note = Note.Create("user-1", "Title", "");
        var tag = Tag.Create("user-1", "Work");

        note.AssignTag(tag);

        note.Tag.Should().BeSameAs(tag);
        note.TagId.Should().Be(tag.Id);
    }

    [Fact]
    public void AssignTag_WithAnotherUsersTag_Throws()
    {
        var note = Note.Create("user-1", "Title", "");
        var tag = Tag.Create("user-2", "Work");

        var act = () => note.AssignTag(tag);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AssignTag_WithNull_Throws()
    {
        var note = Note.Create("user-1", "Title", "");

        var act = () => note.AssignTag(null!);

        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void AssignTag_WithSameTag_Throws()
    {
        var note = Note.Create("user-1", "Title", "");
        var tag = Tag.Create("user-1", "Work");

        note.AssignTag(tag);

        var act = () => note.AssignTag(tag);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AssignTag_WithAnotherTag_ReplacesPreviousTag()
    {
        var note = Note.Create("user-1", "Title", "");
        var firstTag = Tag.Create("user-1", "Work");
        var secondTag = Tag.Create("user-1", "Personal");

        note.AssignTag(firstTag);
        note.AssignTag(secondTag);

        note.Tag.Should().BeSameAs(secondTag);
        note.TagId.Should().Be(secondTag.Id);
    }
    
    [Fact]
    public void AssignTag_WhenDeleted_Throws()
    {
        var note = Note.Create("user-1", "Title", "");
        var tag = Tag.Create("user-1", "Work");
        note.Delete("user-1");

        var act = () => note.AssignTag(tag);

        act.Should().Throw<DomainException>();
    }


    [Fact]
    public void RemoveTag_RemovesAssignedTag()
    {
        var note = Note.Create("user-1", "Title", "");
        var tag = Tag.Create("user-1", "Work");

        note.AssignTag(tag);
        note.RemoveTag();

        note.Tag.Should().BeNull();
        note.TagId.Should().BeNull();
    }

    [Fact]
    public void RemoveTag_WithoutAssignedTag_Throws()
    {
        var note = Note.Create("user-1", "Title", "");

        var act = note.RemoveTag;

        act.Should().Throw<DomainException>();
    }
    
    [Fact]
    public void RemoveTag_WhenDeleted_Throws()
    {
        var note = Note.Create("user-1", "Title", "");
        var tag = Tag.Create("user-1", "Work");
        note.AssignTag(tag);
        note.Delete("user-1");

        var act = note.RemoveTag;

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Delete_SetsDeletedAtAndDeletedBy()
    {
        var note = Note.Create("user-1", "Title", "");

        note.Delete("user-1");

        note.IsDeleted.Should().BeTrue();
        note.DeletedAt.Should().NotBeNull();
        note.DeletedBy.Should().Be("user-1");
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_Throws()
    {
        var note = Note.Create("user-1", "Title", "");
        note.Delete("user-1");

        var act = () => note.Delete("user-1");

        act.Should().Throw<DomainException>();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Delete_WithEmptyDeletedBy_Throws(string? deletedBy)
    {
        var note = Note.Create("user-1", "Title", "");

        var act = () => note.Delete(deletedBy!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Restore_ClearsDeletionData()
    {
        var note = Note.Create("user-1", "Title", "");
        note.Delete("user-1");

        note.Restore();

        note.IsDeleted.Should().BeFalse();
        note.DeletedAt.Should().BeNull();
        note.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Restore_WhenNotDeleted_Throws()
    {
        var note = Note.Create("user-1", "Title", "");

        var act = note.Restore;

        act.Should().Throw<DomainException>();
    }
    
    [Fact]
    public void Restore_AllowsEditingAgain()
    {
        var note = Note.Create("user-1", "Title", "");

        note.Delete("user-1");
        note.Restore();

        note.UpdateTitle("New title");

        note.Title.Value.Should().Be("New title");
    }
}
