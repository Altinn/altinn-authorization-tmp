using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Authorization.Integration.Platform.Appsettings
{
    /// <summary>
    /// Represents the platform-related configuration settings for Altinn authorization.
    /// This class provides endpoints for interacting with key platform services.
    /// </summary>
    public class SblBridgeSettings
    {
        /// <summary>
        /// Gets or sets the endpoint URI for the SblBridge service.
        /// This service provides data from Altinn 2.
        /// </summary>
        /// <remarks>
        /// The endpoint should be a valid URI, typically pointing to an API service.
        /// Example: <c>https://bridge.altinn.no</c>
        /// </remarks>
        public Uri BaseApiUrl { get; set; }
    }
}
