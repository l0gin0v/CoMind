namespace CoMind.Domain.Events;

public sealed record TagCreatedEvent(int TagId, string NameValue, string TagUserId) : Event { }
