namespace Altinn.Authorization.Api.Contracts.AccessManagement;

public class RelationDto
{
    /// <summary>
    /// Delegation id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The From assignments From (e.g. Bakerhuset Johnsen)
    /// </summary>
    public CompactEntityDto From { get; set; }

    /// <summary>
    /// The role connection From:From and To:Via (e.g. Regnskapsfører)
    /// </summary>
    public RoleDto FromRole { get; set; }

    /// <summary>
    /// The To assignments To (e.g. Kjetil Nordmann)
    /// </summary>
    public CompactEntityDto To { get; set; }

    /// <summary>
    /// The role connecting From:Via To:To (e.g. Agent)
    /// </summary>
    public RoleDto ToRole { get; set; }

    /// <summary>
    /// The party facilitating the delegation (e.g. Det Norske Regnskapsfirma)
    /// </summary>
    public CompactEntityDto Via { get; set; }

    /// <summary>
    /// Packages delegated
    /// </summary>
    public List<PackageDto> Packages { get; set; }

    /// <summary>
    /// Resources delegated
    /// </summary>
    public List<ResourceDto> Resources { get; set; }
}
