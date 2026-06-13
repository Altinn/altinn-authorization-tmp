# Test Naming Convention

## Standard

All new test methods should follow this pattern:

```
MethodUnderTest_Scenario_ExpectedResult
```

### Examples

```csharp
// ✅ Good — clearly communicates intent
GetUser_WithInvalidId_Returns404NotFound()
TryDeleteDelegationPolicyRules_PolicyAlreadyDeleted_ReturnsNoChange()
CreateXacmlJsonRequest_MissingSubject_ThrowsArgumentException()

// ❌ Avoid — opaque numbered test cases
HandleRequirementAsync_TC01Async()
PDP_Decision_AltinnApps0001()
WritePolicy_TC03()
```

### Guidelines

| Principle | Detail |
|---|---|
| **No opaque IDs** | `TC01`, `0001` etc. are meaningless without reading the test body. Use a brief description instead. |
| **Use the method name** | Start with the method or endpoint under test. For HTTP endpoints, use the action (e.g., `Post_DeleteRules_…`). |
| **Scenario before result** | Describe the input/state, then the expected outcome. |
| **Keep it concise** | Aim for readable at a glance; abbreviate common terms if unambiguous. |
| **Async suffix** | Not required on test method names (suppressed via `VSTHRD200`). |

### Result vocabulary

For HTTP-endpoint tests, the result segment uses the **numeric + named status** form so the expected outcome is unambiguous:

| Status | Result segment |
|---|---|
| 200 | `Returns200Ok` |
| 201 | `Returns201Created` |
| 202 | `Returns202Accepted` |
| 204 | `Returns204NoContent` |
| 206 | `Returns206PartialContent` |
| 400 | `Returns400BadRequest` |
| 401 | `Returns401Unauthorized` |
| 403 | `Returns403Forbidden` |
| 404 | `Returns404NotFound` |
| 500 | `Returns500InternalServerError` |

Do **not** use bare or synonym forms for the result segment (`Success`, `OK`, `Valid`, `ReturnOk`, `BadRequest`) — those are ambiguous about the status or actually describe the *scenario*. Non-HTTP (service / unit) tests use a `Returns<Outcome>` segment describing the domain outcome (e.g. `ReturnsNull`, `ReturnsEmpty`, `ReturnsFalse`).

### Scope

Applies to all tests. The AccessManagement test projects (`AccessMgmt.Tests`, `Enduser.Api.Tests`, `Api.Tests`, `ServiceOwner.Api.Tests`, `Api.Internal.Tests`, `Core.Tests`, `PersistenceEF.Tests`) were standardized to this convention in #3463; new tests must follow it.

A narrow CI guard (`.github/scripts/check-test-naming.sh`, wired into `tpl-vertical-ci.yml`) backs this up for the AccessManagement vertical: it fails the build on a bare HTTP-status result segment (`...BadRequest`, `...ReturnsOk`), a numeric status without a name (`Returns400`), or an opaque id (`TCxx`, trailing digits). It deliberately does **not** police domain-outcome results (`ReturnsTrue`, `Success`, `Valid`, …) — those stay a review concern.
