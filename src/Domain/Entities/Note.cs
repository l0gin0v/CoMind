namespace CoMind.Domain.Entities;

public class Note : AuditableEntity, ISyncable
{
    public string UserId { get; private set; } = default!;

    public NoteTitle Title { get; private set; } = default!;

    public NoteContent Content { get; private set; } = default!;

    public Guid? TagId { get; private set; }

    public Tag? Tag { get; private set; }
    
    private SyncVersion _syncVersion = SyncVersion.Initial();

    public SyncVersion SyncVersion => _syncVersion;

    private Note() { }

    public static Note Create(
        string userId,
        string title,
        string content)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("Необходимо указать пользователя.");

        var note = new Note
        {
            UserId = userId,
            Title = NoteTitle.Create(title),
            Content = NoteContent.Create(content)
        };

        note.AddDomainEvent(new NoteCreatedEvent(note.Id, note.UserId));

        return note;
    }

    public void UpdateTitle(string title)
    {
        EnsureNotDeleted();

        var newTitle = NoteTitle.Create(title);

        if (Title == newTitle)
            throw new DomainException("Новый заголовок должен отличаться от предыдущего.");

        Title = newTitle;
    }

    public void UpdateContent(string content)
    {
        EnsureNotDeleted();

        var newContent = NoteContent.Create(content);

        if (Content == newContent)
            throw new DomainException("Новое содержимое должно отличаться от предыдущего.");

        Content = newContent;
    }

    public void AssignTag(Tag tag)
    {
        EnsureNotDeleted();

        ArgumentNullException.ThrowIfNull(tag);

        if (tag.UserId != UserId)
            throw new DomainException("Нельзя назначить заметке тег другого пользователя.");
        
        if (tag.IsDeleted)
            throw new DomainException("Нельзя назначить удалённый тег.");

        if (TagId == tag.Id)
            throw new DomainException("Этот тег уже назначен заметке.");

        Tag = tag;
        TagId = tag.Id;
    }

    public void RemoveTag()
    {
        EnsureNotDeleted();

        if (TagId is null)
            throw new DomainException("У заметки нет назначенного тега.");

        Tag = null;
        TagId = null;
    }

    public void Delete(string deletedBy)
    {
        if (IsDeleted)
            throw new DomainException("Заметка уже удалена.");

        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new DomainException("Необходимо указать пользователя.");

        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        if (!IsDeleted)
            throw new DomainException("Заметка не удалена.");

        DeletedAt = null;
        DeletedBy = null;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new DomainException("Нельзя изменять удалённую заметку.");
    }
}
