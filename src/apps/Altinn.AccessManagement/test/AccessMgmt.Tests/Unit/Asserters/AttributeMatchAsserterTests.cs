using System;
using System.Collections.Generic;
using Altinn.AccessManagement.Core.Asserters;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Resolvers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Altinn.AccessManagement.Tests.Unit.Asserters;

/// <summary>
/// summary
/// </summary>
[UnitTest]
public class AttributeMatchAsserterTests
{
    /// <summary>
    /// summary
    /// </summary>
    [Theory]
    [MemberData(nameof(DefaultToCases), MemberType = typeof(AttributeMatchAsserterTests))]
    public void DefaultTo(IEnumerable<AttributeMatch> values, Action<ValidationProblemDetails> assert)
    {
        var asserter = AsserterTests.Asserter<AttributeMatch>();

        var result = asserter.Evaluate(values, asserter.DefaultTo);

        assert(result);
    }

    /// <summary>
    /// summary
    /// </summary>
    public static TheoryData<IEnumerable<AttributeMatch>, Action<ValidationProblemDetails>> DefaultToCases =>
        new()
        {
            {
                [new(BaseUrn.Altinn.Person.IdentifierNo, "<identifierno>")],
                Assert.Null
            },
            {
                [new(BaseUrn.Altinn.Person.UserId, "123")],
                Assert.Null
            },
            {
                [new(BaseUrn.Altinn.Organization.IdentifierNo, "<identifierno>")],
                Assert.Null
            },
            {
                [new(BaseUrn.Altinn.Organization.PartyId, "321")],
                Assert.Null
            },
            {
                [new(BaseUrn.Altinn.Organization.PartyId, "<string>")],
                Assert.NotNull
            },
            {
                [new(BaseUrn.Altinn.Resource.ResourceRegistryId, "<resourceregistryid>")],
                Assert.NotNull
            },
            {
                [new(BaseUrn.Altinn.Organization.IdentifierNo, "<identifierno>"), new(BaseUrn.Altinn.Resource.AppId, "<appid>")],
                Assert.NotNull
            },
            {
                [new(BaseUrn.Altinn.Person.IdentifierNo, string.Empty)],
                Assert.NotNull
            }
        };

    /// <summary>
    /// summary
    /// </summary>
    [Theory]
    [MemberData(nameof(DefaultFromCases), MemberType = typeof(AttributeMatchAsserterTests))]
    public void DefaultFrom(IEnumerable<AttributeMatch> values, Action<ValidationProblemDetails> assert)
    {
        var asserter = AsserterTests.Asserter<AttributeMatch>();

        var result = asserter.Evaluate(values, asserter.DefaultFrom);

        assert(result);
    }

    /// <summary>
    /// summary
    /// </summary>
    public static TheoryData<IEnumerable<AttributeMatch>, Action<ValidationProblemDetails>> DefaultFromCases =>
        new()
        {
            {
                [new(BaseUrn.Altinn.Person.IdentifierNo, "<identifierno>")],
                Assert.Null
            },
            {
                [new(BaseUrn.Altinn.Person.UserId, "123")],
                Assert.Null
            },
            {
                [new(BaseUrn.Altinn.Organization.IdentifierNo, "<identifierno>")],
                Assert.Null
            },
            {
                [new(BaseUrn.Altinn.Organization.PartyId, "321")],
                Assert.Null
            },
            {
                [new(BaseUrn.Altinn.Organization.PartyId, "<string>")],
                Assert.NotNull
            },
            {
                [new(BaseUrn.Altinn.Resource.ResourceRegistryId, "<resourceregistryid>")],
                Assert.NotNull
            },
            {
                [new(BaseUrn.Altinn.Organization.IdentifierNo, "<identifierno>"), new(BaseUrn.Altinn.Resource.AppId, "<appid>")],
                Assert.NotNull
            },
            {
                [new(BaseUrn.Altinn.Person.IdentifierNo, string.Empty)],
                Assert.NotNull
            },
            {
                // urn:altinn:userid is accepted as a "to" identifier but is NOT a valid
                // "from" identifier, so DefaultFrom must reject it (DefaultTo accepts it).
                // This is the one case that actually distinguishes DefaultFrom from DefaultTo.
                [new(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, "123")],
                Assert.NotNull
            }
        };

    /// <summary>
    /// summary
    /// </summary>
    [Theory]
    [MemberData(nameof(DefaultResourceCases), MemberType = typeof(AttributeMatchAsserterTests))]
    public void DefaultResource(IEnumerable<AttributeMatch> values, Action<ValidationProblemDetails> assert)
    {
        var asserter = AsserterTests.Asserter<AttributeMatch>();

        var result = asserter.Evaluate(values, asserter.DefaultResource);

        assert(result);
    }

    /// <summary>
    /// summary
    /// </summary>
    public static TheoryData<IEnumerable<AttributeMatch>, Action<ValidationProblemDetails>> DefaultResourceCases =>
        new()
        {
            {
                [new(BaseUrn.Altinn.Resource.AppOwner, "<appowner>"), new(BaseUrn.Altinn.Resource.AppId, "<appid>")],
                Assert.Null
            },
            {
                [new(BaseUrn.Altinn.Resource.ResourceRegistryId, "<resourceregistryid>")],
                Assert.Null
            },
            {
                [new(BaseUrn.Altinn.Resource.ResourceRegistryId, "<resourceregistryid>"), new(BaseUrn.Altinn.Organization.IdentifierNo, "<identifierno>"), new(BaseUrn.Altinn.Resource.AppId, "<appid>")],
                Assert.NotNull
            },
            {
                [new(BaseUrn.Altinn.Resource.ResourceRegistryId, string.Empty)],
                Assert.NotNull
            }
        };

    /// <summary>
    /// A non-boolean value for a boolean-typed attribute must be reported under the
    /// <c>AttributesAreBoolean</c> key (regression: it was reported under
    /// <c>AttributesAreIntegers</c> due to a copy-paste error).
    /// </summary>
    [Fact]
    public void AttributesAreBoolean_NonBooleanValue_ReportsUnderBooleanKey()
    {
        var asserter = AsserterTests.Asserter<AttributeMatch>();
        var values = new AttributeMatch[] { new("urn:test:flag", "notabool") };

        var result = asserter.Evaluate(values, asserter.AttributesAreBoolean("urn:test:flag"));

        Assert.NotNull(result);
        Assert.Contains("AttributesAreBoolean", result.Errors.Keys);
    }
}
