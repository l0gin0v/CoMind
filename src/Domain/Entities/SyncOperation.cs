namespace CoMind.Domain.Entities;

public sealed class SyncOperation : Entity
{
    public Guid EntityId { get; private set; }

    public SyncEntityType EntityType { get; private set; }

    public OperationType OperationType { get; private set; }

    public SyncVersion BaseVersion { get; private set; } = null!;

    public NoteSnapshot? NoteSnapshot { get; private set; }

    public TagSnapshot? TagSnapshot { get; private set; }

    public CommentSnapshot? CommentSnapshot { get; private set; }

    public SyncStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTimeOffset? LastAttemptAt { get; private set; }

    public string? Error { get; private set; }

    private SyncOperation()
    {
    }

    public static SyncOperation ForNote(
        Guid entityId,
        OperationType operationType,
        SyncVersion baseVersion,
        NoteSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return Create(
            entityId,
            SyncEntityType.Note,
            operationType,
            baseVersion,
            noteSnapshot: snapshot);
    }

    public static SyncOperation ForTag(
        Guid entityId,
        OperationType operationType,
        SyncVersion baseVersion,
        TagSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return Create(
            entityId,
            SyncEntityType.Tag,
            operationType,
            baseVersion,
            tagSnapshot: snapshot);
    }

    public static SyncOperation ForComment(
        Guid entityId,
        OperationType operationType,
        SyncVersion baseVersion,
        CommentSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return Create(
            entityId,
            SyncEntityType.Comment,
            operationType,
            baseVersion,
            commentSnapshot: snapshot);
    }

    private static SyncOperation Create(
        Guid entityId,
        SyncEntityType entityType,
        OperationType operationType,
        SyncVersion baseVersion,
        NoteSnapshot? noteSnapshot = null,
        TagSnapshot? tagSnapshot = null,
        CommentSnapshot? commentSnapshot = null)
    {
        if (entityId == Guid.Empty)
            throw new DomainException("Необходимо указать идентификатор сущности.");

        ArgumentNullException.ThrowIfNull(baseVersion);

        if (!Enum.IsDefined(operationType))
            throw new DomainException("Неизвестный тип операции синхронизации.");
        
        if (!Enum.IsDefined(entityType))
            throw new DomainException("Неизвестный тип синхронизируемой сущности.");
        
        var snapshotsCount =
            (noteSnapshot is not null ? 1 : 0) +
            (tagSnapshot is not null ? 1 : 0) +
            (commentSnapshot is not null ? 1 : 0);

        if (snapshotsCount != 1)
            throw new DomainException("Операция должна содержать ровно один snapshot.");

        var snapshotMatchesEntityType = entityType switch
        {
            SyncEntityType.Note => noteSnapshot is not null,
            SyncEntityType.Tag => tagSnapshot is not null,
            SyncEntityType.Comment => commentSnapshot is not null,
            _ => false
        };
        
        if (!snapshotMatchesEntityType)
            throw new DomainException("Тип snapshot не соответствует типу сущности.");

        return new SyncOperation
        {
            EntityId = entityId,
            EntityType = entityType,
            OperationType = operationType,
            BaseVersion = baseVersion,
            NoteSnapshot = noteSnapshot,
            TagSnapshot = tagSnapshot,
            CommentSnapshot = commentSnapshot,
            Status = SyncStatus.Pending,
            AttemptCount = 0
        };
    }

    public void MarkInProgress(DateTimeOffset attemptedAt)
    {
        EnsureStatus(SyncStatus.Pending);

        if (attemptedAt == default)
            throw new DomainException("Необходимо указать время попытки синхронизации.");

        if (AttemptCount == int.MaxValue)
            throw new DomainException("Достигнуто максимальное количество попыток синхронизации.");

        AttemptCount++;
        LastAttemptAt = attemptedAt;
        Error = null;
        Status = SyncStatus.InProgress;
    }

    public void MarkCompleted()
    {
        EnsureStatus(SyncStatus.InProgress);

        Error = null;
        Status = SyncStatus.Completed;
    }

    public void MarkFailed(string error)
    {
        EnsureStatus(SyncStatus.InProgress);

        if (string.IsNullOrWhiteSpace(error))
            throw new DomainException("Необходимо указать причину ошибки синхронизации.");

        Error = error.Trim();
        Status = SyncStatus.Failed;
    }

    public void MarkConflict()
    {
        EnsureStatus(SyncStatus.InProgress);

        Error = null;
        Status = SyncStatus.Conflict;
    }

    public void Retry()
    {
        EnsureStatus(SyncStatus.Failed);

        Error = null;
        Status = SyncStatus.Pending;
    }

    public void ReturnToPending()
    {
        EnsureStatus(SyncStatus.InProgress);

        Error = null;
        Status = SyncStatus.Pending;
    }

    private void EnsureStatus(SyncStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new DomainException(
                $"Операция находится в статусе {Status}. " +
                $"Ожидаемый статус: {expectedStatus}.");
        }
    }
}
