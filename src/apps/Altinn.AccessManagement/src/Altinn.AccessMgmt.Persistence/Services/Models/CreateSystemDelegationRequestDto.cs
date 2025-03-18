namespace Altinn.AccessMgmt.Persistence.Services.Models;

/// <summary>
/// RequestDto to create Delegation and required assignments for System
/// </summary>
public class CreateSystemDelegationRequestDto
{
    /// <summary>
    /// Client party uuid
    /// </summary>
    public Guid ClientPartyId { get; set; }

    /// <summary>
    /// Client role (From -> Facilitator)
    /// e.g REGN/REVI
    /// </summary>
    public string ClientRole { get; set; } = string.Empty;

    /// <summary>
    /// Agent party uuid
    /// </summary>
    public Guid AgentPartyId { get; set; }

    /// <summary>
    /// Agent name (need to create new party)
    /// System displayName
    /// </summary>
    public string AgentName { get; set; }

    /// <summary>
    /// Agent role (Facilitator -> Agent)
    /// e.g Agent
    /// </summary>
    public string AgentRole { get; set; } = string.Empty;

    /// <summary>
    /// Packages to be delegated to Agent
    /// </summary>
    public string[] Packages { get; set; } = [];
}
