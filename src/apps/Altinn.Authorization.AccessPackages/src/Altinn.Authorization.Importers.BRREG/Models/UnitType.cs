﻿using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// UnitType
/// </summary>
public class UnitType
{
    /// <summary>
    /// Code
    /// </summary>
    [JsonPropertyName("kode")]
    public string Code { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [JsonPropertyName("beskrivelse")]
    public string Description { get; set; }
}