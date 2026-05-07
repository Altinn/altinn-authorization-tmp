namespace Altinn.AccessManagement.Core.Constants
{
    /// <summary>
    /// Attribute representations in XACML
    /// </summary>
    public static class XacmlRequestAttribute
    {
        /// <summary>
        /// xacml string that represents org
        /// </summary>
        public const string OrgAttribute = "urn:altinn:org";

        /// <summary>
        /// xacml string that represents app
        /// </summary>
        public const string AppAttribute = "urn:altinn:app";

        /// <summary>
        /// xacml string that represents instanceid
        /// </summary>
        public const string InstanceAttribute = "urn:altinn:instance-id";

        /// <summary>
        /// xacm string that represents appresource
        /// </summary>
        public const string AppResourceAttribute = "urn:altinn:appresource";

        /// <summary>
        /// xacml string that represents task
        /// </summary>
        public const string TaskAttribute = "urn:altinn:task";

        /// <summary>
        /// xacml string that represents end event
        /// </summary>
        public const string EndEventAttribute = "urn:altinn:end-event";

        /// <summary>
        /// xacml string that represents party using legacy party id
        /// </summary>
        public const string PartyAttribute = "urn:altinn:partyid";

        /// <summary>
        /// xacml string that represents party using uuid
        /// </summary>
        public const string PartyUuidAttribute = "urn:altinn:party:uuid";

        /// <summary>
        /// xacml string that represents organization number 
        /// </summary>
        public const string OrganizationIdentifierAttribute = "urn:altinn:organization:identifier-no";

        /// <summary>
        /// xacml string that represents person number 
        /// </summary>
        public const string PersonIdentifierAttribute = "urn:altinn:person:identifier-no";

        /// <summary>
        /// xacml string that represents user
        /// </summary>
        public const string UserAttribute = "urn:altinn:userid";

        /// <summary>
        /// xacml string that represents role
        /// </summary>
        public const string RoleAttribute = "urn:altinn:rolecode";

        /// <summary>
        /// xacml string that represents resource
        /// </summary>
        public const string ResourceRegistryAttribute = "urn:altinn:resource";

        /// <summary>
        /// xacml string that represents system user
        /// </summary>
        public const string SystemUserAttribute = "urn:altinn:systemuser:uuid";
    }
}
