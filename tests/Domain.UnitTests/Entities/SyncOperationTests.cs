using CoMind.Domain.Entities;
using CoMind.Domain.Enums;
using CoMind.Domain.Exceptions;
using CoMind.Domain.ValueObjects;

namespace CoMind.Domain.UnitTests.Entities;

public class SyncOperationTests
{
    private static readonly Guid EntityId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid NoteId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private const string OwnerId = "owner-1";

    private static readonly SyncVersion BaseVersion = SyncVersion.From(3);

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

    private static CommentSnapshot CreateCommentSnapshot(string content = "Test comment")
        => new(
            content,
            NoteId,
            OwnerId,
            false,
            DateTimeOffset.UtcNow,
            OwnerId,
            DateTimeOffset.UtcNow,
            OwnerId,
            null,
            null);

    private static SyncOperation CreateNoteOperation()
        => SyncOperation.ForNote(
            EntityId,
            OperationType.Update,
            BaseVersion,
            CreateNoteSnapshot());

    [Fact]
    public void ForNote_ShouldCreatePendingNoteOperation()
    {
        var snapshot = CreateNoteSnapshot();

        var operation = SyncOperation.ForNote(
            EntityId,
            OperationType.Create,
            BaseVersion,
            snapshot);

        Assert.NotEqual(Guid.Empty, operation.Id);
        Assert.Equal(EntityId, operation.EntityId);
        Assert.Equal(SyncEntityType.Note, operation.EntityType);
        Assert.Equal(OperationType.Create, operation.OperationType);
        Assert.Equal(BaseVersion, operation.BaseVersion);
        Assert.Equal(snapshot, operation.NoteSnapshot);
        Assert.Null(operation.TagSnapshot);
        Assert.Null(operation.CommentSnapshot);
        Assert.Equal(SyncStatus.Pending, operation.Status);
        Assert.Equal(0, operation.AttemptCount);
        Assert.Null(operation.LastAttemptAt);
        Assert.Null(operation.Error);
    }

    [Fact]
    public void ForTag_ShouldCreatePendingTagOperation()
    {
        var snapshot = CreateTagSnapshot();

        var operation = SyncOperation.ForTag(
            EntityId,
            OperationType.Update,
            BaseVersion,
            snapshot);

        Assert.Equal(EntityId, operation.EntityId);
        Assert.Equal(SyncEntityType.Tag, operation.EntityType);
        Assert.Equal(OperationType.Update, operation.OperationType);
        Assert.Equal(BaseVersion, operation.BaseVersion);
        Assert.Equal(snapshot, operation.TagSnapshot);
        Assert.Null(operation.NoteSnapshot);
        Assert.Null(operation.CommentSnapshot);
        Assert.Equal(SyncStatus.Pending, operation.Status);
    }

    [Fact]
    public void ForComment_ShouldCreatePendingCommentOperation()
    {
        var snapshot = CreateCommentSnapshot();

        var operation = SyncOperation.ForComment(
            EntityId,
            OperationType.Delete,
            BaseVersion,
            snapshot);

        Assert.Equal(EntityId, operation.EntityId);
        Assert.Equal(SyncEntityType.Comment, operation.EntityType);
        Assert.Equal(OperationType.Delete, operation.OperationType);
        Assert.Equal(BaseVersion, operation.BaseVersion);
        Assert.Equal(snapshot, operation.CommentSnapshot);
        Assert.Null(operation.NoteSnapshot);
        Assert.Null(operation.TagSnapshot);
        Assert.Equal(SyncStatus.Pending, operation.Status);
    }

    [Fact]
    public void ForNote_ShouldRejectEmptyEntityId()
    {
        Assert.Throws<DomainException>(() =>
            SyncOperation.ForNote(
                Guid.Empty,
                OperationType.Create,
                BaseVersion,
                CreateNoteSnapshot()));
    }

    [Fact]
    public void ForTag_ShouldRejectEmptyEntityId()
    {
        Assert.Throws<DomainException>(() =>
            SyncOperation.ForTag(
                Guid.Empty,
                OperationType.Create,
                BaseVersion,
                CreateTagSnapshot()));
    }

    [Fact]
    public void ForComment_ShouldRejectEmptyEntityId()
    {
        Assert.Throws<DomainException>(() =>
            SyncOperation.ForComment(
                Guid.Empty,
                OperationType.Create,
                BaseVersion,
                CreateCommentSnapshot()));
    }

    [Fact]
    public void ForNote_ShouldRejectNullBaseVersion()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SyncOperation.ForNote(
                EntityId,
                OperationType.Create,
                null!,
                CreateNoteSnapshot()));
    }

    [Fact]
    public void ForNote_ShouldRejectInvalidOperationType()
    {
        Assert.Throws<DomainException>(() =>
            SyncOperation.ForNote(
                EntityId,
                (OperationType)999,
                BaseVersion,
                CreateNoteSnapshot()));
    }

    [Fact]
    public void ForNote_ShouldRejectNullSnapshot()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SyncOperation.ForNote(
                EntityId,
                OperationType.Create,
                BaseVersion,
                null!));
    }

    [Fact]
    public void ForTag_ShouldRejectNullSnapshot()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SyncOperation.ForTag(
                EntityId,
                OperationType.Create,
                BaseVersion,
                null!));
    }

    [Fact]
    public void ForComment_ShouldRejectNullSnapshot()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SyncOperation.ForComment(
                EntityId,
                OperationType.Create,
                BaseVersion,
                null!));
    }

    [Fact]
    public void MarkInProgress_ShouldUpdateStatusAndAttemptData()
    {
        var operation = CreateNoteOperation();
        var attemptedAt = DateTimeOffset.UtcNow;

        operation.MarkInProgress(attemptedAt);

        Assert.Equal(SyncStatus.InProgress, operation.Status);
        Assert.Equal(1, operation.AttemptCount);
        Assert.Equal(attemptedAt, operation.LastAttemptAt);
        Assert.Null(operation.Error);
    }

    [Fact]
    public void MarkInProgress_ShouldRejectDefaultDateWithoutChangingState()
    {
        var operation = CreateNoteOperation();

        Assert.Throws<DomainException>(() =>
            operation.MarkInProgress(default));

        Assert.Equal(SyncStatus.Pending, operation.Status);
        Assert.Equal(0, operation.AttemptCount);
        Assert.Null(operation.LastAttemptAt);
    }

    [Fact]
    public void MarkInProgress_ShouldIncrementAttemptCountOnEveryAttempt()
    {
        var operation = CreateNoteOperation();

        operation.MarkInProgress(DateTimeOffset.UtcNow);
        operation.MarkFailed("Network error");
        operation.Retry();

        var secondAttemptAt = DateTimeOffset.UtcNow;
        operation.MarkInProgress(secondAttemptAt);

        Assert.Equal(2, operation.AttemptCount);
        Assert.Equal(secondAttemptAt, operation.LastAttemptAt);
        Assert.Equal(SyncStatus.InProgress, operation.Status);
    }

    [Fact]
    public void MarkCompleted_ShouldSetCompletedStatus()
    {
        var operation = CreateNoteOperation();

        operation.MarkInProgress(DateTimeOffset.UtcNow);
        operation.MarkCompleted();

        Assert.Equal(SyncStatus.Completed, operation.Status);
        Assert.Null(operation.Error);
    }

    [Fact]
    public void MarkFailed_ShouldSetFailedStatusAndTrimError()
    {
        var operation = CreateNoteOperation();

        operation.MarkInProgress(DateTimeOffset.UtcNow);
        operation.MarkFailed(" Network error ");

        Assert.Equal(SyncStatus.Failed, operation.Status);
        Assert.Equal("Network error", operation.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void MarkFailed_ShouldRejectEmptyError(string? error)
    {
        var operation = CreateNoteOperation();
        operation.MarkInProgress(DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() =>
            operation.MarkFailed(error!));

        Assert.Equal(SyncStatus.InProgress, operation.Status);
        Assert.Null(operation.Error);
    }

    [Fact]
    public void MarkConflict_ShouldSetConflictStatus()
    {
        var operation = CreateNoteOperation();

        operation.MarkInProgress(DateTimeOffset.UtcNow);
        operation.MarkConflict();

        Assert.Equal(SyncStatus.Conflict, operation.Status);
        Assert.Null(operation.Error);
    }

    [Fact]
    public void Retry_ShouldReturnFailedOperationToPending()
    {
        var operation = CreateNoteOperation();

        operation.MarkInProgress(DateTimeOffset.UtcNow);
        operation.MarkFailed("Network error");
        operation.Retry();

        Assert.Equal(SyncStatus.Pending, operation.Status);
        Assert.Null(operation.Error);
        Assert.Equal(1, operation.AttemptCount);
    }

    [Fact]
    public void ReturnToPending_ShouldRestoreInterruptedOperation()
    {
        var operation = CreateNoteOperation();

        operation.MarkInProgress(DateTimeOffset.UtcNow);
        operation.ReturnToPending();

        Assert.Equal(SyncStatus.Pending, operation.Status);
        Assert.Equal(1, operation.AttemptCount);
        Assert.Null(operation.Error);
    }

    [Fact]
    public void ReturnToPending_ShouldPreserveLastAttemptAt()
    {
        var operation = CreateNoteOperation();
        var attemptedAt = DateTimeOffset.UtcNow;

        operation.MarkInProgress(attemptedAt);
        operation.ReturnToPending();

        Assert.Equal(attemptedAt, operation.LastAttemptAt);
    }

    [Fact]
    public void MarkCompleted_ShouldRejectPendingOperation()
    {
        var operation = CreateNoteOperation();

        Assert.Throws<DomainException>(() =>
            operation.MarkCompleted());

        Assert.Equal(SyncStatus.Pending, operation.Status);
    }

    [Fact]
    public void MarkFailed_ShouldRejectPendingOperation()
    {
        var operation = CreateNoteOperation();

        Assert.Throws<DomainException>(() =>
            operation.MarkFailed("Network error"));

        Assert.Equal(SyncStatus.Pending, operation.Status);
    }

    [Fact]
    public void MarkConflict_ShouldRejectPendingOperation()
    {
        var operation = CreateNoteOperation();

        Assert.Throws<DomainException>(() =>
            operation.MarkConflict());

        Assert.Equal(SyncStatus.Pending, operation.Status);
    }

    [Fact]
    public void Retry_ShouldRejectOperationThatIsNotFailed()
    {
        var operation = CreateNoteOperation();

        Assert.Throws<DomainException>(() =>
            operation.Retry());

        Assert.Equal(SyncStatus.Pending, operation.Status);
    }

    [Fact]
    public void ReturnToPending_ShouldRejectOperationThatIsNotInProgress()
    {
        var operation = CreateNoteOperation();

        Assert.Throws<DomainException>(() =>
            operation.ReturnToPending());

        Assert.Equal(SyncStatus.Pending, operation.Status);
    }

    [Fact]
    public void CompletedOperation_ShouldBeTerminal()
    {
        var operation = CreateNoteOperation();

        operation.MarkInProgress(DateTimeOffset.UtcNow);
        operation.MarkCompleted();

        Assert.Throws<DomainException>(() =>
            operation.MarkInProgress(DateTimeOffset.UtcNow));

        Assert.Throws<DomainException>(() =>
            operation.Retry());

        Assert.Equal(SyncStatus.Completed, operation.Status);
    }

    [Fact]
    public void ConflictOperation_ShouldBeTerminal()
    {
        var operation = CreateNoteOperation();

        operation.MarkInProgress(DateTimeOffset.UtcNow);
        operation.MarkConflict();

        Assert.Throws<DomainException>(() =>
            operation.MarkInProgress(DateTimeOffset.UtcNow));

        Assert.Throws<DomainException>(() =>
            operation.Retry());

        Assert.Equal(SyncStatus.Conflict, operation.Status);
    }

    [Fact]
    public void Retry_ShouldAllowAnotherAttemptAfterFailure()
    {
        var operation = CreateNoteOperation();

        operation.MarkInProgress(DateTimeOffset.UtcNow);
        operation.MarkFailed("Network error");
        operation.Retry();

        operation.MarkInProgress(DateTimeOffset.UtcNow);
        operation.MarkCompleted();

        Assert.Equal(SyncStatus.Completed, operation.Status);
        Assert.Equal(2, operation.AttemptCount);
        Assert.Null(operation.Error);
    }
}
