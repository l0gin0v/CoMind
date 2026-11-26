namespace CoMind.Domain.ValueObjects;

public sealed class NoteSnapshot : ValueObject
{
    public NoteTitle Title { get; }
    public NoteContent Content { get; }
    public Guid? TagId { get; }

    public bool IsDeleted { get; }
    public DateTimeOffset CreatedAt { get; }
    public string? CreatedBy { get; }
    public DateTimeOffset UpdatedAt { get; }
    public string? UpdatedBy { get; }
    public DateTimeOffset? DeletedAt { get; }
    public string? DeletedBy { get; }

    public NoteSnapshot(
        NoteTitle title,
        NoteContent content,
        Guid? tagId,
        bool isDeleted,
        DateTimeOffset createdAt,
        string? createdBy,
        DateTimeOffset updatedAt,
        string? updatedBy,
        DateTimeOffset? deletedAt,
        string? deletedBy)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(content);

        if (isDeleted != deletedAt.HasValue)
            throw new DomainException("Состояние удаления должно соответствовать дате удаления.");
        
        Title = title;
        Content = content;
        TagId = tagId;
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
        yield return Title;
        yield return Content;
        yield return TagId;
        yield return IsDeleted;
        yield return CreatedAt;
        yield return CreatedBy;
        yield return UpdatedAt;
        yield return UpdatedBy;
        yield return DeletedAt;
        yield return DeletedBy;
    }
}
