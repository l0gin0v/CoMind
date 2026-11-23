namespace CoMind.Domain.Events;

public sealed record TagCreatedEvent(Guid TagId, string NameValue, string TagUserId) : Event;
