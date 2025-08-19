using Altinn.AccessManagement.Core.Errors; //Problems.cs
using Altinn.AccessMgmt.Persistence.Core.Models; //ChangeRequestOption.cs
using Altinn.AccessMgmt.Persistence.Services.Models; //PartyBaseInternal.cs
using Altinn.Authorization.Api.Contracts.Party; //AddPartyResultDto.cs
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Interface for party management services
/// </summary>
public interface IPartyService
{
    /// <summary>
    /// Adds a party to the system if it does not already exist
    /// </summary>
    /// <param name="party">The party to add</param>
    /// <param name="options">Change request options</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>
    /// A <see cref="Result{AddPartyResultDto}"/> indicating the outcome of the operation.
    /// </returns>
    Task<Result<AddPartyResultDto>> AddParty(PartyBaseInternal party, ChangeRequestOptions options, CancellationToken cancellationToken = default);
}
