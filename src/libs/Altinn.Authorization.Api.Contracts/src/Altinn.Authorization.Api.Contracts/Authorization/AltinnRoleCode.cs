#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Swashbuckle.Examples;

namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// A string value identifier of an Altinn Role Code. E.g. urn:altinn:role:privatperson or urn:altinn:role:selvregistrert
/// </summary>
[JsonConverter(typeof(JsonConverter))]
[ExcludeFromCodeCoverage]
public class AltinnRoleCode
    : ISpanParsable<AltinnRoleCode>,
    ISpanFormattable,
    IExampleDataProvider<AltinnRoleCode>
{
    private readonly string _value;

    private AltinnRoleCode(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnRoleCode"/> from the specified value without validation.
    /// </summary>
    /// <param name="value">The package identifier.</param>
    /// <returns>A <see cref="AltinnRoleCode"/>.</returns>
    public static AltinnRoleCode CreateUnchecked(string value)
        => new(value);

    /// <inheritdoc/>
    public static IEnumerable<AltinnRoleCode>? GetExamples(ExampleDataOptions options)
    {
        yield return new AltinnRoleCode("privatperson");
        yield return new AltinnRoleCode("selvregistrert");
    }

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
    public static AltinnRoleCode Parse(string s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static AltinnRoleCode Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid AltinnRoleCode");

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
    public static AltinnRoleCode Parse(ReadOnlySpan<char> s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static AltinnRoleCode Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid AltinnRoleCode");

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AltinnRoleCode result)
        => TryParse(s.AsSpan(), s, out result);

    /// <inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out AltinnRoleCode result)
        => TryParse(s, original: null, out result);

    private static bool TryParse(ReadOnlySpan<char> s, string? original, [MaybeNullWhen(false)] out AltinnRoleCode result)
    {
        result = new AltinnRoleCode(original ?? new string(s));
        return true;
    }

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

    private sealed class JsonConverter : JsonConverter<AltinnRoleCode>
    {
        public override AltinnRoleCode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!TryParse(str, null, out var result))
            {
                throw new JsonException("Invalid AltinnRoleCode");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, AltinnRoleCode value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value._value);
        }
    }
}
