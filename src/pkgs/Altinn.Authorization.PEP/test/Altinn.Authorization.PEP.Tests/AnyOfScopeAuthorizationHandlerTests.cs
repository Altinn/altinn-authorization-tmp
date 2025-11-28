using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Security.Claims;
using Altinn.Authorization.Scopes;
using Microsoft.AspNetCore.Authorization;

namespace UnitTests;

public class AnyOfScopeAuthorizationHandlerTests
{
    private readonly AnyOfScopeAuthorizationHandler _sut;

    public AnyOfScopeAuthorizationHandlerTests()
    {
        _sut = new();
    }

    /// <summary>
    /// Test case: Valid scope claim is included in context.
    /// Expected: Context will succeed.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidScope_ContextSuccess()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateAuthzHandlerContext("altinn:appdeploy");

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Test case: Valid scope claim is included in context.
    /// Expected: Context will succeed.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidScopeOf2_OneInvalidPresent_ContextSuccess()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateAuthzHandlerContext("altinn:resourceregistry:write altinn:resourceregistry:read", ["altinn:resourceregistry:admin", "altinn:resourceregistry:write"]);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Test case: Valid scope claim is included in context.
    /// Expected: Context will succeed.
    /// </summary>
    [Theory]
    [InlineData("scope:start")]
    [InlineData("scope:mid")]
    [InlineData("scope:end")]
    public async Task HandleAsync_ValidScope_PartOfScopeString_ContextSuccess(string valid)
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateAuthzHandlerContext("scope:start scope:mid scope:end", [valid]);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task HandleAsync_InvalidScope_NotWholeWord_ContextFail()
    {
        // Arrange
        AuthorizationHandlerContext context = CreateAuthzHandlerContext("scope:start scope:mid scope:end", ["scope", "start", ":", "mid", "end"]);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    /// <summary>
    /// Test case: Valid scope is missing in context
    /// Expected: Context will fail.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidScopeOf2_OneInvalidPresent_ContextFail()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateAuthzHandlerContext("altinn:resourceregistry:read", ["altinn:resourceregistry:admin", "altinn:resourceregistry:write"]);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    /// <summary>
    /// Test case: Valid scope claim is included in context.
    /// Expected: Context will succeed.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidScopeOf2_ContextSuccess()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateAuthzHandlerContext("altinn:resourceregistry:write", ["altinn:resourceregistry:admin", "altinn:resourceregistry:write"]);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task HandleAsync_ValidScope_PartOfLongerScope_ContextSuccess()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateAuthzHandlerContext("altin:foo:bar:baz altinn:foo:bar altinn:foo", ["altinn:foo:bar"]);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Test case: Invalid scope claim is included in context.
    /// Expected: Context will fail.
    /// </summary>
    [Fact]
    public async Task HandleAsync_InvalidScope_ContextFail()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateAuthzHandlerContext("altinn:invalid");

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    /// <summary>
    /// Test case: Empty scope claim is included in context.
    /// Expected: Context will fail.
    /// </summary>
    [Fact]
    public async Task HandleAsync_EmptyScope_ContextFail()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateAuthzHandlerContext(string.Empty);

        // Act
        await _sut.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    private AuthorizationHandlerContext CreateAuthzHandlerContext(string scopeClaim)
    {
        AnyOfScopeAuthorizationRequirement requirement = new AnyOfScopeAuthorizationRequirement("altinn:appdeploy");

        ClaimsPrincipal user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim("urn:altinn:scope", scopeClaim, "string", "org"),
                    new Claim("urn:altinn:org", "brg", "string", "org")
                },
                "AuthenticationTypes.Federation"));

        AuthorizationHandlerContext context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            default(Document));
        return context;
    }

    private AuthorizationHandlerContext CreateAuthzHandlerContext(string scopeClaim, scoped ReadOnlySpan<string> requiredScopes)
    {
        AnyOfScopeAuthorizationRequirement requirement = new AnyOfScopeAuthorizationRequirement(requiredScopes);

        ClaimsPrincipal user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim("scope", scopeClaim, "string", "org"),
                    new Claim("urn:altinn:org", "brg", "string", "org")
                },
                "AuthenticationTypes.Federation"));

        AuthorizationHandlerContext context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            default(Document));
        return context;
    }
}
