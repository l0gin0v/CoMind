using CoMind.Domain.Exceptions;
using CoMind.Domain.ValueObjects;

namespace CoMind.Domain.UnitTests.ValueObjects;

public class SyncVersionTests
{
    [Fact]
    public void Initial_ShouldReturnZero()
    {
        var version = SyncVersion.Initial();

        Assert.Equal(0, version.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(long.MaxValue)]
    public void From_ShouldCreateVersion_WhenValueIsNonNegative(long value)
    {
        var version = SyncVersion.From(value);

        Assert.Equal(value, version.Value);
    }

    [Fact]
    public void From_ShouldThrow_WhenValueIsNegative()
    {
        Assert.Throws<DomainException>(
            () => SyncVersion.From(-1));
    }

    [Fact]
    public void Increment_ShouldIncreaseVersionByOne()
    {
        var version = SyncVersion.From(5);

        var incremented = version.Increment();

        Assert.Equal(6, incremented.Value);
    }

    [Fact]
    public void Increment_ShouldNotModifyOriginalVersion()
    {
        var version = SyncVersion.From(5);

        _ = version.Increment();

        Assert.Equal(5, version.Value);
    }

    [Fact]
    public void Increment_ShouldThrow_WhenValueIsMaxValue()
    {
        var version = SyncVersion.From(long.MaxValue);

        Assert.Throws<DomainException>(
            () =>
            {
                version.Increment();
                return version;
            });
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenVersionsAreEqual()
    {
        var first = SyncVersion.From(5);
        var second = SyncVersion.From(5);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenVersionsAreDifferent()
    {
        var first = SyncVersion.From(5);
        var second = SyncVersion.From(6);

        Assert.NotEqual(first, second);
    }
}
