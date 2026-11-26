using CoMind.Domain.Entities;
using CoMind.Domain.Enums;
using CoMind.Domain.Exceptions;
using CoMind.Domain.ValueObjects;

namespace CoMind.Domain.UnitTests.Entities;

public class SyncConflictTests
{
    private static readonly Guid EntityId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid NoteId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private const string OwnerId = "owner-1";
    private const string OtherUserId = "user-2";

    private static readonly SyncVersion BaseVersion = SyncVersion.From(3);
    private static readonly SyncVersion ServerVersion = SyncVersion.From(4);

    private static NoteSnapshot CreateNoteSnapshot(string title = "Test note")
        => new(
            NoteTitle.Create(title),
            NoteContent.Create("Test content"),
            null,
            false,
            DateTimeOffset.UtcNow,
            OwnerId,
            DateTimeOffset.UtcNow,
            OwnerId,
            null,
            null);

    private static TagSnapshot CreateTagSnapshot(string name = "Work")
        => new(
            TagName.Create(name),
            Color.Red,
            false,
            DateTimeOffset.UtcNow,
            OwnerId,
            DateTimeOffset.UtcNow,
            OwnerId,
            null,
            null);

    private static CommentSnapshot CreateCommentSnapshot(
        string content = "Test comment",
        Guid? noteId = null,
        string authorId = OwnerId)
        => new(
            content,
            noteId ?? NoteId,
            authorId,
            false,
            DateTimeOffset.UtcNow,
            authorId,
            DateTimeOffset.UtcNow,
            authorId,
            null,
            null);

    private static SyncConflict CreateNoteConflict()
        => SyncConflict.ForNote(
            OwnerId,
            EntityId,
            BaseVersion,
            ServerVersion,
            CreateNoteSnapshot("Local"),
            CreateNoteSnapshot("Server"));

    private static SyncConflict CreateTagConflict()
        => SyncConflict.ForTag(
            OwnerId,
            EntityId,
            BaseVersion,
            ServerVersion,
            CreateTagSnapshot("Local"),
            CreateTagSnapshot("Server"));

    private static SyncConflict CreateCommentConflict()
        => SyncConflict.ForComment(
            OwnerId,
            EntityId,
            BaseVersion,
            ServerVersion,
            CreateCommentSnapshot("Local"),
            CreateCommentSnapshot("Server"));

    [Fact]
    public void ForNote_ShouldCreatePendingNoteConflict()
    {
        var local = CreateNoteSnapshot("Local");
        var server = CreateNoteSnapshot("Server");

        var conflict = SyncConflict.ForNote(
            OwnerId, EntityId, BaseVersion, ServerVersion, local, server);

        Assert.Equal(OwnerId, conflict.OwnerId);
        Assert.Equal(EntityId, conflict.EntityId);
        Assert.Equal(SyncEntityType.Note, conflict.EntityType);
        Assert.Equal(BaseVersion, conflict.BaseVersion);
        Assert.Equal(ServerVersion, conflict.ServerVersion);
        Assert.Equal(local, conflict.LocalNoteSnapshot);
        Assert.Equal(server, conflict.ServerNoteSnapshot);
        Assert.Equal(ConflictStatus.Pending, conflict.Status);
        Assert.Null(conflict.ResolutionType);
        Assert.Null(conflict.ResolvedNoteSnapshot);
    }

    [Fact]
    public void ForNote_ShouldTrimOwnerId()
    {
        var conflict = SyncConflict.ForNote(
            $" {OwnerId} ",
            EntityId,
            BaseVersion,
            ServerVersion,
            CreateNoteSnapshot(),
            CreateNoteSnapshot());

        Assert.Equal(OwnerId, conflict.OwnerId);
    }

    [Fact]
    public void ForTag_ShouldCreatePendingTagConflict()
    {
        var local = CreateTagSnapshot("Local");
        var server = CreateTagSnapshot("Server");

        var conflict = SyncConflict.ForTag(
            OwnerId, EntityId, BaseVersion, ServerVersion, local, server);

        Assert.Equal(SyncEntityType.Tag, conflict.EntityType);
        Assert.Equal(local, conflict.LocalTagSnapshot);
        Assert.Equal(server, conflict.ServerTagSnapshot);
        Assert.Null(conflict.LocalNoteSnapshot);
        Assert.Null(conflict.LocalCommentSnapshot);
    }

    [Fact]
    public void ForComment_ShouldCreatePendingCommentConflict()
    {
        var local = CreateCommentSnapshot("Local");
        var server = CreateCommentSnapshot("Server");

        var conflict = SyncConflict.ForComment(
            OwnerId, EntityId, BaseVersion, ServerVersion, local, server);

        Assert.Equal(SyncEntityType.Comment, conflict.EntityType);
        Assert.Equal(local, conflict.LocalCommentSnapshot);
        Assert.Equal(server, conflict.ServerCommentSnapshot);
        Assert.Null(conflict.LocalNoteSnapshot);
        Assert.Null(conflict.LocalTagSnapshot);
    }

    [Fact]
    public void ForComment_ShouldRejectDifferentNoteIds()
    {
        var local = CreateCommentSnapshot(noteId: NoteId);
        var server = CreateCommentSnapshot(noteId: Guid.NewGuid());

        Assert.Throws<DomainException>(() =>
            SyncConflict.ForComment(
                OwnerId, EntityId, BaseVersion, ServerVersion, local, server));
    }

    [Fact]
    public void ForComment_ShouldRejectDifferentAuthorIds()
    {
        var local = CreateCommentSnapshot(authorId: "author-1");
        var server = CreateCommentSnapshot(authorId: "author-2");

        Assert.Throws<DomainException>(() =>
            SyncConflict.ForComment(
                OwnerId, EntityId, BaseVersion, ServerVersion, local, server));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ForNote_ShouldRejectInvalidOwner(string? ownerId)
    {
        Assert.Throws<DomainException>(() =>
            SyncConflict.ForNote(
                ownerId!,
                EntityId,
                BaseVersion,
                ServerVersion,
                CreateNoteSnapshot(),
                CreateNoteSnapshot()));
    }

    [Fact]
    public void ForNote_ShouldRejectEmptyEntityId()
    {
        Assert.Throws<DomainException>(() =>
            SyncConflict.ForNote(
                OwnerId,
                Guid.Empty,
                BaseVersion,
                ServerVersion,
                CreateNoteSnapshot(),
                CreateNoteSnapshot()));
    }

    [Fact]
    public void ForNote_ShouldRejectNullSnapshots()
    {
        Assert.Throws<DomainException>(() =>
            SyncConflict.ForNote(
                OwnerId,
                EntityId,
                BaseVersion,
                ServerVersion,
                null!,
                CreateNoteSnapshot()));

        Assert.Throws<DomainException>(() =>
            SyncConflict.ForNote(
                OwnerId,
                EntityId,
                BaseVersion,
                ServerVersion,
                CreateNoteSnapshot(),
                null!));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(2)]
    public void ForNote_ShouldRejectServerVersionNotGreaterThanBase(long version)
    {
        Assert.Throws<DomainException>(() =>
            SyncConflict.ForNote(
                OwnerId,
                EntityId,
                BaseVersion,
                SyncVersion.From(version),
                CreateNoteSnapshot(),
                CreateNoteSnapshot()));
    }

    [Fact]
    public void ResolveLocal_ShouldSelectLocalSnapshot()
    {
        var conflict = CreateNoteConflict();
        var resolvedAt = DateTimeOffset.UtcNow;

        conflict.ResolveLocal(OwnerId, resolvedAt);

        Assert.Equal(ConflictStatus.Resolved, conflict.Status);
        Assert.Equal(ConflictResolutionType.Local, conflict.ResolutionType);
        Assert.Equal(conflict.LocalNoteSnapshot, conflict.ResolvedNoteSnapshot);
        Assert.Equal(OwnerId, conflict.ResolvedBy);
        Assert.Equal(resolvedAt, conflict.ResolvedAt);
    }

    [Fact]
    public void ResolveLocal_ShouldTrimResolvedBy()
    {
        var conflict = CreateNoteConflict();

        conflict.ResolveLocal($" {OwnerId} ", DateTimeOffset.UtcNow);

        Assert.Equal(OwnerId, conflict.ResolvedBy);
    }

    [Fact]
    public void ResolveServer_ShouldSelectServerSnapshot()
    {
        var conflict = CreateTagConflict();
        var resolvedAt = DateTimeOffset.UtcNow;

        conflict.ResolveServer(OwnerId, resolvedAt);

        Assert.Equal(ConflictStatus.Resolved, conflict.Status);
        Assert.Equal(ConflictResolutionType.Server, conflict.ResolutionType);
        Assert.Equal(conflict.ServerTagSnapshot, conflict.ResolvedTagSnapshot);
        Assert.Equal(OwnerId, conflict.ResolvedBy);
        Assert.Equal(resolvedAt, conflict.ResolvedAt);
    }

    [Fact]
    public void ResolveMerged_ShouldUseProvidedSnapshot()
    {
        var conflict = CreateCommentConflict();
        var merged = CreateCommentSnapshot("Merged");
        var resolvedAt = DateTimeOffset.UtcNow;

        conflict.ResolveMerged(merged, OwnerId, resolvedAt);

        Assert.Equal(ConflictStatus.Resolved, conflict.Status);
        Assert.Equal(ConflictResolutionType.Merged, conflict.ResolutionType);
        Assert.Equal(merged, conflict.ResolvedCommentSnapshot);
        Assert.Equal(OwnerId, conflict.ResolvedBy);
        Assert.Equal(resolvedAt, conflict.ResolvedAt);
    }

    [Fact]
    public void ResolveMergedComment_ShouldRejectChangedNoteId()
    {
        var conflict = CreateCommentConflict();
        var merged = CreateCommentSnapshot(
            "Merged",
            noteId: Guid.NewGuid());

        Assert.Throws<DomainException>(() =>
            conflict.ResolveMerged(merged, OwnerId, DateTimeOffset.UtcNow));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
        Assert.Null(conflict.ResolvedCommentSnapshot);
        Assert.Null(conflict.ResolutionType);
        Assert.Null(conflict.ResolvedAt);
        Assert.Null(conflict.ResolvedBy);
    }

    [Fact]
    public void ResolveMergedComment_ShouldRejectChangedAuthorId()
    {
        var conflict = CreateCommentConflict();
        var merged = CreateCommentSnapshot(
            "Merged",
            authorId: "another-author");

        Assert.Throws<DomainException>(() =>
            conflict.ResolveMerged(merged, OwnerId, DateTimeOffset.UtcNow));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
        Assert.Null(conflict.ResolvedCommentSnapshot);
        Assert.Null(conflict.ResolutionType);
        Assert.Null(conflict.ResolvedAt);
        Assert.Null(conflict.ResolvedBy);
    }

    [Fact]
    public void ResolveMerged_ShouldRejectWrongEntityType()
    {
        var conflict = CreateNoteConflict();

        Assert.Throws<DomainException>(() =>
            conflict.ResolveMerged(
                CreateTagSnapshot(),
                OwnerId,
                DateTimeOffset.UtcNow));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
    }

    [Fact]
    public void ResolveLocal_ShouldRejectAnotherUser()
    {
        var conflict = CreateNoteConflict();

        Assert.Throws<DomainException>(() =>
            conflict.ResolveLocal(OtherUserId, DateTimeOffset.UtcNow));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
    }

    [Fact]
    public void ResolveServer_ShouldRejectAnotherUser()
    {
        var conflict = CreateNoteConflict();

        Assert.Throws<DomainException>(() =>
            conflict.ResolveServer(OtherUserId, DateTimeOffset.UtcNow));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
    }

    [Fact]
    public void ResolveMerged_ShouldRejectAnotherUser()
    {
        var conflict = CreateNoteConflict();

        Assert.Throws<DomainException>(() =>
            conflict.ResolveMerged(
                CreateNoteSnapshot(),
                OtherUserId,
                DateTimeOffset.UtcNow));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
    }

    [Fact]
    public void ResolveLocal_ShouldRejectNullOwner()
    {
        var conflict = CreateNoteConflict();

        Assert.Throws<DomainException>(() =>
            conflict.ResolveLocal(null!, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void ResolveMerged_ShouldRejectNullSnapshot()
    {
        var conflict = CreateNoteConflict();

        Assert.Throws<ArgumentNullException>(() =>
            conflict.ResolveMerged(
                (NoteSnapshot)null!,
                OwnerId,
                DateTimeOffset.UtcNow));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
    }

    [Fact]
    public void ResolveLocal_ShouldRejectDefaultDateWithoutChangingState()
    {
        var conflict = CreateNoteConflict();
        var originalSnapshot = conflict.LocalNoteSnapshot;

        Assert.Throws<DomainException>(() =>
            conflict.ResolveLocal(OwnerId, default));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
        Assert.Null(conflict.ResolvedNoteSnapshot);
        Assert.Null(conflict.ResolutionType);
        Assert.Null(conflict.ResolvedAt);
        Assert.Null(conflict.ResolvedBy);
        Assert.Equal(originalSnapshot, conflict.LocalNoteSnapshot);
    }

    [Fact]
    public void ResolveServer_ShouldRejectDefaultDateWithoutChangingState()
    {
        var conflict = CreateTagConflict();
        var originalSnapshot = conflict.ServerTagSnapshot;

        Assert.Throws<DomainException>(() =>
            conflict.ResolveServer(OwnerId, default));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
        Assert.Null(conflict.ResolvedTagSnapshot);
        Assert.Null(conflict.ResolutionType);
        Assert.Null(conflict.ResolvedAt);
        Assert.Null(conflict.ResolvedBy);
        Assert.Equal(originalSnapshot, conflict.ServerTagSnapshot);
    }

    [Fact]
    public void ResolveMerged_ShouldRejectDefaultDateWithoutChangingState()
    {
        var conflict = CreateNoteConflict();
        var merged = CreateNoteSnapshot("Merged");

        Assert.Throws<DomainException>(() =>
            conflict.ResolveMerged(merged, OwnerId, default));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
        Assert.Null(conflict.ResolvedNoteSnapshot);
        Assert.Null(conflict.ResolutionType);
        Assert.Null(conflict.ResolvedAt);
        Assert.Null(conflict.ResolvedBy);
    }

    [Fact]
    public void Resolve_ShouldRejectAlreadyResolvedConflict()
    {
        var conflict = CreateNoteConflict();

        conflict.ResolveLocal(OwnerId, DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() =>
            conflict.ResolveServer(OwnerId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Cancel_ShouldSetCancelledState()
    {
        var conflict = CreateNoteConflict();
        var cancelledAt = DateTimeOffset.UtcNow;

        conflict.Cancel(OwnerId, cancelledAt);

        Assert.Equal(ConflictStatus.Cancelled, conflict.Status);
        Assert.Equal(OwnerId, conflict.CancelledBy);
        Assert.Equal(cancelledAt, conflict.CancelledAt);
        Assert.Null(conflict.ResolutionType);
        Assert.Null(conflict.ResolvedAt);
    }

    [Fact]
    public void Cancel_ShouldTrimCancelledBy()
    {
        var conflict = CreateNoteConflict();

        conflict.Cancel($" {OwnerId} ", DateTimeOffset.UtcNow);

        Assert.Equal(OwnerId, conflict.CancelledBy);
    }

    [Fact]
    public void Cancel_ShouldRejectAnotherUser()
    {
        var conflict = CreateNoteConflict();

        Assert.Throws<DomainException>(() =>
            conflict.Cancel(OtherUserId, DateTimeOffset.UtcNow));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
    }

    [Fact]
    public void Cancel_ShouldRejectNullOwner()
    {
        var conflict = CreateNoteConflict();

        Assert.Throws<DomainException>(() =>
            conflict.Cancel(null!, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Cancel_ShouldRejectDefaultDateWithoutChangingState()
    {
        var conflict = CreateNoteConflict();

        Assert.Throws<DomainException>(() =>
            conflict.Cancel(OwnerId, default));

        Assert.Equal(ConflictStatus.Pending, conflict.Status);
        Assert.Null(conflict.CancelledAt);
        Assert.Null(conflict.CancelledBy);
        Assert.Null(conflict.ResolutionType);
        Assert.Null(conflict.ResolvedAt);
    }

    [Fact]
    public void Cancel_ShouldRejectAlreadyResolvedConflict()
    {
        var conflict = CreateNoteConflict();

        conflict.ResolveLocal(OwnerId, DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() =>
            conflict.Cancel(OwnerId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Cancel_ShouldRejectAlreadyCancelledConflict()
    {
        var conflict = CreateNoteConflict();

        conflict.Cancel(OwnerId, DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() =>
            conflict.Cancel(OwnerId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CancelledConflict_ShouldNotBeResolved()
    {
        var conflict = CreateNoteConflict();

        conflict.Cancel(OwnerId, DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() =>
            conflict.ResolveLocal(OwnerId, DateTimeOffset.UtcNow));
    }
}
