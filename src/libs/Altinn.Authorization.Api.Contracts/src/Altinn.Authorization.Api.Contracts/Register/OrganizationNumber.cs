﻿#nullable enable

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Swashbuckle.Examples;

namespace Altinn.Authorization.Api.Contracts.Register;

/// <summary>
/// A organization number (a string of 9 digits).
/// </summary>
[JsonConverter(typeof(JsonConverter))]
public class OrganizationNumber : ISpanParsable<OrganizationNumber>,
    ISpanFormattable,
    IExampleDataProvider<OrganizationNumber>,
    IEquatable<OrganizationNumber>,
    IEqualityOperators<OrganizationNumber, OrganizationNumber, bool>
{
    private static readonly SearchValues<char> NUMBERS = SearchValues.Create(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);

    private readonly string _value;

    private OrganizationNumber(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Creates a new <see cref="OrganizationNumber"/> from the specified value without validation.
    /// </summary>
    /// <param name="value">The organization identifier.</param>
    /// <returns>A <see cref="OrganizationNumber"/>.</returns>
    public static OrganizationNumber CreateUnchecked(string value)
        => new(value);

    /// <inheritdoc/>
    public static IEnumerable<OrganizationNumber>? GetExamples(ExampleDataOptions options)
    {
        yield return new OrganizationNumber("987654321");
        yield return new OrganizationNumber("123456789");
    }

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
    public static OrganizationNumber Parse(string s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static OrganizationNumber Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid organization number");

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
    public static OrganizationNumber Parse(ReadOnlySpan<char> s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static OrganizationNumber Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid organization number");

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out OrganizationNumber result)
        => TryParse(s.AsSpan(), s, out result);

    /// <inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out OrganizationNumber result)
        => TryParse(s, original: null, out result);

    private static bool TryParse(ReadOnlySpan<char> s, string? original, [MaybeNullWhen(false)] out OrganizationNumber result)
    {
        if (s.Length != 9)
        {
            result = null;
            return false;
        }

        if (s.ContainsAnyExcept(NUMBERS))
        {
            result = null;
            return false;
        }

        result = new OrganizationNumber(original ?? new string(s));
        return true;
    }

    public bool Equals(OrganizationNumber? other)
        => ReferenceEquals(this, other) || (other is not null && _value == other._value);

    /// <inheritdoc/>
    public override string ToString()
        => _value;

    /// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
    public string ToString(string? format)
        => _value;

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => _value;

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (destination.Length < _value.Length)
        {
            charsWritten = 0;
            return false;
        }

        _value.AsSpan().CopyTo(destination);
        charsWritten = _value.Length;
        return true;
    }

    public static bool operator ==(OrganizationNumber? left, OrganizationNumber? right)
        => ReferenceEquals(left, right) || (left?.Equals(right) ?? right is null);

    public static bool operator !=(OrganizationNumber? left, OrganizationNumber? right)
        => !(left == right);

    private sealed class JsonConverter : JsonConverter<OrganizationNumber>
    {
        public override OrganizationNumber? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!TryParse(str, null, out var result))
            {
                throw new JsonException("Invalid organization number");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, OrganizationNumber value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value._value);
        }
    }
}
