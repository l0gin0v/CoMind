using CoMind.Domain.Entities;
using CoMind.Domain.Exceptions;
using CoMind.Domain.ValueObjects;

namespace CoMind.Domain.UnitTests.Entities;

public class TagTests
{
    [Fact]
    public void Create_WithValidData_ReturnsTag()
    {
        var tag = Tag.Create("user-1", "C#", Color.Red);

        tag.UserId.Should().Be("user-1");
        tag.Name.Value.Should().Be("c#");
        tag.Color.Should().Be(Color.Red);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyUserId_Throws(string? userId)
    {
        var act = () => Tag.Create(userId!, "c#");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Rename_UpdatesName()
    {
        var tag = Tag.Create("user-1", "старое");

        tag.Rename("Новое");

        tag.Name.Value.Should().Be("новое");
    }
}
