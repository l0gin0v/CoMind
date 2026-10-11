using CoMind.Domain.Common;

namespace CoMind.Domain.UnitTests.Common.ValueObjects;

public class ValueObjectTests
{
    private sealed class TestVo(params object?[] components) : ValueObject
    {
        protected override IEnumerable<object> GetEqualityComponents()
            => components.Select(c => c!);
    }
    
    private sealed class OtherVo(params object?[] c) : ValueObject
    {
        protected override IEnumerable<object> GetEqualityComponents()
            => c.Select(x => x!);
    }

    [Fact]
    public void Equals_ReturnsTrue_ForEqualComponents()
    {
        var a = new TestVo("x", 1, true);
        var b = new TestVo("x", 1, true);

        a.Equals(b).Should().BeTrue();
        b.Equals(a).Should().BeTrue(); // симметричность
    }

    [Fact]
    public void Equals_ReturnsTrue_ForSameInstance()
    {
        var a = new TestVo("x", 1);

        a.Equals(a).Should().BeTrue();
    }

    [Fact]
    public void Equals_ReturnsTrue_ForEmptyComponents()
        => new TestVo().Equals(new TestVo()).Should().BeTrue();

    [Fact]
    public void Equals_ReturnsTrue_ForNullComponent()
    {
        var a =  new TestVo([null]);
        var b =  new TestVo([null]);
        
        a.Equals(b).Should().BeTrue();
    }

    [Theory]
    [InlineData("y", 1)]
    [InlineData("x", 2)]
    [InlineData("x", 1, 9)]
    public void Equals_ReturnsFalse_WhenComponentsDiffer(
        string s, int i, int? extra = null)
    {
        var a = new TestVo("x", 1);
        var b = extra is null
            ? new TestVo(s, i)
            : new TestVo(s, i, extra);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenOtherIsNull()
        => new TestVo("x").Equals(null).Should().BeFalse();

    [Fact]
    public void Equals_ReturnsFalse_WhenOtherIsNotValueObject()
        => new TestVo("x").Equals("not a VO").Should().BeFalse();

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferentValueObjectType()
    {
        var a = new TestVo("x");
        var b = new OtherVo("x");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_ReflectsEquality()
    {
        var a = new TestVo("x", 1);
        var b = new TestVo("x", 1);
        var c = new TestVo("y", 1);

        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
        (a == c).Should().BeFalse();
        (a != c).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_HandlesNulls()
    {
        TestVo? nullLeft = null;
        TestVo? nullRight = null;
        var notNull = new TestVo("x");

        (nullLeft == nullRight).Should().BeTrue();
        (nullLeft == notNull).Should().BeFalse();
        (notNull == nullRight).Should().BeFalse();
        (nullLeft != notNull).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_IsEqual_ForEqualObjects()
    {
        var a = new TestVo("x", 1, true);
        var b = new TestVo("x", 1, true);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_IsStable_AcrossCalls()
    {
        var a = new TestVo("x", 1);
        var first = a.GetHashCode();

        a.GetHashCode().Should().Be(first);
        a.GetHashCode().Should().Be(first);
    }

    [Fact]
    public void WorksIn_HashSet_And_Dictionary()
    {
        var set = new HashSet<TestVo>
        {
            new("x", 1),
            new("x", 1),
            new("y", 1)
        };
        set.Should().HaveCount(2);

        var dict = new Dictionary<TestVo, string>
        {
            [new TestVo("x", 1)] = "a"
        };
        dict.ContainsKey(new TestVo("x", 1)).Should().BeTrue();
        dict.ContainsKey(new TestVo("x", 2)).Should().BeFalse();
    }
}
