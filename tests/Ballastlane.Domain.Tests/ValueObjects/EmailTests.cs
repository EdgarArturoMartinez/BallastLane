using Ballastlane.Domain.Exceptions;
using Ballastlane.Domain.ValueObjects;

namespace Ballastlane.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("  USER@EXAMPLE.COM  ")]   // normalization
    [InlineData("a@b.co")]
    public void Create_ValidEmail_ReturnsNormalized(string input)
    {
        var email = Email.Create(input);

        Assert.Equal(input.Trim().ToLowerInvariant(), email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyValue_ThrowsDomainException(string? value)
    {
        Assert.Throws<DomainException>(() => Email.Create(value!));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain")]
    [InlineData("noatsign.com")]
    [InlineData("user@")]
    public void Create_InvalidFormat_ThrowsDomainException(string bad)
    {
        Assert.Throws<DomainException>(() => Email.Create(bad));
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("USER@EXAMPLE.COM");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var a = Email.Create("a@example.com");
        var b = Email.Create("b@example.com");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        var email = Email.Create("Test@Example.COM");

        Assert.Equal("test@example.com", email.ToString());
    }
}
