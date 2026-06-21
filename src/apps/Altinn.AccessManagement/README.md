# Altinn Access Management

Backend functionality for Access Management:
- Administration of rights for apps and resources
- Administration of rights for API schemes

## Getting started

Open `Altinn.AccessManagement.sln` and run the `Altinn.AccessManagement` project
(the browser opens the Swagger UI automatically).

Or from the command line:

```bash
dotnet run --project src/apps/Altinn.AccessManagement/src/Altinn.AccessManagement
```

## Setting up the database

Access Management needs a local PostgreSQL (Azure runs 14; 15 works locally).

1. Install and start PostgreSQL, choosing an admin password.
2. Create the database `authorizationdb`.
3. Create two login roles with privileges on it:
   - `platform_authorization_admin` (superuser, can-login)
   - `platform_authorization` (can-login)
4. Create the schema `delegations` in `authorizationdb`, owned by
   `platform_authorization_admin`.
