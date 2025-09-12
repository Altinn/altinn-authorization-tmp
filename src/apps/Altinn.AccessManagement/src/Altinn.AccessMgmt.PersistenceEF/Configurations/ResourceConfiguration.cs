﻿using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource> 
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToDefaultTable();
        builder.EnableAudit();

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.RefId);

        builder.PropertyWithReference(navKey: t => t.Type, foreignKey: t => t.TypeId, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.SetNull);
        builder.PropertyWithReference(navKey: t => t.Provider, foreignKey: t => t.ProviderId, principalKey: t => t.Id);
    }
}

public class AuditResourceConfiguration : AuditConfiguration<AuditResource> { }
