using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Altinn.Authorization.ABAC.Utils;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Platform.Authorization.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Altinn.Platform.Authorization.IntegrationTests.MockServices
{
    public class ResourceRegistryMock : IResourceRegistry
    {
        public async Task<XacmlPolicy> GetResourcePolicyAsync(string resourceId)
        {
            if (File.Exists(Path.Combine(GetResourceRegistryPolicyPath(resourceId), "policy.xml")))
            {
                return await Task.FromResult(ParsePolicy("policy.xml", GetResourceRegistryPolicyPath(resourceId)));
            }
           
            return null;
        }

        private static string GetResourceRegistryPolicyPath(string resourceId)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(AltinnApps_DecisionTests).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "Xacml", "3.0", "ResourceRegistry", resourceId);
        }
        
        public static XacmlPolicy ParsePolicy(string policyDocumentTitle, string policyPath)
        {
            XmlDocument policyDocument = new XmlDocument();

            policyDocument.Load(Path.Combine(policyPath, policyDocumentTitle));
            XacmlPolicy policy;
            using (XmlReader reader = XmlReader.Create(new StringReader(policyDocument.OuterXml)))
            {
                policy = XacmlParser.ParseXacmlPolicy(reader);
            }

            return policy;
        }
    }
}
