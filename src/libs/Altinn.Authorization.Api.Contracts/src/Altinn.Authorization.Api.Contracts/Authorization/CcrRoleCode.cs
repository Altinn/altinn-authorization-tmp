#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Swashbuckle.Examples;

namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// A string value identifier of a CCR (enhetsregisteret) role. E.g. urn:altinn:external-role:ccr:daglig-leder
/// </summary>
[JsonConverter(typeof(JsonConverter))]
[ExcludeFromCodeCoverage]
public class CcrRoleCode
    : ISpanParsable<CcrRoleCode>,
    ISpanFormattable,
    IExampleDataProvider<CcrRoleCode>
{
    private readonly string _value;

    private CcrRoleCode(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Creates a new <see cref="CcrRoleCode"/> from the specified value without validation.
    /// </summary>
    /// <param name="value">The package identifier.</param>
    /// <returns>A <see cref="CcrRoleCode"/>.</returns>
    public static CcrRoleCode CreateUnchecked(string value)
        => new(value);

    /// <inheritdoc/>
    public static IEnumerable<CcrRoleCode>? GetExamples(ExampleDataOptions options)
    {
        yield return new CcrRoleCode("daglig-leder");
    }

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
    public static CcrRoleCode Parse(string s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static CcrRoleCode Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid CcrRoleCode");

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
    public static CcrRoleCode Parse(ReadOnlySpan<char> s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static CcrRoleCode Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid CcrRoleCode");

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out CcrRoleCode result)
        => TryParse(s.AsSpan(), s, out result);

    /// <inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out CcrRoleCode result)
        => TryParse(s, original: null, out result);

    private static bool TryParse(ReadOnlySpan<char> s, string? original, [MaybeNullWhen(false)] out CcrRoleCode result)
    {
        result = new CcrRoleCode(original ?? new string(s));
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

    private sealed class JsonConverter : JsonConverter<CcrRoleCode>
    {
        public override CcrRoleCode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!TryParse(str, null, out var result))
            {
                throw new JsonException("Invalid CcrRoleCode");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, CcrRoleCode value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value._value);
        }
    }
}
