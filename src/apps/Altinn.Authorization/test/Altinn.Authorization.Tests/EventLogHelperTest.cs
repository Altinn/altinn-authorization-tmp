using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Platform.Authorization.Constants;
using Altinn.Platform.Authorization.Helpers;

namespace Altinn.Platform.Authorization.Tests;

public class EventLogHelperTest
{
    private static XacmlContextRequest CreateContextRequest(
        IEnumerable<(string category, string attributeId, string value)> attributes)
    {
        var grouped = attributes.GroupBy(a => a.category);
        var attrList = new List<XacmlContextAttributes>();
        foreach (var group in grouped)
        {
            var contextAttributes = new XacmlContextAttributes(new Uri(group.Key));
            foreach (var (_, attributeId, value) in group)
            {
                var attr = new XacmlAttribute(new Uri(attributeId), false);
                attr.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), value));
                contextAttributes.Attributes.Add(attr);
            }

            attrList.Add(contextAttributes);
        }

        return new XacmlContextRequest(false, false, attrList);
    }

    // --- GetResourceAttributes ---

    [Fact]
    public void GetResourceAttributes_WithResourceRegistryAttribute_ReturnsResource()
    {
        var request = CreateContextRequest(
        [
            (XacmlConstants.MatchAttributeCategory.Resource, XacmlRequestAttribute.ResourceRegistryAttribute, "nav_sykepenger"),
            (XacmlConstants.MatchAttributeCategory.Resource, XacmlRequestAttribute.InstanceAttribute, "1234/5678"),
            (XacmlConstants.MatchAttributeCategory.Resource, XacmlRequestAttribute.PartyAttribute, "50001337"),
        ]);

        var (resource, instanceId, resourcePartyId) = EventLogHelper.GetResourceAttributes(request);

        Assert.Equal("nav_sykepenger", resource);
        Assert.Equal("1234/5678", instanceId);
        Assert.Equal(50001337, resourcePartyId);
    }

    [Fact]
    public void GetResourceAttributes_WithOrgApp_ReturnsCompositeResource()
    {
        var request = CreateContextRequest(
        [
            (XacmlConstants.MatchAttributeCategory.Resource, XacmlRequestAttribute.OrgAttribute, "ttd"),
            (XacmlConstants.MatchAttributeCategory.Resource, XacmlRequestAttribute.AppAttribute, "testapp"),
        ]);

        var (resource, instanceId, resourcePartyId) = EventLogHelper.GetResourceAttributes(request);

        Assert.Equal("app_ttd_testapp", resource);
        Assert.Equal(string.Empty, instanceId);
        Assert.Null(resourcePartyId);
    }

    [Fact]
    public void GetResourceAttributes_NullRequest_ReturnsDefaults()
    {
        var (resource, instanceId, resourcePartyId) = EventLogHelper.GetResourceAttributes(null);

        Assert.Equal("app__", resource);
        Assert.Equal(string.Empty, instanceId);
        Assert.Null(resourcePartyId);
    }

    // --- GetSubjectInformation ---

    [Fact]
    public void GetSubjectInformation_WithAllAttributes_ReturnsAll()
    {
        var request = CreateContextRequest(
        [
            (XacmlConstants.MatchAttributeCategory.Subject, XacmlRequestAttribute.UserAttribute, "1001"),
            (XacmlConstants.MatchAttributeCategory.Subject, XacmlRequestAttribute.PartyAttribute, "50001"),
            (XacmlConstants.MatchAttributeCategory.Subject, XacmlRequestAttribute.OrgAttribute, "ttd"),
            (XacmlConstants.MatchAttributeCategory.Subject, XacmlRequestAttribute.LegacyOrganizationNumberAttribute, "987654321"),
            (XacmlConstants.MatchAttributeCategory.Subject, XacmlRequestAttribute.SessionIdAttribute, "sess-123"),
            (XacmlConstants.MatchAttributeCategory.Subject, XacmlRequestAttribute.PartyUuidAttribute, "abc-uuid"),
        ]);

        var (userId, partyId, org, orgNumber, sessionId, partyUuid) = EventLogHelper.GetSubjectInformation(request);

        Assert.Equal(1001, userId);
        Assert.Equal(50001, partyId);
        Assert.Equal("ttd", org);
        Assert.Equal(987654321, orgNumber);
        Assert.Equal("sess-123", sessionId);
        Assert.Equal("abc-uuid", partyUuid);
    }

    [Fact]
    public void GetSubjectInformation_NullRequest_ReturnsDefaults()
    {
        var (userId, partyId, org, orgNumber, sessionId, partyUuid) = EventLogHelper.GetSubjectInformation(null);

        Assert.Null(userId);
        Assert.Null(partyId);
        Assert.Equal(string.Empty, org);
        Assert.Null(orgNumber);
        Assert.Null(sessionId);
        Assert.Null(partyUuid);
    }

    [Fact]
    public void GetSubjectInformation_SystemUserIdAttribute_SetsPartyUuid()
    {
        var request = CreateContextRequest(
        [
            (XacmlConstants.MatchAttributeCategory.Subject, XacmlRequestAttribute.SystemUserIdAttribute, "sys-user-uuid"),
        ]);

        var (_, _, _, _, _, partyUuid) = EventLogHelper.GetSubjectInformation(request);

        Assert.Equal("sys-user-uuid", partyUuid);
    }

    // --- GetActionInformation ---

    [Fact]
    public void GetActionInformation_WithAction_ReturnsActionId()
    {
        var request = CreateContextRequest(
        [
            (XacmlConstants.MatchAttributeCategory.Action, XacmlConstants.MatchAttributeIdentifiers.ActionId, "read"),
        ]);

        string actionId = EventLogHelper.GetActionInformation(request);

        Assert.Equal("read", actionId);
    }

    [Fact]
    public void GetActionInformation_NullRequest_ReturnsEmpty()
    {
        string actionId = EventLogHelper.GetActionInformation(null);
        Assert.Equal(string.Empty, actionId);
    }

    [Fact]
    public void GetActionInformation_NoActionCategory_ReturnsEmpty()
    {
        var request = CreateContextRequest(
        [
            (XacmlConstants.MatchAttributeCategory.Resource, XacmlRequestAttribute.OrgAttribute, "ttd"),
        ]);

        string actionId = EventLogHelper.GetActionInformation(request);
        Assert.Equal(string.Empty, actionId);
    }

    // --- GetClientIpAddress ---

    [Fact]
    public void GetClientIpAddress_NullContext_ReturnsNull()
    {
        Assert.Null(EventLogHelper.GetClientIpAddress(null));
    }
}
