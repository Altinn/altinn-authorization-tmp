using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Npgsql.TypeMapping;

namespace Altinn.Authorization.Configuration.Postgres.Options;

/// <summary>
/// Configuration options for connecting to a PostgreSQL database.
/// This class supports authentication using either Managed Identity or Username/Password methods.
/// </summary>
public class AltinnPostgresOptions
{
    /// <summary>
    /// Initializes a new empty instance of the <see cref="AltinnPostgresOptions"/> class.
    /// must be present for <see cref="IOptions{AltinnPostgresOptions}"/> to work as it require
    /// an empty ctor
    /// </summary>
    public AltinnPostgresOptions() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnPostgresOptions"/> class.
    /// Configures the PostgreSQL options via a callback action.
    /// </summary>
    /// <param name="configureOptions">An action that configures the PostgreSQL options.</param>
    public AltinnPostgresOptions(Action<AltinnPostgresOptions> configureOptions) => configureOptions?.Invoke(this);

    /// <summary>
    /// Gets or sets the logger used for logging relevant information while migrating and configuring services.
    /// </summary>
    public ILogger Logger { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of prepared statements that can be cached for the connection.
    /// Default value is 50.
    /// </summary>
    public int MaxAutoPrepare { get; set; } = 50;

    /// <summary>
    /// Gets or sets the minimum number of usages before a prepared statement is automatically prepared.
    /// Default value is 2.
    /// </summary>
    public int AutoPrepareMinUsages { get; set; } = 2;

    /// <summary>
    /// Gets or sets the PostgreSQL server's host address.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Gets or sets the name of the PostgreSQL database to connect to.
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use Managed Identity for authentication.
    /// </summary>
    internal bool UseManagedIdentity { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to use Username/Password authentication.
    /// </summary>
    internal bool UseUsernameAndPassword { get; set; } = false;

    /// <summary>
    /// Gets or sets the username for PostgreSQL Username/Password authentication.
    /// </summary>
    internal string Username { get; set; }

    /// <summary>
    /// Gets or sets the password for PostgreSQL Username/Password authentication.
    /// </summary>
    internal string Password { get; set; }

    /// <summary>
    /// Gets or sets the token credential for Managed Identity authentication.
    /// </summary>
    internal TokenCredential TokenCredential { get; set; }

    /// <summary>
    /// Provides a simple API for configuring and Npg Data source
    /// </summary>
    internal Action<NpgsqlDataSourceBuilder> DataSourceBuilder { get; private set; } = (_) => { };

    /// <summary>
    /// Configures the options to use Managed Identity for PostgreSQL authentication.
    /// </summary>
    /// <param name="configureOptions">An optional action to further configure Managed Identity options.</param>
    /// <returns>The current instance of <see cref="AltinnPostgresOptions"/> for method chaining.</returns>
    public AltinnPostgresOptions ConfigureDataSource(Action<NpgsqlDataSourceBuilder> configureOptions = null)
    {
        if (configureOptions != null)
        {
            DataSourceBuilder = configureOptions;
        }

        return this;
    }

    /// <summary>
    /// Configures the options to use Managed Identity for PostgreSQL authentication.
    /// </summary>
    /// <param name="configureOptions">An optional action to further configure Managed Identity options.</param>
    /// <returns>The current instance of <see cref="AltinnPostgresOptions"/> for method chaining.</returns>
    public AltinnPostgresOptions ConfigureManagedIdentity(Action<ManagedIdentityOptions> configureOptions = null)
    {
        var cred = new ManagedIdentityOptions(configureOptions).TokenCredentials;
        UseManagedIdentity = true;
        TokenCredential = cred;
        return this;
    }

    /// <summary>
    /// Configures the options to use Username and Password for PostgreSQL authentication.
    /// </summary>
    /// <param name="configureOptions">An optional action to further configure Username/Password options.</param>
    /// <returns>The current instance of <see cref="AltinnPostgresOptions"/> for method chaining.</returns>
    public AltinnPostgresOptions ConfigureUsernameAndPassword(Action<UsernameAndPasswordOptions> configureOptions = null)
    {
        var cred = new UsernameAndPasswordOptions(configureOptions);
        UseUsernameAndPassword = true;
        Username = cred.Username;
        Password = cred.Password;
        return this;
    }

    /// <summary>
    /// Options for Username/Password-based authentication for PostgreSQL.
    /// </summary>
    public class UsernameAndPasswordOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsernameAndPasswordOptions"/> class.
        /// Configures Username/Password options via a callback action.
        /// </summary>
        /// <param name="configureOptions">An action that configures the Username/Password options.</param>
        public UsernameAndPasswordOptions(Action<UsernameAndPasswordOptions> configureOptions) => configureOptions?.Invoke(this);

        /// <summary>
        /// Gets or sets the username for PostgreSQL Username/Password authentication.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for PostgreSQL Username/Password authentication.
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Options for Managed Identity-based authentication for PostgreSQL.
    /// </summary>
    public class ManagedIdentityOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedIdentityOptions"/> class.
        /// Configures Managed Identity options via a callback action.
        /// </summary>
        /// <param name="configureOptions">An action that configures the Managed Identity options.</param>
        public ManagedIdentityOptions(Action<ManagedIdentityOptions> configureOptions) => configureOptions?.Invoke(this);

        /// <summary>
        /// Gets or sets the token credentials used for Managed Identity authentication.
        /// </summary>
        public TokenCredential TokenCredentials { get; set; }
    }
}
