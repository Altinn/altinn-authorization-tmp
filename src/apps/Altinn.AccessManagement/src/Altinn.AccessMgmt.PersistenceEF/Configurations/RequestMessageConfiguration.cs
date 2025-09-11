using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class RequestMessageConfiguration : IEntityTypeConfiguration<RequestMessage>
{
    public void Configure(EntityTypeBuilder<RequestMessage> builder)
    {
        builder.ToDefaultTable();
        builder.EnableAudit();

        builder.HasKey(p => p.Id);

        builder.Property(t => t.RequestId).IsRequired();
        builder.Property(t => t.AuthorId).IsRequired();

        builder.PropertyWithReference(navKey: t => t.Request, foreignKey: t => t.RequestId, principalKey: t => t.Id);
        builder.PropertyWithReference(navKey: t => t.Author, foreignKey: t => t.AuthorId, principalKey: t => t.Id);
    }
}

public class AuditRequestMessageConfiguration : AuditConfiguration<AuditRequestMessage> { }
