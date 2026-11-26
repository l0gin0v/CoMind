using CoMind.Domain.Exceptions;
using CoMind.Domain.ValueObjects;

namespace CoMind.Domain.UnitTests.ValueObjects;

public class CommentSnapshotTests
{
    private static readonly Guid NoteId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static CommentSnapshot Create(
        string content = "Test comment",
        Guid? noteId = null,
        string authorId = "user-1",
        bool isDeleted = false,
        DateTimeOffset? deletedAt = null,
        string? createdBy = "user-1",
        string? updatedBy = "user-1",
        string? deletedBy = null)
        => new(
            content,
            noteId ?? NoteId,
            authorId,
            isDeleted,
            DateTimeOffset.Parse("2026-01-01T10:00:00Z"),
            createdBy,
            DateTimeOffset.Parse("2026-01-02T10:00:00Z"),
            updatedBy,
            deletedAt,
            deletedBy);

    [Fact]
    public void Create_TrimsContent()
    {
        var snapshot = Create("  Test comment  ");

        snapshot.Content.Should().Be("Test comment");
    }

    [Fact]
    public void Create_Throws_WhenContentIsEmpty()
    {
        var act = () => Create(" ");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Throws_WhenContentExceedsMaximumLength()
    {
        var act = () => Create(new string('a', 2001));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_AllowsContentAtMaximumLength()
    {
        var snapshot = Create(new string('a', 2000));

        snapshot.Content.Should().HaveLength(2000);
    }

    [Fact]
    public void Create_Throws_WhenNoteIdIsEmpty()
    {
        var act = () => Create(noteId: Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_Throws_WhenAuthorIdIsInvalid(string? authorId)
    {
        var act = () => Create(authorId: authorId!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Throws_WhenDeletedStateDoesNotMatchDeletedAt()
    {
        var act = () => Create(
            isDeleted: true,
            deletedAt: null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_AllowsDeletedSnapshot()
    {
        var deletedAt = DateTimeOffset.Parse("2026-01-03T10:00:00Z");

        var snapshot = Create(
            isDeleted: true,
            deletedAt: deletedAt,
            deletedBy: "user-1");

        snapshot.IsDeleted.Should().BeTrue();
        snapshot.DeletedAt.Should().Be(deletedAt);
    }

    [Fact]
    public void Equals_ReturnsTrue_ForIdenticalSnapshots()
    {
        var first = Create();
        var second = Create();

        first.Equals(second).Should().BeTrue();
        (first == second).Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenContentDiffers()
    {
        Create("First").Equals(Create("Second")).Should().BeFalse();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenNullableFieldsDiffer()
    {
        var first = Create(createdBy: null);
        var second = Create(createdBy: "");

        first.Equals(second).Should().BeFalse();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDeletionStateDiffers()
    {
        var active = Create();

        var deleted = Create(
            isDeleted: true,
            deletedAt: DateTimeOffset.Parse("2026-01-03T10:00:00Z"));

        active.Equals(deleted).Should().BeFalse();
    }
}
