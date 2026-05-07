# Step 7: DI Registration — Completed

## Summary

Added service registrations for the new authorization services in `ServiceCollectionExtensions.AddAccessMgmtCore()`.

## File Modified

| File | Change |
|---|---|
| `Extensions/ServiceCollectionExtensions.cs` | Added two `AddScoped` registrations |

## Registrations Added

```csharp
services.AddScoped<IAuthorizationContextService, AuthorizationContextService>();
services.AddScoped<IAuthorizationDecisionService, AuthorizationDecisionService>();
```

## Dependencies Already Registered

The following dependencies used by the new services were already registered:

| Service | Registration |
|---|---|
| `IEntityService` → `EntityService` | Already in `AddAccessMgmtCore` |
| `ConnectionQuery` | Already registered via `Altinn.AccessMgmt.PersistenceEF` extensions |
| `IPolicyRetrievalPoint` | Already registered in `Altinn.AccessManagement.Core` extensions |
| `ILogger<T>` | Provided by the framework |

## OED Role Assignment Service Registration

| Service | Status |
|---|---|
| `IOedRoleAssignmentService` | Registered as typed HTTP client `OedRoleAssignmentClient` in `IntegrationDependencyInjectionExtensions` |

Configured via `OedAuthzSettings:ApiEndpoint` app setting. Maskinporten token handling to be configured at host level.

## Build Status

✅ Build successful
