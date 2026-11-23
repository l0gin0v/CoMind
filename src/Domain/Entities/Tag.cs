namespace CoMind.Domain.Entities;

public class Tag : AuditableEntity, ISyncable
{
    public TagName Name { get; private set; } = default!;

    public Color? Color { get; private set; }

    public string UserId { get; private set; } = default!;
    
    private SyncVersion _syncVersion = SyncVersion.Initial();

    public SyncVersion SyncVersion => _syncVersion;

    private Tag() { }

    public static Tag Create(string userId, string name, Color? color = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("Необходимо указать пользователя.");

        var tag = new Tag
        {
            UserId = userId,
            Name = TagName.Create(name),
            Color = color
        };

        tag.AddDomainEvent(
            new TagCreatedEvent(tag.Id, tag.Name.Value, tag.UserId));

        return tag;
    }

    public void Rename(string name)
    {
        EnsureNotDeleted();
        
        var newName = TagName.Create(name);

        if (Name == newName)
            throw new DomainException("Новое название должно отличаться от текущего.");

        Name = newName;
    }

    public void ChangeColor(Color? color)
    {
        EnsureNotDeleted();
        
        if (Equals(Color, color))
            throw new DomainException("Новый цвет должен отличаться от текущего.");

        Color = color;
    }
    
    public void Restore()
    {
        if (!IsDeleted)
            throw new DomainException("Тег не удалён.");

        DeletedAt = null;
        DeletedBy = null;
    }
    
    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new DomainException("Нельзя изменять удалённый тег.");
    }
}
