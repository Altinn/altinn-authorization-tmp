namespace Altinn.Platform.Authorization.Constants
{
    /// <summary>
    /// Altinn specific XACML constants used for urn identifiers and attributes
    /// </summary>
    public static class AltinnXacmlConstants
    {
        /// <summary>
        /// Altinn specific prefixes
        /// </summary>
        public static class Prefixes
        {
            /// <summary>
            /// The Policy Id prefix.
            /// </summary>
            public const string PolicyId = "urn:altinn:policyid:";

            /// <summary>
            /// The Obligation Id prefix.
            /// </summary>
            public const string ObligationId = "urn:altinn:obligationid:";

            /// <summary>
            /// The Obligation Assignment Id prefix.
            /// </summary>
            public const string ObligationAssignmentid = "urn:altinn:obligation-assignmentid:";
        }

        /// <summary>
        /// Match attribute identifiers
        /// </summary>
        public static class MatchAttributeIdentifiers
        {
            /// <summary>
            /// Org attribute match indentifier 
            /// </summary>
            public const string OrgAttribute = "urn:altinn:org";

            /// <summary>
            /// Org number attribute match indentifier 
            /// </summary>
            public const string OrgNumberAttribute = "urn:altinn:organizationnumber";

            /// <summary>
            /// App attribute match indentifier 
            /// </summary>
            public const string AppAttribute = "urn:altinn:app";

            /// <summary>
            /// Resource registry match identifer
            /// </summary>
            public const string ResourceRegistry = "urn:altinn:resource";

            /// <summary>
            /// Instance attribute match indentifier 
            /// </summary>
            public const string InstanceAttribute = "urn:altinn:instance-id";

            /// <summary>
            /// App resource attribute match indentifier 
            /// </summary>
            public const string AppResourceAttribute = "urn:altinn:appresource";

            /// <summary>
            /// Task attribute match indentifier 
            /// </summary>
            public const string TaskAttribute = "urn:altinn:task";

            /// <summary>
            /// End-event attribute match indentifier 
            /// </summary>
            public const string EndEventAttribute = "urn:altinn:end-event";

            /// <summary>
            /// Party Id attribute match indentifier 
            /// </summary>
            public const string PartyAttribute = "urn:altinn:partyid";

            /// <summary>
            /// User Id attribute match indentifier 
            /// </summary>>
            public const string UserAttribute = "urn:altinn:userid";

            /// <summary>
            /// Role Code attribute match indentifier 
            /// </summary>
            public const string RoleAttribute = "urn:altinn:rolecode";

            /// <summary>
            /// Digitalt D�dsbo Role Code Attribute match identifier
            /// </summary>
            public const string OedRoleAttribute = "urn:digitaltdodsbo:rolecode";

            /// <summary>
            /// AccessPackage Attribute match identifier
            /// </summary>
            public const string AccessPackageAttribute = "urn:altinn:accesspackage";

            /// <summary>
            /// SessionId Attribute match identifier
            /// </summary>
            public const string SessionIdAttribute = "urn:altinn:sessionid";

            /// <summary>
            /// SystemUserId Attribute match identifier
            /// </summary>
            public const string SystemUserIdAttribute = "urn:altinn:systemuser:uuid";
        }

        /// <summary>
        /// Attribute categories.
        /// </summary>
        public static class MatchAttributeCategory
        {
            /// <summary>
            /// The minimum authentication level category.
            /// </summary>
            public const string MinimumAuthenticationLevel = "urn:altinn:minimum-authenticationlevel";

            /// <summary>
            /// The minimum authentication level for organization category
            /// </summary>
            public const string MinimumAuthenticationLevelOrg = "urn:altinn:minimum-authenticationlevel-org";
        }
    }
}
