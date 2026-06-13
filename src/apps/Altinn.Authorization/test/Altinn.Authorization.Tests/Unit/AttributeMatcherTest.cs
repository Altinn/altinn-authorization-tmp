using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Xacml;
using Xunit;

namespace Altinn.Authorization.Tests.Unit
{
    /// <summary>
    /// Pure-logic tests for <see cref="AttributeMatcher"/>, the XACML attribute-match
    /// function dispatch.
    ///
    /// Focus is the <c>string-is-in</c> function. The matcher is invoked once per
    /// (policy value, single request-bag element), so for bag membership each
    /// element must be compared for equality with the policy value — never with a
    /// substring test. The bundled OASIS conformance suite (IIA008/009, IIC008/009,
    /// IIIF003/004/007) only exercises exact-match and clear non-match cases, where a
    /// substring test happens to give the same answer as equality, so it does not on
    /// its own catch a substring defect. These tests pin the membership semantics
    /// directly.
    /// </summary>
    [UnitTest]
    public class AttributeMatcherTest
    {
        private const string StringIsIn = XacmlConstants.AttributeMatchFunction.StringIsIn;

        [Fact]
        public void MatchAttributes_StringIsIn_RequestValueEqualsPolicyValue_ReturnsTrue()
        {
            // The request-bag element equals the policy value, so the value is a member of the bag.
            Assert.True(AttributeMatcher.MatchAttributes("riddle me this", "riddle me this", StringIsIn));
        }

        [Theory]
        [InlineData("admin", "superadmin")]   // policy value is a suffix of the request value
        [InlineData("read", "readwrite")]     // policy value is a prefix of the request value
        [InlineData("user", "superuser")]
        public void MatchAttributes_StringIsIn_PolicyValueIsSubstringOfRequestValue_ReturnsFalse(string policyValue, string requestValue)
        {
            // Regression guard: string-is-in is bag membership (per-element equality), not
            // String.Contains. A substring match would let the policy value "admin" match a
            // request value "superadmin" and wrongly satisfy the target — a privilege escalation.
            Assert.False(AttributeMatcher.MatchAttributes(policyValue, requestValue, StringIsIn));
        }

        [Fact]
        public void MatchAttributes_StringIsIn_NoOverlap_ReturnsFalse()
        {
            Assert.False(AttributeMatcher.MatchAttributes("convicted-felon", "law-abiding-citizen", StringIsIn));
        }
    }
}
