namespace Altinn.AccessManagement.Core.Models.IdPortenAuthorization
{
    /// <summary>
    /// Model representing an ID-porten authorization
    /// </summary>
    public class IdPortenAuthorization
    {
        /// <summary>
        /// Id of the authorization
        /// </summary>
        public string Authorization_id { get; set; }

        /// <summary>
        /// Client id of the authorization
        /// </summary>
        public required string Client_id { get; set; }

        /// <summary>
        /// Client name of the authorization
        /// </summary>
        public string Client_name { get; set; }

        /// <summary>
        /// Long client description of the authorization
        /// </summary>
        public string Client_description { get; set; }

        /// <summary>
        /// Timestamp of when the authorization was granted
        /// </summary>
        public long Authorized_at { get; set; }

        /// <summary>
        /// Timestamp of when the authorization expires
        /// </summary>
        public long Expires { get; set; }

        /// <summary>
        /// Scopes granted for the authorization
        /// </summary>
        public IEnumerable<Scope> Scopes { get; set; }

        /// <summary>
        /// User agent string of the device that granted the authorization
        /// </summary>
        public string User_agent { get; set; }

        /// <summary>
        /// Consumer organization
        /// </summary>
        public Consumer Consumer { get; set; }
    }

    /// <summary>
    /// Model for consumer organization
    /// </summary>
    public class Consumer
    {
        /// <summary>
        /// Authority, iso6523-actorid-upis
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Organization number and prefix 0192:
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Organization number of the consumer organization
        /// </summary>
        public string OrgNo { get; set; }
    }

    /// <summary>
    /// Model for scope of the authorization
    /// </summary>
    public class Scope
    {
        /// <summary>
        /// Technical scope name, e.g. "altinn:serviceowner/instances.read"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description (title) of the scope
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Long description of the scope
        /// </summary>
        public string Long_description { get; set; }

        /// <summary>
        /// Wether the scope requires user consent or not
        /// </summary>
        public bool Requires_user_consent { get; set; }

        /// <summary>
        /// Organization number of the owner of the scope
        /// </summary>
        public string Owner_orgno { get; set; }
    }
}
