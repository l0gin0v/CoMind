using System.Globalization;

namespace CoMind.Domain.ValueObjects;

public sealed class SyncVersion : ValueObject
{
    public long Value { get; }

    private SyncVersion(long value)
    {
        Value = value;
    }

    public static SyncVersion Initial()
        => new(0);

    public static SyncVersion From(long value)
    {
        if (value < 0)
            throw new DomainException("Версия синхронизации не может быть отрицательной.");

        return new SyncVersion(value);
    }

    public SyncVersion Increment()
    {
        if (Value == long.MaxValue)
            throw new DomainException("Достигнуто максимальное значение версии синхронизации.");

        return new SyncVersion(Value + 1);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
