using Altinn.AccessMgmt.PersistenceEF.Models.Consent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations.Consent;

/// <summary>
/// Maps <see cref="ConsentMetadata"/> to the existing <c>consent.metadata</c> table.
/// The table has no primary key, so the entity is keyless.
/// </summary>
public class ConsentMetadataConfiguration : IEntityTypeConfiguration<ConsentMetadata>
{
    public void Configure(EntityTypeBuilder<ConsentMetadata> builder)
    {
        builder.ToTable("metadata", "consent");
        builder.HasNoKey();
    }
}
