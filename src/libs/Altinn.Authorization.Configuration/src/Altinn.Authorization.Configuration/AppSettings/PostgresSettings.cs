using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.Configuration.AppSettings;

/// <summary>
/// Configuration settings for connecting to a PostgreSQL database.
/// This class holds the necessary information required to establish a connection to a PostgreSQL instance.
/// </summary>
[ExcludeFromCodeCoverage]
public class PostgresSettings
{
    /// <summary>
    /// The hostname or IP address of the PostgreSQL server.
    /// This is the address used to connect to the database instance.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// The name of the specific database to connect to within the PostgreSQL server.
    /// This specifies which database the connection will target.
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// The username used for authentication when connecting to the PostgreSQL database.
    /// This username must have the appropriate permissions to access the specified database.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// The password associated with the username for authenticating to the PostgreSQL database.
    /// This is used to secure the connection to the database instance.
    /// </summary>
    public string Password { get; set; }
}
