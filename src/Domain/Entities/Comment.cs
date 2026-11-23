namespace CoMind.Domain.Entities;

public class Comment : AuditableEntity, ISyncable
{
    public string Content { get; private set; } = default!;

    public Guid NoteId { get; private set; }

    public Note Note { get; private set; } = default!;

    public string AuthorId { get; private set; } = default!;
    
    private SyncVersion _syncVersion = SyncVersion.Initial();

    public SyncVersion SyncVersion => _syncVersion;

    private Comment() { }

    public static Comment Create(Note note, string authorId, string content)
    {
        ArgumentNullException.ThrowIfNull(note);

        if (note.IsDeleted)
            throw new DomainException("Нельзя создавать комментарии к удалённой заметке.");

        if (string.IsNullOrWhiteSpace(authorId))
            throw new DomainException("Необходимо указать автора комментария.");

        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Комментарий не может быть пустым.");

        content = content.Trim();

        if (content.Length > 2000)
            throw new DomainException("Комментарий не может превышать 2000 символов.");

        var comment = new Comment
        {
            NoteId = note.Id,
            AuthorId = authorId,
            Content = content
        };

        comment.AddDomainEvent(new CommentCreatedEvent(comment.NoteId, comment.AuthorId));

        return comment;
    }

    public void UpdateContent(string newContent, string userId, Note note)
    {
        ArgumentNullException.ThrowIfNull(note);

        if (note.Id != NoteId)
            throw new DomainException("Указана другая заметка.");

        if (note.IsDeleted)
            throw new DomainException("Нельзя изменять комментарий удалённой заметки.");
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("Необходимо указать пользователя.");

        if (AuthorId != userId)
            throw new DomainException("Редактировать комментарий может только его автор.");
        
        if (string.IsNullOrWhiteSpace(newContent))
            throw new DomainException("Комментарий не может быть пустым.");

        newContent = newContent.Trim();

        if (newContent.Length > 2000)
            throw new DomainException("Комментарий не может превышать 2000 символов.");

        if (Content == newContent)
            throw new DomainException("Новое содержимое должно отличаться от прошлого.");

        Content = newContent;
    }
}
