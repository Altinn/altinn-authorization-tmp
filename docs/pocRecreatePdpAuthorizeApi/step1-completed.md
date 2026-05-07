# Step 1: Define the Authorization Request/Response Models — Completed

## Summary

Created XACML JSON profile DTO models in the shared contracts library `Altinn.Authorization.Api.Contracts` under the `Authorization` namespace.

## Project

`Altinn.Authorization.Api.Contracts`

## Namespace

`Altinn.Authorization.Api.Contracts.Authorization`

## Files Created

### Request DTOs

| File | Class | Description |
|---|---|---|
| `AuthorizationRequestDto.cs` | `AuthorizationRequestDto` | Root request wrapper containing a `Request` property |
| `AuthorizationXacmlRequestDto.cs` | `AuthorizationXacmlRequestDto` | XACML JSON request with AccessSubject, Resource, Action, MultiRequests |
| `AuthorizationXacmlCategoryDto.cs` | `AuthorizationXacmlCategoryDto` | Category object (used for Subject, Resource, Action, etc.) with Id, CategoryId, Content, and Attribute list |
| `AuthorizationXacmlAttributeDto.cs` | `AuthorizationXacmlAttributeDto` | Single attribute with AttributeId, Value, DataType, Issuer, IncludeInResult |
| `AuthorizationXacmlMultiRequestsDto.cs` | `AuthorizationXacmlMultiRequestsDto` | Container for multi-request references |
| `AuthorizationXacmlRequestReferenceDto.cs` | `AuthorizationXacmlRequestReferenceDto` | Individual request reference (list of ReferenceIds) |

### Response DTOs

| File | Class | Description |
|---|---|---|
| `AuthorizationResponseDto.cs` | `AuthorizationResponseDto` | Root response containing a list of results |
| `AuthorizationXacmlResultDto.cs` | `AuthorizationXacmlResultDto` | Single result with Decision, Status, Obligations, Advice, Category, PolicyIdentifierList |
| `AuthorizationXacmlStatusDto.cs` | `AuthorizationXacmlStatusDto` | Status with StatusMessage, StatusDetails, StatusCode |
| `AuthorizationXacmlStatusCodeDto.cs` | `AuthorizationXacmlStatusCodeDto` | Recursive status code (Value + nested StatusCode) |
| `AuthorizationXacmlObligationOrAdviceDto.cs` | `AuthorizationXacmlObligationOrAdviceDto` | Obligation or advice with Id and list of AttributeAssignment |
| `AuthorizationXacmlAttributeAssignmentDto.cs` | `AuthorizationXacmlAttributeAssignmentDto` | Attribute assignment with AttributeId, Value, Category, DataType, Issuer |
| `AuthorizationXacmlPolicyIdentifierListDto.cs` | `AuthorizationXacmlPolicyIdentifierListDto` | Lists of PolicyIdReference and PolicySetIdReference |
| `AuthorizationXacmlIdReferenceDto.cs` | `AuthorizationXacmlIdReferenceDto` | Single policy/policy set reference with Id and Version |

## Mapping to Old Models

These DTOs are structurally equivalent to the `*External` models in `Altinn.Platform.Authorization.Models.External`:

| Old Model (Altinn.Authorization) | New DTO (Api.Contracts) |
|---|---|
| `XacmlJsonRequestRootExternal` | `AuthorizationRequestDto` |
| `XacmlJsonRequestExternal` | `AuthorizationXacmlRequestDto` |
| `XacmlJsonCategoryExternal` | `AuthorizationXacmlCategoryDto` |
| `XacmlJsonAttributeExternal` | `AuthorizationXacmlAttributeDto` |
| `XacmlJsonMultiRequestsExternal` | `AuthorizationXacmlMultiRequestsDto` |
| `XacmlJsonRequestReferenceExternal` | `AuthorizationXacmlRequestReferenceDto` |
| `XacmlJsonResponseExternal` | `AuthorizationResponseDto` |
| `XacmlJsonResultExternal` | `AuthorizationXacmlResultDto` |
| `XacmlJsonStatusExternal` | `AuthorizationXacmlStatusDto` |
| `XacmlJsonStatusCodeExternal` | `AuthorizationXacmlStatusCodeDto` |
| `XacmlJsonObligationOrAdviceExternal` | `AuthorizationXacmlObligationOrAdviceDto` |
| `XacmlJsonAttributeAssignmentExternal` | `AuthorizationXacmlAttributeAssignmentDto` |
| `XacmlJsonPolicyIdentifierListExternal` | `AuthorizationXacmlPolicyIdentifierListDto` |
| `XacmlJsonIdReferenceExternal` | `AuthorizationXacmlIdReferenceDto` |

## Build Status

✅ Build successful
