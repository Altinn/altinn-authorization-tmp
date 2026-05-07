# Step 6: DecisionController in ServiceOwner API — Completed

## Summary

Created a thin `DecisionController` in `Altinn.AccessManagement.Api.ServiceOwner` that delegates all business logic to `IAuthorizationDecisionService`.

## File Created

| File | Description |
|---|---|
| `Controllers/DecisionController.cs` | Thin controller with single `Authorize` endpoint |

## Endpoint

| Method | Route | Auth |
|---|---|---|
| `POST` | `accessmanagement/api/v1/serviceowner/authorize` | `AuthorizeScopeAccess` policy (Step 8) |

## Implementation

```csharp
[ApiController]
[Route("accessmanagement/api/v1/serviceowner")]
public class DecisionController(IAuthorizationDecisionService authorizationDecisionService) : ControllerBase
{
    [HttpPost("authorize")]
    public async Task<ActionResult<AuthorizationResponseDto>> Authorize(
        [FromBody] AuthorizationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await authorizationDecisionService.AuthorizeAsync(request, cancellationToken);
        return Ok(response);
    }
}
```

## Design

- **Primary constructor** — follows the convention used by other controllers in the project
- **No business logic** — all decision-making is delegated to the service layer
- **Contract DTOs** — uses `AuthorizationRequestDto` / `AuthorizationResponseDto` from `Altinn.Authorization.Api.Contracts`
- **Content types** — explicit JSON media types via `[Consumes]` and `[Produces]`

## Build Status

✅ Build successful
