#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Swashbuckle.Examples;

namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// A string value for a legacy role code. E.g. urn:altinn:rolecode:priv or urn:altinn:rolecode:seln or urn:altinn:rolecode:dagl
/// </summary>
[JsonConverter(typeof(JsonConverter))]
[ExcludeFromCodeCoverage]
public class LegacyRoleCode
    : ISpanParsable<LegacyRoleCode>,
    ISpanFormattable,
    IExampleDataProvider<LegacyRoleCode>
{
    private readonly string _value;

    private LegacyRoleCode(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Creates a new <see cref="LegacyRoleCode"/> from the specified value without validation.
    /// </summary>
    /// <param name="value">The role code.</param>
    /// <returns>A <see cref="LegacyRoleCode"/>.</returns>
    public static LegacyRoleCode CreateUnchecked(string value)
        => new(value);

    /// <inheritdoc/>
    public static IEnumerable<LegacyRoleCode>? GetExamples(ExampleDataOptions options)
    {
        yield return new LegacyRoleCode("dagl");
        yield return new LegacyRoleCode("priv");
    }

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
    public static LegacyRoleCode Parse(string s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static LegacyRoleCode Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid RoleCode");

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
    public static LegacyRoleCode Parse(ReadOnlySpan<char> s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static LegacyRoleCode Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid RoleCode");

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out LegacyRoleCode result)
        => TryParse(s.AsSpan(), s, out result);

    /// <inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out LegacyRoleCode result)
        => TryParse(s, original: null, out result);

    private static bool TryParse(ReadOnlySpan<char> s, string? original, [MaybeNullWhen(false)] out LegacyRoleCode result)
    {
        result = new LegacyRoleCode(original ?? new string(s));
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

    private sealed class JsonConverter : JsonConverter<LegacyRoleCode>
    {
        public override LegacyRoleCode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!TryParse(str, null, out var result))
            {
                throw new JsonException("Invalid RoleCode");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, LegacyRoleCode value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value._value);
        }
    }
}
