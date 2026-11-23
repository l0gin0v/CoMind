namespace CoMind.Domain.Events;

public sealed record NoteCreatedEvent(Guid NoteId, string UserId) : Event;
