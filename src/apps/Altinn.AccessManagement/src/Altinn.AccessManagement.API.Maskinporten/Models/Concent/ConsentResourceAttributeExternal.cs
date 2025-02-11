namespace Altinn.AccessManagement.Api.Maskinporten.Models.Concent
{
    /// <summary>
    /// A resurce attribute identifying part or whole resource
    /// </summary>
    public class ConsentResourceAttributeExternal
    {

        public string Id { get; set; }

        public string Value { get; set; }

        public List<ConsentRightExternal> ConcentRights { get; set; }
    }
}
