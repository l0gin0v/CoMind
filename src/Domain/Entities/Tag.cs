using System.Net;
using CoMind.Domain.Events;

namespace CoMind.Domain.Entities;

public class Tag : Entity
{
    public TagName Name { get; private set; } = default!;
    
    public Color? Color { get; private set; }

    public string UserId { get; private set; } = default!;

    private Tag() { }

    public static Tag Create(string userId, string name, Color? color = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("Необходимо указать пользователя.");

        var tag = new Tag
        {
            UserId = userId,
            Name = TagName.Create(name),
            Color = color,
        };
        
        tag.AddDomainEvent(new TagCreatedEvent(tag.Id, tag.Name.Value, tag.UserId));
        return tag;
    }

    public void Rename(string name)
    {
        Name = TagName.Create(name);
    }
    
    public void changeColor(Color? color)
    {
        Color = color;
    }
}
