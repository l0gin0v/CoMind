namespace CoMind.Domain.ValueObjects;

public sealed class CommentSnapshot : ValueObject
{
    public string Content { get; }
    public Guid NoteId { get; }
    public string AuthorId { get; }

    public bool IsDeleted { get; }
    public DateTimeOffset CreatedAt { get; }
    public string? CreatedBy { get; }
    public DateTimeOffset UpdatedAt { get; }
    public string? UpdatedBy { get; }
    public DateTimeOffset? DeletedAt { get; }
    public string? DeletedBy { get; }

    public CommentSnapshot(
        string content,
        Guid noteId,
        string authorId,
        bool isDeleted,
        DateTimeOffset createdAt,
        string? createdBy,
        DateTimeOffset updatedAt,
        string? updatedBy,
        DateTimeOffset? deletedAt,
        string? deletedBy)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Содержимое комментария не может быть пустым.");

        content = content.Trim();

        if (content.Length > 2000)
            throw new DomainException("Комментарий не может превышать 2000 символов.");

        if (noteId == Guid.Empty)
            throw new DomainException("Необходимо указать заметку.");

        if (string.IsNullOrWhiteSpace(authorId))
            throw new DomainException("Необходимо указать автора комментария.");

        if (isDeleted != deletedAt.HasValue)
            throw new DomainException("Состояние удаления должно соответствовать дате удаления.");
        
        Content = content.Trim();
        NoteId = noteId;
        AuthorId = authorId;
        IsDeleted = isDeleted;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
        UpdatedAt = updatedAt;
        UpdatedBy = updatedBy;
        DeletedAt = deletedAt;
        DeletedBy = deletedBy;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Content;
        yield return NoteId;
        yield return AuthorId;
        yield return IsDeleted;
        yield return CreatedAt;
        yield return CreatedBy;
        yield return UpdatedAt;
        yield return UpdatedBy;
        yield return DeletedAt;
        yield return DeletedBy;
    }
}
