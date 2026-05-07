# Step 8: Authorization Policy for the New Endpoint — Completed

## Summary

Added the `AuthorizeScopeAccess` authorization policy to protect the new authorize endpoint, requiring the `altinn:authorization/authorize` or `altinn:authorization/authorize.admin` Maskinporten scope.

## Files Modified

| File | Change |
|---|---|
| `AuthzConstants.cs` | Added `SCOPE_AUTHORIZE`, `SCOPE_AUTHORIZE_ADMIN`, and `POLICY_AUTHORIZE` constants |
| `AccessManagementHost.cs` | Registered the `AuthorizeScopeAccess` policy with `ScopeAccessRequirement` |
| `DecisionController.cs` | Added `[Authorize(Policy = AuthzConstants.POLICY_AUTHORIZE)]` to the endpoint |

## Constants Added

```csharp
public const string SCOPE_AUTHORIZE = "altinn:authorization/authorize";
public const string SCOPE_AUTHORIZE_ADMIN = "altinn:authorization/authorize.admin";
public const string POLICY_AUTHORIZE = "AuthorizeScopeAccess";
```

## Policy Registration

```csharp
.AddPolicy(AuthzConstants.POLICY_AUTHORIZE, policy =>
    policy.Requirements.Add(new ScopeAccessRequirement([
        AuthzConstants.SCOPE_AUTHORIZE,
        AuthzConstants.SCOPE_AUTHORIZE_ADMIN
    ])))
```

This means the endpoint accepts either scope — `altinn:authorization/authorize` (for service owners accessing their own resources) or `altinn:authorization/authorize.admin` (for cross-owner access). This matches the scopes used by the old endpoint in `Altinn.Authorization`.

## Build Status

✅ Build successful
