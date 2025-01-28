namespace Altinn.Authorization.Integration.Register.Extensions;

/// <summary>
/// AltinnRegisterOptions
/// </summary>
public class AltinnRegisterOptions
{
    /// <summary>
    /// ctor
    /// </summary>
    public AltinnRegisterOptions()
    {
    }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="configureOptions">configure altinn register</param>
    public AltinnRegisterOptions(Action<AltinnRegisterOptions> configureOptions)
    {
        configureOptions(this);
    }

    public string Endpoint { get; set; }
}
