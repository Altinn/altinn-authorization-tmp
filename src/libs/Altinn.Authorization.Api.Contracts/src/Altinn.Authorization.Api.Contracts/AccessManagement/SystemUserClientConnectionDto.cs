using static Altinn.Authorization.Api.Contracts.AccessManagement.AccessPackageDto.Check;

namespace Altinn.AccessMgmt.Persistence.Services.Models;

/// <summary>
/// Client connection model for system users
/// </summary>
public class SystemUserClientConnectionDto
{
    /// <summary>
    /// Identity of the client delegation
    /// </summary>
    public Guid Id { get; set; }

    public ClientDelegation Delegation { get; set; }

    public Client From { get; set; }

    public AgentRole Role { get; set; }

    public Agent To { get; set; }

    public ServiceProvider Facilitator { get; set; }

    public ServiceProviderRole FacilitatorRole { get; set; }

    /// <summary>
    /// Delegation of the client connection
    /// </summary>
    public class ClientDelegation
    {
        /// <summary>
        /// The id of the delegation
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The client assignment the delegation is tied to
        /// </summary>
        public Guid FromId { get; set; }

        /// <summary>
        /// The agent assignment the delegation is tied to
        /// </summary>
        public Guid ToId { get; set; }

        public Guid FacilitatorId { get; set; }
    }

    /// <summary>
    /// The client entity the client connection
    /// </summary>
    public class Client
    {
        public Guid Id { get; set; }

        public Guid TypeId { get; set; }

        public Guid VariantId { get; set; }

        public string Name { get; set; }

        public string RefId { get; set; }

        public Guid? ParentId { get; set; }
    }

    /// <summary>
    /// The agent role tied to the client connection
    /// </summary>
    public class AgentRole
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public bool IsKeyRole { get; set; }

        public string Urn { get; set; }

        public string LegacyRoleCode { get; set; }

        public string LegacyUrn { get; set; }

        public string Provider { get; set; }
    }

    /// <summary>
    /// The agent entity of the client connection
    /// </summary>
    public class Agent
    {
        public Guid Id { get; set; }

        public Guid TypeId { get; set; }

        public Guid VariantId { get; set; }

        public string Name { get; set; }

        public string RefId { get; set; }

        public Guid? ParentId { get; set; }
    }

    /// <summary>
    /// The service provider entity the client connection is related to
    /// </summary>
    public class ServiceProvider
    {
        public Guid Id { get; set; }

        public Guid TypeId { get; set; }

        public Guid VariantId { get; set; }

        public string Name { get; set; }

        public string RefId { get; set; }

        public Guid? ParentId { get; set; }
    }

    /// <summary>
    /// The role the service provider has for the client 
    /// </summary>
    public class ServiceProviderRole
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public bool IsKeyRole { get; set; }

        public string Urn { get; set; }

        public string LegacyRoleCode { get; set; }

        public string LegacyUrn { get; set; }

        public string Provider { get; set; }
    }
}
