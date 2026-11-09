using CoMind.Domain.Events;

namespace CoMind.Domain.Entities;

public class Comment : AuditableEntity
{
    public string Content { get; private set; } = default!;
    public int NoteId { get; private set; }
    public Note Note { get; private set; } = default!;

    public string AuthorId { get; private set; } = default!;

    private Comment() { }

    public static Comment Create(int noteId, string authorId, string content)
    {
        if (noteId <= 0)
            throw new DomainException("NoteId не может быть отрицательным.");
        if (string.IsNullOrWhiteSpace(authorId))
            throw new DomainException("Необходимо указать автора комментария.");
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Комментарий не может быть пустым.");
        if (content.Length > 2000)
            throw new DomainException("Комментарий не может превышать 2000 символов");

        var comment = new Comment
        {
            NoteId = noteId,
            AuthorId = authorId,
            Content = content.Trim(),
        };
        
        comment.AddDomainEvent(
            new CommentCreatedEvent(comment.NoteId, comment.AuthorId)
        );
        return comment;
    }

    public void UpdateContent(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new DomainException("Комментарий не может быть пустым.");
        if (newContent.Length > 2000)
            throw new DomainException("Комментарий не может превышать 2000 символов");
        
        newContent = newContent.Trim();
        
        if (Content == newContent)
            throw new DomainException("Новое содержимое должно отличаться от прошлого.");
        
        Content = newContent;
    }
}
