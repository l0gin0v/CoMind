using CoMind.Domain.Exceptions;
using CoMind.Domain.ValueObjects;

namespace CoMind.Domain.UnitTests.ValueObjects;

public class TagSnapshotTests
{
    private static TagSnapshot Create(
        string name = "Work",
        Color? color = null,
        bool isDeleted = false,
        DateTimeOffset? deletedAt = null,
        string? createdBy = "user-1",
        string? updatedBy = "user-1",
        string? deletedBy = null)
        => new(
            TagName.Create(name),
            color,
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
        var snapshot = Create(color: Color.Red);

        snapshot.Name.Should().Be(TagName.Create("Work"));
        snapshot.Color.Should().Be(Color.Red);
        snapshot.IsDeleted.Should().BeFalse();
        snapshot.CreatedBy.Should().Be("user-1");
        snapshot.UpdatedBy.Should().Be("user-1");
    }

    [Fact]
    public void Create_AllowsNullColor()
    {
        var snapshot = Create();

        snapshot.Color.Should().BeNull();
    }

    [Fact]
    public void Create_Throws_WhenNameIsNull()
    {
        var act = () => new TagSnapshot(
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
        var first = Create(color: Color.Red);
        var second = Create(color: Color.Red);

        first.Equals(second).Should().BeTrue();
        (first == second).Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenNameDiffers()
    {
        var first = Create("Work");
        var second = Create("Personal");

        first.Equals(second).Should().BeFalse();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenColorDiffers()
    {
        var first = Create(color: Color.Red);
        var second = Create(color: Color.Blue);

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
