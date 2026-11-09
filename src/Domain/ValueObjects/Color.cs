namespace CoMind.Domain.ValueObjects;

public sealed class Color : ValueObject
{
    public string Code { get; }
    private Color(string code) => Code = code;

    private static readonly HashSet<string> SupportedCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "#E05C4D", "#D98B2B", "#4CAF50", "#26A69A",
        "#5C6BC0", "#AB47BC", "#78909C"
    };
    
    public static Color From(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Цвет не может быть пустым.");

        code = code.Trim().ToUpperInvariant();

        if (!SupportedCodes.Contains(code))
            throw new UnsupportedColorException(code);

        return new Color(code);
    }

    public static Color Red => From("#E05C4D");
    public static Color Orange => From("#D98B2B");
    public static Color Green => From("#4CAF50");
    public static Color Teal => From("#26A69A");
    public static Color Blue => From("#5C6BC0");
    public static Color Purple => From("#AB47BC");
    public static Color Grey => From("#78909C");

    public static implicit operator string(Color color)
        => color.ToString();

    public static explicit operator Color(string code) => From(code);

    public override string ToString() => Code;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
