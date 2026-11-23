using CoMind.Domain.Exceptions;

namespace CoMind.Domain.ValueObjects;

public sealed class NoteContent : ValueObject
{
    public string Markdown { get; }

    private NoteContent(string markdown) => Markdown = markdown;

    public static NoteContent Create(string? markdown)
    {
        if (markdown is null)
            throw new DomainException("Содержимое заметки не может быть null.");

        markdown = markdown
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');

        return new NoteContent(markdown);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Markdown;
    }

    public override string ToString() => Markdown;

    public static implicit operator string(NoteContent content)
        => content.Markdown;
}
