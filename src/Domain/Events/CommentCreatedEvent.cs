namespace CoMind.Domain.Events;

public sealed record CommentCreatedEvent(Guid NoteId, string AuthorId) : Event;
