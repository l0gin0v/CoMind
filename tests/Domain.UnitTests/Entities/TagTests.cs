
using CoMind.Domain.Entities;
using CoMind.Domain.Events;
using CoMind.Domain.Exceptions;
using CoMind.Domain.ValueObjects;

namespace CoMind.Domain.UnitTests.Entities;

public class TagTests
{
    // ===== Create =====

    [Fact]
    public void Create_WithValidData_ReturnsTag()
    {
        var tag = Tag.Create("user-1", "C#", Color.Red);

        tag.Id.Should().NotBe(Guid.Empty);
        tag.UserId.Should().Be("user-1");
        tag.Name.Value.Should().Be("c#");
        tag.Color.Should().Be(Color.Red);
    }

    [Fact]
    public void Create_WithoutColor_ReturnsTag()
    {
        var tag = Tag.Create("user-1", "C#");

        tag.Color.Should().BeNull();
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
    public void Create_WithInvalidName_Throws()
    {
        var act = () => Tag.Create("user-1", "");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_RaisesTagCreatedEvent()
    {
        var tag = Tag.Create("user-1", "C#");

        tag.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TagCreatedEvent>()
            .Which.TagId.Should().Be(tag.Id);

        var evt = tag.DomainEvents
            .OfType<TagCreatedEvent>()
            .Single();

        evt.NameValue.Should().Be("c#");
        evt.TagUserId.Should().Be("user-1");
    }

    // ===== Rename =====

    [Fact]
    public void Rename_UpdatesName()
    {
        var tag = Tag.Create("user-1", "старое");

        tag.Rename("Новое");

        tag.Name.Value.Should().Be("новое");
    }

    [Fact]
    public void Rename_TrimsAndNormalizesName()
    {
        var tag = Tag.Create("user-1", "старое");

        tag.Rename("  НОВОЕ  ");

        tag.Name.Value.Should().Be("новое");
    }

    [Fact]
    public void Rename_WithSameName_Throws()
    {
        var tag = Tag.Create("user-1", "C#");

        var act = () => tag.Rename("c#");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithEmptyName_Throws(string? name)
    {
        var tag = Tag.Create("user-1", "старое");

        var act = () => tag.Rename(name!);

        act.Should().Throw<DomainException>();
    }

    // ===== ChangeColor =====

    [Fact]
    public void ChangeColor_UpdatesColor()
    {
        var tag = Tag.Create("user-1", "C#", Color.Red);

        tag.ChangeColor(Color.Blue);

        tag.Color.Should().Be(Color.Blue);
    }

    [Fact]
    public void ChangeColor_CanRemoveColor()
    {
        var tag = Tag.Create("user-1", "C#", Color.Red);

        tag.ChangeColor(null);

        tag.Color.Should().BeNull();
    }

    [Fact]
    public void ChangeColor_CanSetColorAfterRemovingIt()
    {
        var tag = Tag.Create("user-1", "C#", Color.Red);

        tag.ChangeColor(null);
        tag.ChangeColor(Color.Blue);

        tag.Color.Should().Be(Color.Blue);
    }

    [Fact]
    public void ChangeColor_WithSameColor_Throws()
    {
        var tag = Tag.Create("user-1", "C#", Color.Red);

        var act = () => tag.ChangeColor(Color.Red);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeColor_WithBothColorsNull_Throws()
    {
        var tag = Tag.Create("user-1", "C#");

        var act = () => tag.ChangeColor(null);

        act.Should().Throw<DomainException>();
    }
}
