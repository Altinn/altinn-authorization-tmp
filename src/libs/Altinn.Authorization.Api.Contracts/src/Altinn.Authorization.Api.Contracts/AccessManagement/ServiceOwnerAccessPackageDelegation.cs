using System.ComponentModel.DataAnnotations;
using Altinn.Authorization.Api.Contracts.Consent;

namespace Altinn.Authorization.Api.Contracts.AccessManagement
{
    /// <summary>
    /// Defines  package delegation between two 
    /// </summary>
    public class ServiceOwnerAccessPackageDelegation
    {
        [Required]
        public ServiceOwnerConnectionPartyUrn From { get; set; }

        [Required]
        public ServiceOwnerConnectionPartyUrn To { get; set; }

        [Required]
        public AccessPackageUrn PackageUrn { get; set; }
    }
}
