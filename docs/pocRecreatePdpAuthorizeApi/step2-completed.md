# Step 2: Entity Resolution Service — Completed (Verification Only)

## Summary

Verified that the existing `IEntityService` in `Altinn.AccessMgmt.Core` fully covers all party/entity resolution needs required to replace the external API dependencies (`IRegisterService`, `IProfile`, `IParties`).

**No code changes were required.** The existing implementation is sufficient.

## Entity Model Properties

The `Entity` model (via `BaseEntity`) provides all the identifiers needed:

| Property | Type | Usage |
|---|---|---|
| `Id` | `Guid` | Party UUID — primary identifier |
| `PartyId` | `int?` | Altinn party ID |
| `UserId` | `int?` | Altinn user ID |
| `OrganizationIdentifier` | `string?` | Organization number |
| `PersonIdentifier` | `string?` | Person identifier (SSN) |
| `ParentId` | `Guid?` | Parent entity (for hierarchy/keyrole relationships) |
| `TypeId` / `Type` | `Guid` / `EntityType` | Entity type (Organization, Person, etc.) |
| `VariantId` / `Variant` | `Guid` / `EntityVariant` | Entity variant |
| `Name` | `string` | Display name |
| `IsDeleted` | `bool` | Soft-delete flag |

## Dependency Replacement Mapping

| Old Dependency Call | Replacement in `IEntityService` |
|---|---|
| `IRegisterService.PartyLookup(orgNo, null)` | `GetByOrgNo(string orgNo)` |
| `IRegisterService.PartyLookup(null, ssn)` | `GetByPersNo(string persNo)` |
| `IRegisterService.GetParty(int partyId)` | `GetByPartyId(int partyId)` |
| `IRegisterService.GetPartiesAsync(List<Guid>)` | `GetEntities(IEnumerable<Guid> ids)` |
| `IRegisterService.GetPartiesAsync(List<int>)` | `GetEntitiesByPartyIds(IEnumerable<int> partyIds)` |
| `IProfile.GetUserProfile(int userId)` → party info | `GetByUserId(int userId)` |
| `IParties` keyrole party lookups | `GetChildren(Guid parentId)` / `GetEntities()` with assignment queries |

## Key Observations

- `GetByOrgNo` and `GetByPersNo` return entities matching by `OrganizationIdentifier` and `PersonIdentifier` respectively — direct replacements for Register API `PartyLookup`.
- `GetByUserId` returns the entity for a given Altinn user ID — replaces the Profile API lookup that was used to get party UUID from user ID.
- `GetByPartyId` returns entity by Altinn party ID — replaces `IRegisterService.GetParty()`.
- `GetEntities(IEnumerable<Guid>)` supports batch UUID lookups — replaces `GetPartiesAsync(List<Guid>)`.
- `GetChildren` and `ParentId` on Entity support hierarchy/keyrole traversal.

## Build Status

✅ No changes — existing implementation verified as sufficient.
