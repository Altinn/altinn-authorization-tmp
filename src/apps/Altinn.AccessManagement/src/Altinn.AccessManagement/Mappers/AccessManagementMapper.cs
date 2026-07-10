using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.AccessManagement.Models;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Urn.Json;
using Riok.Mapperly.Abstractions;

namespace Altinn.AccessManagement.Mappers;

/// <summary>
/// Maps between the internal rights/delegation models and the external API models used by the AccessManagement controllers.
/// </summary>
[Mapper]
public static partial class AccessManagementMapper
{
    /// <summary>
    /// Maps a rights delegation check request to its internal representation. The From attribute is set separately by the caller from the authenticated reportee.
    /// </summary>
    [MapperIgnoreTarget(nameof(RightsDelegationCheckRequest.From))]
    public static partial RightsDelegationCheckRequest ToInternal(this RightsDelegationCheckRequestExternal request);

    /// <summary>
    /// Maps a list of rights delegation check results to their external representation.
    /// </summary>
    public static partial List<RightDelegationCheckResultExternal> ToExternal(this List<RightDelegationCheckResult> results);

    [MapProperty(nameof(RightDelegationCheckResult.Action) + "." + nameof(AttributeMatch.Value), nameof(RightDelegationCheckResultExternal.Action))]
    private static partial RightDelegationCheckResultExternal ToExternal(this RightDelegationCheckResult result);

    /// <summary>
    /// Maps a rights delegation request to a delegation lookup. The From attribute is set separately by the caller from the authenticated reportee.
    /// </summary>
    [MapperIgnoreTarget(nameof(DelegationLookup.From))]
    public static partial DelegationLookup ToInternal(this RightsDelegationRequestExternal request);

    /// <summary>
    /// Maps a request to revoke an offered delegation to a delegation lookup. The From attribute is set separately by the caller from the authenticated reportee.
    /// </summary>
    [MapperIgnoreTarget(nameof(DelegationLookup.From))]
    public static partial DelegationLookup ToInternal(this RevokeOfferedDelegationExternal request);

    /// <summary>
    /// Maps a request to revoke a received delegation to a delegation lookup. The To attribute is set separately by the caller from the authenticated reportee.
    /// </summary>
    [MapperIgnoreTarget(nameof(DelegationLookup.To))]
    public static partial DelegationLookup ToInternal(this RevokeReceivedDelegationExternal request);

    /// <summary>
    /// Maps a right specified by resource and action to its internal representation. The action is only identified by its attribute value on the
    /// external model, so the internal <see cref="AttributeMatch"/> is reconstructed using the well-known XACML action attribute id.
    /// </summary>
    private static Right ToInternal(this BaseRightExternal right) => new()
    {
        Resource = right.Resource.ToInternal(),
        Action = new AttributeMatch { Id = XacmlConstants.MatchAttributeIdentifiers.ActionId, Value = right.Action }
    };

    private static partial List<AttributeMatch> ToInternal(this List<AttributeMatchExternal> matches);

    /// <summary>
    /// Maps the result of a rights delegation to its external representation.
    /// </summary>
    [MapProperty(nameof(DelegationActionResult.Rights), nameof(RightsDelegationResponseExternal.RightDelegationResults))]
    public static partial RightsDelegationResponseExternal ToExternal(this DelegationActionResult result);

    [MapProperty(nameof(RightDelegationResult.Action) + "." + nameof(AttributeMatch.Value), nameof(RightDelegationResultExternal.Action))]
    private static partial RightDelegationResultExternal ToExternal(this RightDelegationResult result);

    /// <summary>
    /// Maps rights delegated to or from Altinn 2 to their external representation.
    /// </summary>
    public static partial IEnumerable<RightDelegationExternal> ToExternal(this IEnumerable<RightDelegation> delegations);

    /// <summary>
    /// Maps an attribute used to identify the recipient to clear access caching for. The Urn is only present on the internal model and stays unset here.
    /// </summary>
    [MapperIgnoreTarget(nameof(BaseAttribute.Urn))]
    public static partial BaseAttribute ToInternal(this BaseAttributeExternal attribute);

    /// <summary>
    /// Maps a list of delegation changes to their external representation.
    /// </summary>
    public static partial List<DelegationChangeExternal> ToExternal(this List<DelegationChange> changes);

    /// <summary>
    /// Maps a request to delegate access to an app instance to its internal representation. ResourceId, InstanceId, PerformedBy,
    /// InstanceDelegationMode and InstanceDelegationSource are not present on the request body and are set separately by the caller.
    /// </summary>
    [MapperIgnoreTarget(nameof(AppsInstanceDelegationRequest.ResourceId))]
    [MapperIgnoreTarget(nameof(AppsInstanceDelegationRequest.InstanceId))]
    [MapperIgnoreTarget(nameof(AppsInstanceDelegationRequest.PerformedBy))]
    [MapperIgnoreTarget(nameof(AppsInstanceDelegationRequest.InstanceDelegationMode))]
    [MapperIgnoreTarget(nameof(AppsInstanceDelegationRequest.InstanceDelegationSource))]
    [MapProperty(nameof(AppsInstanceDelegationRequestDto.From) + "." + nameof(UrnJsonTypeValue<PartyUrn>.Value), nameof(AppsInstanceDelegationRequest.From))]
    [MapProperty(nameof(AppsInstanceDelegationRequestDto.To) + "." + nameof(UrnJsonTypeValue<PartyUrn>.Value), nameof(AppsInstanceDelegationRequest.To))]
    public static partial AppsInstanceDelegationRequest ToInternal(this AppsInstanceDelegationRequestDto dto);

    [MapProperty(nameof(RightDto.Action) + "." + nameof(UrnJsonTypeValue<ActionUrn>.Value), nameof(RightInternal.Action))]
    private static partial RightInternal ToInternal(this RightDto right);

    /// <summary>
    /// Maps the result of an app instance delegation to its external representation.
    /// </summary>
    public static partial AppsInstanceDelegationResponseDto ToDto(this AppsInstanceDelegationResponse response);

    /// <summary>
    /// Maps a list of app instance delegation results to their external representation.
    /// </summary>
    public static partial List<AppsInstanceDelegationResponseDto> ToDto(this List<AppsInstanceDelegationResponse> responses);

    private static partial RightDelegationResultDto ToDto(this InstanceRightDelegationResult result);

    /// <summary>
    /// Maps the result of an app instance revoke to its external representation.
    /// </summary>
    public static partial AppsInstanceRevokeResponseDto ToDto(this AppsInstanceRevokeResponse response);

    /// <summary>
    /// Maps a list of app instance revoke results to their external representation.
    /// </summary>
    public static partial List<AppsInstanceRevokeResponseDto> ToDto(this List<AppsInstanceRevokeResponse> responses);

    private static partial RightRevokeResultDto ToDto(this InstanceRightRevokeResult result);

    /// <summary>
    /// Maps a list of resource right delegation check results to their external representation.
    /// </summary>
    public static partial IEnumerable<ResourceRightDelegationCheckResultDto> ToDto(this List<ResourceRightDelegationCheckResult> results);
}
