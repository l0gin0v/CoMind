namespace CoMind.Domain.Entities;

public class SyncConflict : AuditableEntity
{
    public string OwnerId { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public SyncEntityType EntityType { get; private set; }

    public SyncVersion BaseVersion { get; private set; } = default!;
    public SyncVersion ServerVersion { get; private set; } = default!;

    public NoteSnapshot? LocalNoteSnapshot { get; private set; }
    public NoteSnapshot? ServerNoteSnapshot { get; private set; }

    public TagSnapshot? LocalTagSnapshot { get; private set; }
    public TagSnapshot? ServerTagSnapshot { get; private set; }

    public CommentSnapshot? LocalCommentSnapshot { get; private set; }
    public CommentSnapshot? ServerCommentSnapshot { get; private set; }

    public NoteSnapshot? ResolvedNoteSnapshot { get; private set; }
    public TagSnapshot? ResolvedTagSnapshot { get; private set; }
    public CommentSnapshot? ResolvedCommentSnapshot { get; private set; }

    public ConflictStatus Status { get; private set; }
        = ConflictStatus.Pending;

    public ConflictResolutionType? ResolutionType { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public string? ResolvedBy { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancelledBy { get; private set; }

    private SyncConflict() { }
    
    public static SyncConflict ForNote(
        string ownerId,
        Guid entityId,
        SyncVersion baseVersion,
        SyncVersion serverVersion,
        NoteSnapshot localSnapshot,
        NoteSnapshot serverSnapshot)
    {
        return Create(
            SyncEntityType.Note,
            ownerId,
            entityId,
            baseVersion,
            serverVersion,
            localNoteSnapshot: localSnapshot,
            serverNoteSnapshot: serverSnapshot);
    }
    
    public static SyncConflict ForTag(
        string ownerId,
        Guid entityId,
        SyncVersion baseVersion,
        SyncVersion serverVersion,
        TagSnapshot localSnapshot,
        TagSnapshot serverSnapshot)
    {
        return Create(
            SyncEntityType.Tag,
            ownerId,
            entityId,
            baseVersion,
            serverVersion,
            localTagSnapshot: localSnapshot,
            serverTagSnapshot: serverSnapshot);
    }
    
    public static SyncConflict ForComment(
        string ownerId,
        Guid entityId,
        SyncVersion baseVersion,
        SyncVersion serverVersion,
        CommentSnapshot localSnapshot,
        CommentSnapshot serverSnapshot)
    {
        return Create(
            SyncEntityType.Comment,
            ownerId,
            entityId,
            baseVersion,
            serverVersion,
            localCommentSnapshot: localSnapshot,
            serverCommentSnapshot: serverSnapshot);
    }

    private static SyncConflict Create(
        SyncEntityType entityType,
        string ownerId,
        Guid entityId,
        SyncVersion baseVersion,
        SyncVersion serverVersion,
        NoteSnapshot? localNoteSnapshot = null,
        NoteSnapshot? serverNoteSnapshot = null,
        TagSnapshot? localTagSnapshot = null,
        TagSnapshot? serverTagSnapshot = null,
        CommentSnapshot? localCommentSnapshot = null,
        CommentSnapshot? serverCommentSnapshot = null)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new DomainException("Необходимо указать владельца конфликта.");

        if (entityId == Guid.Empty)
            throw new DomainException("Необходимо указать идентификатор сущности.");

        ArgumentNullException.ThrowIfNull(baseVersion);
        ArgumentNullException.ThrowIfNull(serverVersion);

        if (serverVersion.Value <= baseVersion.Value)
            throw new DomainException(
                "Версия сервера должна быть больше базовой версии конфликта.");

        ValidateSnapshots(
            entityType,
            localNoteSnapshot,
            serverNoteSnapshot,
            localTagSnapshot,
            serverTagSnapshot,
            localCommentSnapshot,
            serverCommentSnapshot);

        return new SyncConflict
        {
            OwnerId = ownerId.Trim(),
            EntityId = entityId,
            EntityType = entityType,
            BaseVersion = baseVersion,
            ServerVersion = serverVersion,

            LocalNoteSnapshot = localNoteSnapshot,
            ServerNoteSnapshot = serverNoteSnapshot,

            LocalTagSnapshot = localTagSnapshot,
            ServerTagSnapshot = serverTagSnapshot,

            LocalCommentSnapshot = localCommentSnapshot,
            ServerCommentSnapshot = serverCommentSnapshot
        };
    }

    private static void ValidateSnapshots(
        SyncEntityType entityType,
        NoteSnapshot? localNote,
        NoteSnapshot? serverNote,
        TagSnapshot? localTag,
        TagSnapshot? serverTag,
        CommentSnapshot? localComment,
        CommentSnapshot? serverComment)
    {
        bool valid = entityType switch
        {
            SyncEntityType.Note =>
                localNote is not null &&
                serverNote is not null &&
                localTag is null &&
                serverTag is null &&
                localComment is null &&
                serverComment is null,

            SyncEntityType.Tag =>
                localTag is not null &&
                serverTag is not null &&
                localNote is null &&
                serverNote is null &&
                localComment is null &&
                serverComment is null,

            SyncEntityType.Comment =>
                localComment is not null &&
                serverComment is not null &&
                localNote is null &&
                serverNote is null &&
                localTag is null &&
                serverTag is null &&
                localComment.NoteId == serverComment.NoteId &&
                localComment.AuthorId == serverComment.AuthorId,

            _ => false
        };

        if (!valid)
            throw new DomainException(
                "Снимки конфликта не соответствуют типу сущности.");
    }
    
    public void ResolveLocal(string resolvedBy, DateTimeOffset resolvedAt)
    {
        EnsureCanResolve(resolvedBy, resolvedAt);

        switch (EntityType)
        {
            case SyncEntityType.Note:
                ResolvedNoteSnapshot = LocalNoteSnapshot;
                break;

            case SyncEntityType.Tag:
                ResolvedTagSnapshot = LocalTagSnapshot;
                break;

            case SyncEntityType.Comment:
                ResolvedCommentSnapshot = LocalCommentSnapshot;
                break;
        }

        CompleteResolution(ConflictResolutionType.Local, resolvedBy, resolvedAt);
    }
    
    public void ResolveServer(string resolvedBy, DateTimeOffset resolvedAt)
    {
        EnsureCanResolve(resolvedBy, resolvedAt);

        switch (EntityType)
        {
            case SyncEntityType.Note:
                ResolvedNoteSnapshot = ServerNoteSnapshot;
                break;

            case SyncEntityType.Tag:
                ResolvedTagSnapshot = ServerTagSnapshot;
                break;

            case SyncEntityType.Comment:
                ResolvedCommentSnapshot = ServerCommentSnapshot;
                break;
        }

        CompleteResolution(ConflictResolutionType.Server, resolvedBy, resolvedAt);
    }
    
    public void ResolveMerged(
        NoteSnapshot mergedSnapshot,
        string resolvedBy,
        DateTimeOffset resolvedAt)
    {
        EnsureCanResolve(resolvedBy, resolvedAt);
        EnsureEntityType(SyncEntityType.Note);
        ArgumentNullException.ThrowIfNull(mergedSnapshot);

        ResolvedNoteSnapshot = mergedSnapshot;
        CompleteResolution(ConflictResolutionType.Merged, resolvedBy, resolvedAt);
    }

    public void ResolveMerged(
        TagSnapshot mergedSnapshot,
        string resolvedBy,
        DateTimeOffset resolvedAt)
    {
        EnsureCanResolve(resolvedBy, resolvedAt);
        EnsureEntityType(SyncEntityType.Tag);
        ArgumentNullException.ThrowIfNull(mergedSnapshot);

        ResolvedTagSnapshot = mergedSnapshot;
        CompleteResolution(ConflictResolutionType.Merged, resolvedBy, resolvedAt);
    }

    public void ResolveMerged(
        CommentSnapshot mergedSnapshot,
        string resolvedBy,
        DateTimeOffset resolvedAt)
    {
        EnsureCanResolve(resolvedBy, resolvedAt);
        EnsureEntityType(SyncEntityType.Comment);
        ArgumentNullException.ThrowIfNull(mergedSnapshot);
        
        if (mergedSnapshot.NoteId != ServerCommentSnapshot!.NoteId ||
            mergedSnapshot.AuthorId != ServerCommentSnapshot.AuthorId)
        {
            throw new DomainException("Нельзя изменять заметку или автора комментария при разрешении конфликта.");
        }

        ResolvedCommentSnapshot = mergedSnapshot;
        CompleteResolution(ConflictResolutionType.Merged, resolvedBy, resolvedAt);
    }
    
    public void Cancel(string cancelledBy, DateTimeOffset cancelledAt)
    {
        EnsurePending();

        EnsureOwner(cancelledBy);
        
        if (cancelledAt == default)
            throw new DomainException("Необходимо указать дату отмены конфликта.");

        Status = ConflictStatus.Cancelled;
        CancelledBy = cancelledBy.Trim();
        CancelledAt = cancelledAt;
    }

    private void EnsureCanResolve(string resolvedBy, DateTimeOffset resolvedAt)
    {
        EnsurePending();
        EnsureOwner(resolvedBy);
        
        if (resolvedAt == default)
            throw new DomainException("Необходимо указать дату разрешения конфликта.");
    }

    private void EnsurePending()
    {
        if (Status != ConflictStatus.Pending)
            throw new DomainException(
                "Разрешить или отменить можно только ожидающий конфликт.");
    }

    private void EnsureOwner(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId) || userId.Trim() != OwnerId)
            throw new DomainException(
                "Операцию с конфликтом может выполнить только его владелец.");
    }

    private void EnsureEntityType(SyncEntityType expectedType)
    {
        if (EntityType != expectedType)
            throw new DomainException(
                "Тип снимка не соответствует типу конфликтующей сущности.");
    }

    private void CompleteResolution(
        ConflictResolutionType resolutionType,
        string resolvedBy,
        DateTimeOffset resolvedAt)
    {
        
        
        ResolutionType = resolutionType;
        ResolvedBy = resolvedBy.Trim();
        ResolvedAt = resolvedAt;
        Status = ConflictStatus.Resolved;
    }
}
