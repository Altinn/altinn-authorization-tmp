using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class ConnectionConfiguration : IEntityTypeConfiguration<Connection> { }
public class ExtendedConnectionConfiguration : IEntityTypeConfiguration<ExtConnection> { }
public class AuditConnectionConfiguration : AuditConfiguration<AuditConnection> { public AuditConnectionConfiguration() : base("Connection") { } }
