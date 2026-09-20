using System.ComponentModel.DataAnnotations.Schema;

namespace CoMind.Domain.Common;

public abstract class Entity
{ 
    public int Id { get; set; }

    private readonly List<Event> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<Event> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(Event domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(Event domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
