namespace CoMind.Domain.Events;

public sealed record CommentCreatedEvent(int NoteId, string AuthorId) : Event;
