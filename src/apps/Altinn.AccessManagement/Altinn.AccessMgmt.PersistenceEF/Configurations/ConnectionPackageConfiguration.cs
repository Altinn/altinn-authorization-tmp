using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class ConnectionPackageConfiguration : IEntityTypeConfiguration<ConnectionPackage> { }
public class ExtendedConnectionPackageConfiguration : IEntityTypeConfiguration<ExtConnectionPackage> { }
public class AuditConnectionPackageConfiguration : AuditConfiguration<AuditConnectionPackage> { public AuditConnectionPackageConfiguration() : base("ConnectionPackage") { } }
