namespace CoMind.Domain.Entities;

public class Note : AuditableEntity
{
    public string UserId { get; private set; } = default!;

    public NoteTitle Title { get; private set; } = default!;

    public NoteContent Content { get; private set; } = default!;

    private readonly List<Comment> _comments = [];
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    private readonly List<Tag> _tags = [];
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    private Note() { }

    public static Note Create(
        string userId,
        string title,
        string content)
    {
        throw new NotImplementedException();
    }
}
