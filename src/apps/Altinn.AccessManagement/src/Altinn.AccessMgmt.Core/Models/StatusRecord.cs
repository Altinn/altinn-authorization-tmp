﻿namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// Persistent status
/// </summary>
public class StatusRecord
{
    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name (e.g. AccessMgmt-Ingest-PartyFeed)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Current state
    /// </summary>
    public string State { get; set; }

    /// <summary>
    /// Latest message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Payload data (e.g. JsonSerialized item)
    /// </summary>
    public string Payload { get; set; }

    /// <summary>
    /// A defineable limit (e.g. retryLimit)
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// A counter (e.g. retryAttempts)
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}
