using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Shared;

/// <summary>
/// Strongly typed Party ID value object
/// </summary>
[JsonConverter(typeof(PartyIdJsonConverter))]
public readonly record struct PartyId
{
    public int Value { get; }

    public PartyId(int value)
    {
        if (value <= 0)
            throw new ArgumentException("PartyId must be greater than 0", nameof(value));
        Value = value;
    }

    public static implicit operator int(PartyId partyId) => partyId.Value;
    public static implicit operator PartyId(int value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly typed User ID value object
/// </summary>
[JsonConverter(typeof(UserIdJsonConverter))]
public readonly record struct UserId
{
    public int Value { get; }

    public UserId(int value)
    {
        if (value <= 0)
            throw new ArgumentException("UserId must be greater than 0", nameof(value));
        Value = value;
    }

    public static implicit operator int(UserId userId) => userId.Value;
    public static implicit operator UserId(int value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly typed Resource ID value object
/// </summary>
[JsonConverter(typeof(ResourceIdJsonConverter))]
public readonly record struct ResourceId
{
    public string Value { get; }

    public ResourceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ResourceId cannot be null or whitespace", nameof(value));
        Value = value;
    }

    public static implicit operator string(ResourceId resourceId) => resourceId.Value;
    public static implicit operator ResourceId(string value) => new(value);
    public override string ToString() => Value;
}

/// <summary>
/// Strongly typed Delegation ID value object
/// </summary>
[JsonConverter(typeof(DelegationIdJsonConverter))]
public readonly record struct DelegationId
{
    public Guid Value { get; }

    public DelegationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DelegationId cannot be empty", nameof(value));
        Value = value;
    }

    public static DelegationId New() => new(Guid.NewGuid());
    public static implicit operator Guid(DelegationId delegationId) => delegationId.Value;
    public static implicit operator DelegationId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Strongly typed Consent ID value object
/// </summary>
[JsonConverter(typeof(ConsentIdJsonConverter))]
public readonly record struct ConsentId
{
    public Guid Value { get; }

    public ConsentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ConsentId cannot be empty", nameof(value));
        Value = value;
    }

    public static ConsentId New() => new(Guid.NewGuid());
    public static implicit operator Guid(ConsentId consentId) => consentId.Value;
    public static implicit operator ConsentId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Organization number value object with validation
/// </summary>
[JsonConverter(typeof(OrganizationNumberJsonConverter))]
public readonly record struct OrganizationNumber
{
    public string Value { get; }

    public OrganizationNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("OrganizationNumber cannot be null or whitespace", nameof(value));
        
        if (value.Length != 9 || !value.All(char.IsDigit))
            throw new ArgumentException("OrganizationNumber must be 9 digits", nameof(value));
        
        Value = value;
    }

    public static implicit operator string(OrganizationNumber orgNumber) => orgNumber.Value;
    public static implicit operator OrganizationNumber(string value) => new(value);
    public override string ToString() => Value;
}

/// <summary>
/// Person identifier value object with validation
/// </summary>
[JsonConverter(typeof(PersonIdentifierJsonConverter))]
public readonly record struct PersonIdentifier
{
    public string Value { get; }

    public PersonIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PersonIdentifier cannot be null or whitespace", nameof(value));
        
        if (value.Length != 11 || !value.All(char.IsDigit))
            throw new ArgumentException("PersonIdentifier must be 11 digits", nameof(value));
        
        Value = value;
    }

    public static implicit operator string(PersonIdentifier personId) => personId.Value;
    public static implicit operator PersonIdentifier(string value) => new(value);
    public override string ToString() => Value;
}

// JSON Converters
internal class PartyIdJsonConverter : JsonConverter<PartyId>
{
    public override PartyId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, PartyId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);
}

internal class UserIdJsonConverter : JsonConverter<UserId>
{
    public override UserId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, UserId value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);
}

internal class ResourceIdJsonConverter : JsonConverter<ResourceId>
{
    public override ResourceId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString() ?? string.Empty);

    public override void Write(Utf8JsonWriter writer, ResourceId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

internal class DelegationIdJsonConverter : JsonConverter<DelegationId>
{
    public override DelegationId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, DelegationId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

internal class ConsentIdJsonConverter : JsonConverter<ConsentId>
{
    public override ConsentId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, ConsentId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

internal class OrganizationNumberJsonConverter : JsonConverter<OrganizationNumber>
{
    public override OrganizationNumber Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString() ?? string.Empty);

    public override void Write(Utf8JsonWriter writer, OrganizationNumber value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

internal class PersonIdentifierJsonConverter : JsonConverter<PersonIdentifier>
{
    public override PersonIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString() ?? string.Empty);

    public override void Write(Utf8JsonWriter writer, PersonIdentifier value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

/// <summary>
/// Attribute match value object for XACML attributes
/// </summary>
public class AttributeMatch : IEquatable<AttributeMatch>
{
    public string Id { get; private set; }
    public string Value { get; private set; }
    public AttributeMatchType Type { get; private set; }
    public string? DataType { get; private set; }

    private AttributeMatch() { } // EF Constructor

    public AttributeMatch(string id, string value, AttributeMatchType type = AttributeMatchType.Equals, string? dataType = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty", nameof(id));
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));

        Id = id;
        Value = value;
        Type = type;
        DataType = dataType;
    }

    public bool Matches(string otherValue)
    {
        return Type switch
        {
            AttributeMatchType.Equals => string.Equals(Value, otherValue, StringComparison.OrdinalIgnoreCase),
            AttributeMatchType.Contains => otherValue.Contains(Value, StringComparison.OrdinalIgnoreCase),
            AttributeMatchType.StartsWith => otherValue.StartsWith(Value, StringComparison.OrdinalIgnoreCase),
            AttributeMatchType.EndsWith => otherValue.EndsWith(Value, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    public bool Equals(AttributeMatch? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id && Value == other.Value && Type == other.Type;
    }

    public override bool Equals(object? obj) => Equals(obj as AttributeMatch);
    public override int GetHashCode() => HashCode.Combine(Id, Value, Type);
}

/// <summary>
/// Enums for domain models
/// </summary>
public enum DelegationChangeType
{
    Created,
    Updated,
    Revoked,
    Restored
}

public enum AttributeMatchType
{
    Equals,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual
}