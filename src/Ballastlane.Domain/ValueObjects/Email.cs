using Ballastlane.Domain.Exceptions;

namespace Ballastlane.Domain.ValueObjects;

/// <summary>
/// Value Object: Email — immutable, self-validating, compared by value.
/// Pattern: Value Object (DDD)
/// SOLID: SRP — responsible only for email validity and equality.
/// </summary>
public sealed class Email : IEquatable<Email>
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email is required.");

        var trimmed = value.Trim().ToLowerInvariant();

        // Basic structural validation — no external dependencies
        var atIndex = trimmed.IndexOf('@');
        if (atIndex <= 0 || atIndex == trimmed.Length - 1 || !trimmed[(atIndex + 1)..].Contains('.'))
            throw new DomainException($"'{value}' is not a valid email address.");

        return new Email(trimmed);
    }

    public override string ToString() => Value;
    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as Email);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
}
