using System.Threading.Tasks;
using Altinn.Authorization.ABAC.Xacml;

namespace Altinn.Platform.Authorization.Services.Interface
{
    /// <summary>
    /// Interface for resource registry
    /// </summary>
    public interface IResourceRegistry
    {
        /// <summary>
        /// Returns a policy based on the resourceId
        /// </summary>
        /// <param name="resourceId">the policyid</param>
        /// <returns>XacmlPolicy</returns>
        Task<XacmlPolicy> GetResourcePolicyAsync(string resourceId);
    }
}
