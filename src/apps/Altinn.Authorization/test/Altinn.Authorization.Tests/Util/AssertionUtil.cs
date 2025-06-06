using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Platform.Authorization.Clients.Interfaces;
using Altinn.Platform.Authorization.Models;
using Altinn.Platform.Authorization.Models.DelegationChangeEvent;
using Altinn.Platform.Authorization.Models.EventLog;
using Altinn.Platform.Authorization.Services.Interfaces;
using Altinn.Platform.Register.Models;
using Moq;
using Xunit;

namespace Altinn.Platform.Authorization.IntegrationTests.Util
{
    /// <summary>
    /// Class with methods that can help with assertions of larger objects.
    /// </summary>
    public static class AssertionUtil
    {
        /// <summary>
        /// Asserts that two collections of objects have the same property values in the same positions.
        /// </summary>
        /// <typeparam name="T">The Type</typeparam>
        /// <param name="expected">A collection of expected instances</param>
        /// <param name="actual">The collection of actual instances</param>
        /// <param name="assertMethod">The assertion method to be used</param>
        public static void AssertCollections<T>(ICollection<T> expected, ICollection<T> actual, Action<T, T> assertMethod)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);

            Dictionary<int, T> expectedDict = new Dictionary<int, T>();
            Dictionary<int, T> actualDict = new Dictionary<int, T>();

            int i = 1;
            foreach (T ex in expected)
            {
                expectedDict.Add(i, ex);
                i++;
            }

            i = 1;
            foreach (T ac in actual)
            {
                actualDict.Add(i, ac);
                i++;
            }

            foreach (int key in expectedDict.Keys)
            {
                assertMethod(expectedDict[key], actualDict[key]);
            }
        }

        /// <summary>
        /// Assert that two <see cref="XacmlContextResponse"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(XacmlContextResponse expected, XacmlContextResponse actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);
            Assert.Equal(expected.Results.Count, actual.Results.Count);

            if (expected.Results.Count > 0)
            {
                AssertEqual(expected.Results.First(), actual.Results.First());
            }
        }
        
        /// <summary>
        /// Assert that two <see cref="XacmlJsonResponse"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(XacmlJsonResponse expected, XacmlJsonResponse actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);
            Assert.Equal(expected.Response.Count, actual.Response.Count);

            if (expected.Response.Count > 0)
            {
                for (int i = 0; i < expected.Response.Count(); i++)
                {
                    AssertEqual(expected.Response[i], actual.Response[i]);
                }
            }
        }

        public static void AssertEqual(Dictionary<string, List<DelegationChange>> expected, Dictionary<string, List<DelegationChange>> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);
            foreach (KeyValuePair<string, List<DelegationChange>> expectedEntry in expected)
            {
                List<DelegationChange> actualValue = actual[expectedEntry.Key];
                Assert.NotNull(actualValue);
                AssertEqual(expectedEntry.Value, actualValue);
            }
        }

        public static void AssertEqual(List<DelegationChange> expected, List<DelegationChange> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);
            foreach (DelegationChange expectedEntity in expected)
            {
                DelegationChange actualentity = actual.FirstOrDefault(a => a.AltinnAppId == expectedEntity.AltinnAppId
                                                                        && a.BlobStoragePolicyPath == expectedEntity.BlobStoragePolicyPath
                                                                        && a.CoveredByPartyId == expectedEntity.CoveredByPartyId
                                                                        && a.CoveredByUserId == expectedEntity.CoveredByUserId
                                                                        && a.OfferedByPartyId == expectedEntity.OfferedByPartyId
                                                                        && a.DelegationChangeType == expectedEntity.DelegationChangeType);
                Assert.NotNull(actualentity);
            }
        }

        public static void AssertEqual(DelegationChangeEventList expected, DelegationChangeEventList actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.DelegationChangeEvents.Count, actual.DelegationChangeEvents.Count);
            for (int i = 0; i < expected.DelegationChangeEvents.Count; i++)
            {
                AssertEqual(expected.DelegationChangeEvents[i], actual.DelegationChangeEvents[i]);
            }
        }

        public static void AssertEqual(DelegationChangeEvent expected, DelegationChangeEvent actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.EventType, actual.EventType);
            AssertEqual(expected.DelegationChange, actual.DelegationChange);
        }

        public static void AssertEqual(SimpleDelegationChange expected, SimpleDelegationChange actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.DelegationChangeId, actual.DelegationChangeId);
            Assert.Equal(expected.AltinnAppId, actual.AltinnAppId);
            Assert.Equal(expected.OfferedByPartyId, actual.OfferedByPartyId);
            Assert.Equal(expected.CoveredByPartyId, actual.CoveredByPartyId);
            Assert.Equal(expected.CoveredByUserId, actual.CoveredByUserId);
            Assert.Equal(expected.PerformedByUserId, actual.PerformedByUserId);
            Assert.Equal(expected.Created, actual.Created);
        }

        /// <summary>
        /// Assert that two <see cref="XacmlContextRequest"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(XacmlContextRequest expected, XacmlContextRequest actual)
        {
            Assert.Equal(expected.Attributes.Count, actual.Attributes.Count);
            Assert.Equal(expected.GetResourceAttributes().Attributes.Count, actual.GetResourceAttributes().Attributes.Count);
            Assert.Equal(expected.GetSubjectAttributes().Attributes.Count, actual.GetSubjectAttributes().Attributes.Count);
            AssertEqual(expected.Attributes, actual.Attributes);
        }

        /// <summary>
        /// Assert that two Lists of <see cref="Rule"/> have the same number of rules and each rule have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        /// <param name="assertOutputValues">Whether output only values should also be asserted</param>
        public static void AssertEqual(List<Rule> expected, List<Rule> actual, bool assertOutputValues = false)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i], actual[i], assertOutputValues);
            }
        }

        /// <summary>
        /// Assert that two <see cref="Rule"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        /// <param name="assertOutputValues">Whether output only values should also be asserted</param>
        public static void AssertEqual(Rule expected, Rule actual, bool assertOutputValues = false)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            if (assertOutputValues)
            {
                Assert.Equal(expected.RuleId, actual.RuleId);
                Assert.Equal(expected.Type, actual.Type);
            }

            Assert.Equal(expected.CreatedSuccessfully, actual.CreatedSuccessfully);
            Assert.Equal(expected.DelegatedByUserId, actual.DelegatedByUserId);
            Assert.Equal(expected.OfferedByPartyId, actual.OfferedByPartyId);
            AssertEqual(expected.CoveredBy, actual.CoveredBy);
            AssertEqual(expected.Resource, actual.Resource);
            AssertEqual(expected.Action, actual.Action);
        }

        /// <summary>
        /// Assert that two <see cref="ResourcePolicyResponse"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertResourcePolicyResponseEqual(ResourcePolicyResponse expected, ResourcePolicyResponse actual)
        {
            if (actual.ResourcePolicies != null || expected.ResourcePolicies != null)
            {
                AssertCollections(expected.ResourcePolicies, actual.ResourcePolicies, AssertResourcePolicyEqual);
            }
            
            AssertCollections(expected.AppId, actual.AppId, AssertAttributeMatchEqual);
            Assert.Equal(expected.MinimumAuthenticationLevel, actual.MinimumAuthenticationLevel);

            if (expected.ErrorResponse != null && actual.ErrorResponse != null)
            {
                Assert.Equal(expected.ErrorResponse, actual.ErrorResponse);
            }
        }

        /// <summary>
        /// Assert that two <see cref="Rule"/> have the same property in the same positions.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertRuleEqual(Rule expected, Rule actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.CreatedSuccessfully, actual.CreatedSuccessfully);
            Assert.Equal(expected.DelegatedByUserId, actual.DelegatedByUserId);
            Assert.Equal(expected.OfferedByPartyId, actual.OfferedByPartyId);
            Assert.Equal(expected.Type, actual.Type);
            AssertEqual(expected.CoveredBy, actual.CoveredBy);
            AssertEqual(expected.Resource, actual.Resource);
            AssertEqual(expected.Action, actual.Action);
        }

        public static void AssertAuthorizationEvent(Mock<IEventsQueueClient> eventQueue, AuthorizationEvent expectedAuthorizationEvent, Times numberOfTimes)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            string serializedAuthorizationEvent = JsonSerializer.Serialize(expectedAuthorizationEvent, options);
            eventQueue.Verify(
                e => e.EnqueueAuthorizationEvent(
                    It.Is<string>(q => q == serializedAuthorizationEvent), It.IsAny<CancellationToken>()), 
                numberOfTimes);
        }

        /// <summary>
        /// Assert that two <see cref="AccessListAuthorizationResponse"/> have the same property values.
        /// </summary>
        /// <param name="expected">An instance with the expected values.</param>
        /// <param name="actual">The instance to verify.</param>
        public static void AssertEqual(AccessListAuthorizationResponse expected, AccessListAuthorizationResponse actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.Result, actual.Result);
            Assert.Equal(expected.Subject.ToString(), actual.Subject.ToString());
            Assert.Equal(expected.Resource.ToString(), actual.Resource.ToString());
            Assert.Equal(expected.Action.ToString(), actual.Action.ToString());
        }

        public static void AssertParty(Party expected, Party actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.PartyId, actual.PartyId);
            Assert.Equal(expected.PartyUuid, actual.PartyUuid);
            Assert.Equal(expected.PartyTypeName, actual.PartyTypeName);
            Assert.Equal(expected.OrgNumber, actual.OrgNumber);
            Assert.Equal(expected.SSN, actual.SSN);
            Assert.Equal(expected.UnitType, actual.UnitType);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.IsDeleted, actual.IsDeleted);
            Assert.Equal(expected.OnlyHierarchyElementWithNoAccess, actual.OnlyHierarchyElementWithNoAccess);
            Assert.Equal(expected.Organization, actual.Organization);
            Assert.Equal(expected.Person, actual.Person);
            AssertCollections(expected.ChildParties, actual.ChildParties, AssertParty);
        }

        private static void AssertEqual(List<AttributeMatch> expected, List<AttributeMatch> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i], actual[i]);
            }
        }

        private static void AssertEqual(AttributeMatch expected, AttributeMatch actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Value, actual.Value);
        }

        private static void AssertEqual(XacmlJsonResult expected, XacmlJsonResult actual)
        {
            Assert.Equal(expected.Decision, actual.Decision);
            Assert.Equal(expected.Status.StatusCode.Value, actual.Status.StatusCode.Value);
            AssertEqual(expected.Obligations, actual.Obligations);
            AssertEqual(expected.Category, actual.Category);
        }

        private static void AssertEqual(List<XacmlJsonObligationOrAdvice> expected, List<XacmlJsonObligationOrAdvice> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);

            Assert.Equal(expected.Count, actual.Count);

            AssertEqual(expected.FirstOrDefault(), actual.FirstOrDefault());
        }

        private static void AssertEqual(List<XacmlJsonCategory> expected, List<XacmlJsonCategory> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);

            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i], actual[i]);
            }
        }

        private static void AssertEqual(XacmlJsonCategory expected, XacmlJsonCategory actual)
        {
            Assert.Equal(expected.CategoryId, actual.CategoryId);
            Assert.Equal(expected.Content, actual.Content);
            Assert.Equal(expected.Id, actual.Id);
            AssertEqual(expected.Attribute, actual.Attribute);
        }

        private static void AssertEqual(XacmlJsonObligationOrAdvice expected, XacmlJsonObligationOrAdvice actual)
        {
            Assert.Equal(expected.AttributeAssignment.Count, actual.AttributeAssignment.Count);

            AssertEqual(expected.AttributeAssignment.FirstOrDefault(), actual.AttributeAssignment.FirstOrDefault());
        }

        private static void AssertEqual(XacmlJsonAttributeAssignment expected, XacmlJsonAttributeAssignment actual)
        {
            Assert.Equal(expected.AttributeId, actual.AttributeId);
            Assert.Equal(expected.Category, actual.Category);
            Assert.Equal(expected.DataType, actual.DataType);
            Assert.Equal(expected.Issuer, actual.Issuer);
            Assert.Equal(expected.Value, actual.Value, true);
        }

        private static void AssertEqual(List<XacmlJsonAttribute> expected, List<XacmlJsonAttribute> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i], actual[i]);
            }
        }

        private static void AssertEqual(XacmlJsonAttribute expected, XacmlJsonAttribute actual)
        {
            Assert.Equal(expected.AttributeId, actual.AttributeId);
            Assert.Equal(expected.DataType, actual.DataType);
            Assert.Equal(expected.IncludeInResult, actual.IncludeInResult);
            Assert.Equal(expected.Issuer, actual.Issuer);
            Assert.Equal(expected.Value, actual.Value, true);
        }

        private static void AssertEqual(XacmlContextResult expected, XacmlContextResult actual)
        {
            Assert.NotNull(actual);
            Assert.NotNull(expected);

            Assert.Equal(expected.Decision, actual.Decision);

            AssertEqual(expected.Status, actual.Status);

            AssertEqual(expected.Attributes, actual.Attributes);

            Assert.Equal(expected.Obligations.Count, actual.Obligations.Count);

            if (expected.Obligations.Count > 0)
            {
                AssertEqual(expected.Obligations.First(), actual.Obligations.First());
            }
        }

        private static void AssertEqual(XacmlContextStatus expected, XacmlContextStatus actual)
        {
            if (expected != null)
            {
                Assert.NotNull(actual);
                Assert.Equal(expected.StatusCode.StatusCode, actual.StatusCode.StatusCode);
            }
        }

        private static void AssertEqual(XacmlObligation expected, XacmlObligation actual)
        {
            Assert.Equal(expected.FulfillOn, actual.FulfillOn);
            Assert.Equal(expected.ObligationId, actual.ObligationId);
            Assert.Equal(expected.AttributeAssignment.Count, expected.AttributeAssignment.Count);

            if (expected.AttributeAssignment.Count > 0)
            {
                AssertEqual(expected.AttributeAssignment.First(), actual.AttributeAssignment.First());
            }
        }

        private static void AssertEqual(ICollection<XacmlContextAttributes> expected, ICollection<XacmlContextAttributes> actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            Assert.Equal(expected.Count, actual.Count);

            List<XacmlContextAttributes> expectedList = expected.ToList();
            List<XacmlContextAttributes> actualList = actual.ToList();

            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expectedList[i], actualList[i]);
            }
        }

        private static void AssertEqual(XacmlContextAttributes expected, XacmlContextAttributes actual)
        {
            Assert.Equal(expected.Category.OriginalString, actual.Category.OriginalString);

            List<XacmlAttribute> expectedList = expected.Attributes.ToList();
            List<XacmlAttribute> actualList = actual.Attributes.ToList();

            for (int i = 0; i < expected.Attributes.Count; i++)
            {
                AssertEqual(expectedList[i], actualList[i]);
            }
        }

        private static void AssertEqual(XacmlAttribute expected, XacmlAttribute actual)
        {
            Assert.Equal(expected.AttributeId, actual.AttributeId);
            Assert.Equal(expected.IncludeInResult, actual.IncludeInResult);
            Assert.Equal(expected.Issuer, actual.Issuer);
            Assert.Equal(expected.AttributeValues.Count, actual.AttributeValues.Count);
            AssertEqual(expected.AttributeValues, actual.AttributeValues);
        }

        private static void AssertEqual(ICollection<XacmlAttributeValue> expected, ICollection<XacmlAttributeValue> actual)
        {
            List<XacmlAttributeValue> expectedList = expected.ToList();
            List<XacmlAttributeValue> actualList = actual.ToList();

            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expectedList[i], actualList[i]);
            }
        }

        private static void AssertEqual(XacmlAttributeValue expected, XacmlAttributeValue actual)
        {
            Assert.Equal(expected.DataType, actual.DataType);
            Assert.Equal(expected.Value, actual.Value, ignoreCase: true);
        }

        private static void AssertEqual(XacmlAttributeAssignment expected, XacmlAttributeAssignment actual)
        {
            Assert.Equal(expected.Value, actual.Value, ignoreCase: true);
            Assert.Equal(expected.Category, actual.Category);
            Assert.Equal(expected.AttributeId, actual.AttributeId);
            Assert.Equal(expected.DataType, actual.DataType);
        }

        private static void AssertResourcePolicyEqual(ResourcePolicy expected, ResourcePolicy actual)
        {
            Assert.Equal(expected.Title, actual.Title);
            AssertCollections(expected.Actions, actual.Actions, AssertResourceActionEqual);
            AssertCollections(expected.Resource, actual.Resource, AssertAttributeMatchEqual);
            if (expected.Description != null || actual.Description != null)
            {
                Assert.Equal(expected.Description, actual.Description);
            }
        }

        private static void AssertResourceActionEqual(ResourceAction expected, ResourceAction actual)
        {
            Assert.Equal(expected.Title, actual.Title);
            AssertAttributeMatchEqual(expected.Match, actual.Match);
            AssertCollections(expected.RoleGrants, actual.RoleGrants, AssertRoleGrantEqual);

            if (expected.Description != null && actual.Description != null)
            {
                Assert.Equal(expected.Description, actual.Description);
            }
        }

        private static void AssertRoleGrantEqual(RoleGrant expected, RoleGrant actual)
        {
            Assert.Equal(expected.IsDelegable, actual.IsDelegable);
            Assert.Equal(expected.RoleTypeCode, actual.RoleTypeCode);
        }

        private static void AssertAttributeMatchEqual(AttributeMatch expected, AttributeMatch actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Value, actual.Value);
        }
    }
}
