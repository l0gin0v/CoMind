namespace CoMind.Domain.ValueObjects;

public class TagName : ValueObject
{
    public string Value { get; }
    
    private TagName(string value) => Value = value;

    public static TagName Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Имя тэга не может быть пустым.");

        name = name.Trim().ToLowerInvariant();

        if (name.Length > 50)
            throw new DomainException("Имя тэга не может превышать 50 символов.");

        return new TagName(name);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
    public static implicit operator string(TagName name) => name.Value;
}
