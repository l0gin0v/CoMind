using CoMind.Domain.Common;

namespace CoMind.Domain.UnitTests.Common.Entities;

public class EntityTests
{
    private class TestEntity : Entity { }
    public class TestEvent : Event { }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestEvent();

        // Act
        entity.AddDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(domainEvent);
    }

    [Fact]
    public void RemoveDomainEvent_ShouldRemoveEventFromCollection()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestEvent();
        entity.AddDomainEvent(domainEvent);

        // Act
        entity.RemoveDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEventsFromCollection()
    {
        // Arrange
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestEvent());
        entity.AddDomainEvent(new TestEvent());

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnlyCollection()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        // Проверяем, что коллекцию нельзя скастить и напрямую изменить
        (entity.DomainEvents as List<Event>).Should().BeNull();
        
        // Проверяем интерфейс защиты
        entity.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<Event>>();
    }
}
