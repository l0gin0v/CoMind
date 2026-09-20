namespace CoMind.Domain.ValueObjects;

public class Timestamp : ValueObject
{
    protected override IEnumerable<object> GetEqualityComponents()
    {
        throw new NotImplementedException();
    }
}
