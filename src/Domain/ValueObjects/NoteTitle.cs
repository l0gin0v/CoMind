namespace CoMind.Domain.ValueObjects;

public sealed class NoteTitle : ValueObject
{
    public string Value { get; }

    private NoteTitle(string value) => Value = value;

    public static NoteTitle Create(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Заголовок заметки не может быть пустым.");

        title = title.Trim();

        if (title.Length > 200)
            throw new DomainException("Заголовок заметки не может превышать 200 символов.");

        return new NoteTitle(title);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(NoteTitle title) => title.Value;
}
