﻿using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

/// <inheritdoc/>
public interface IPackageDelegationService : IDbExtendedRepo<PackageDelegation, ExtPackageDelegation> { }