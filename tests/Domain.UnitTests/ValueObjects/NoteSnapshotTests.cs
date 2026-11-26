using CoMind.Domain.Exceptions;
using CoMind.Domain.ValueObjects;

namespace CoMind.Domain.UnitTests.ValueObjects;

public class NoteSnapshotTests
{
    private static readonly Guid TagId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static NoteSnapshot Create(
        Guid? tagId = null,
        bool isDeleted = false,
        DateTimeOffset? deletedAt = null,
        string? createdBy = "user-1",
        string? updatedBy = "user-1",
        string? deletedBy = null)
        => new(
            NoteTitle.Create("Test note"),
            NoteContent.Create("Test content"),
            tagId,
            isDeleted,
            DateTimeOffset.Parse("2026-01-01T10:00:00Z"),
            createdBy,
            DateTimeOffset.Parse("2026-01-02T10:00:00Z"),
            updatedBy,
            deletedAt,
            deletedBy);

    [Fact]
    public void Create_SetsAllProperties()
    {
        var snapshot = Create(TagId);

        snapshot.Title.Should().Be(NoteTitle.Create("Test note"));
        snapshot.Content.Should().Be(NoteContent.Create("Test content"));
        snapshot.TagId.Should().Be(TagId);
        snapshot.IsDeleted.Should().BeFalse();
        snapshot.CreatedBy.Should().Be("user-1");
        snapshot.UpdatedBy.Should().Be("user-1");
    }

    [Fact]
    public void Create_AllowsNullTagId()
    {
        var snapshot = Create();

        snapshot.TagId.Should().BeNull();
    }

    [Fact]
    public void Create_Throws_WhenTitleIsNull()
    {
        var act = () => new NoteSnapshot(
            null!,
            NoteContent.Create("Test content"),
            null,
            false,
            DateTimeOffset.UtcNow,
            null,
            DateTimeOffset.UtcNow,
            null,
            null,
            null);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_Throws_WhenContentIsNull()
    {
        var act = () => new NoteSnapshot(
            NoteTitle.Create("Test note"),
            null!,
            null,
            false,
            DateTimeOffset.UtcNow,
            null,
            DateTimeOffset.UtcNow,
            null,
            null,
            null);

        act.Should().Throw<ArgumentNullException>();
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
        var first = Create(TagId);
        var second = Create(TagId);

        first.Equals(second).Should().BeTrue();
        (first == second).Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenTagIdDiffers()
    {
        var first = Create();
        var second = Create(TagId);

        first.Equals(second).Should().BeFalse();
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
