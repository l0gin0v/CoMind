namespace CoMind.Domain.ValueObjects;

public sealed class TagSnapshot : ValueObject
{
    public TagName Name { get; }
    public Color? Color { get; }

    public bool IsDeleted { get; }
    public DateTimeOffset CreatedAt { get; }
    public string? CreatedBy { get; }
    public DateTimeOffset UpdatedAt { get; }
    public string? UpdatedBy { get; }
    public DateTimeOffset? DeletedAt { get; }
    public string? DeletedBy { get; }

    public TagSnapshot(
        TagName name,
        Color? color,
        bool isDeleted,
        DateTimeOffset createdAt,
        string? createdBy,
        DateTimeOffset updatedAt,
        string? updatedBy,
        DateTimeOffset? deletedAt,
        string? deletedBy)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (isDeleted != deletedAt.HasValue)
            throw new DomainException("Состояние удаления должно соответствовать дате удаления.");
        
        Name = name;
        Color = color;
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
        yield return Name;
        yield return Color;
        yield return IsDeleted;
        yield return CreatedAt;
        yield return CreatedBy;
        yield return UpdatedAt;
        yield return UpdatedBy;
        yield return DeletedAt;
        yield return DeletedBy;
    }
}
