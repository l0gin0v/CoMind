namespace CoMind.Domain.ValueObjects;

public sealed class DeviceId : ValueObject
{
    public Guid Value { get; }

    private DeviceId(Guid value)
    {
        Value = value;
    }

    public static DeviceId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("Идентификатор устройства не может быть пустым.");

        return new DeviceId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
