namespace Altinn.Authorization.Api.Contracts.AccessManagement
{
    public class ResourceOwnerAccessPackageDelegation
    {
        public string From { get; set; }

        public string To { get; set; }

        public List<string> PackageUrns { get; set; }
    }
}
