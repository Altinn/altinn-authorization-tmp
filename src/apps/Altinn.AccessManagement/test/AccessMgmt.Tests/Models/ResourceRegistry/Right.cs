using Altinn.AccessManagement.Core.Models;

namespace AccessMgmt.Tests.Models.ResourceRegistry
{
    /// <summary>
    /// Copy from Resource Registry Right model, but with AccessorUrns instead of HasPermit and CanDelegate, as this is the model used in the ResourceRegistryClientMock
    /// </summary>
    public class Right
    {
        /// <summary>
        /// Unique key for action
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The name of the right to be presented in frontend, this is not used for any processing but only for display purposes. The name can be derived from the action part of the right key or can be a more user-friendly name associated with the right.
        /// </summary>
        public HashSet<string> AccessorUrns { get; set; }

        /// <summary>
        /// Name of the action to present in frontend
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Concatenated key for subresources from policy rule
        /// </summary>
        public IEnumerable<PolicyAttributeMatch> Resource { get; set; }

        /// <summary>
        /// Action
        /// </summary>
        public PolicyAttributeMatch Action { get; set; }
    }
}
