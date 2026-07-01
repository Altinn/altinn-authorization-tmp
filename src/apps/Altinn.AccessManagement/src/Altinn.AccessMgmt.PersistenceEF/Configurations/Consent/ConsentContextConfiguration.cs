using Altinn.AccessMgmt.PersistenceEF.Models.Consent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations.Consent;

/// <summary>
/// Maps <see cref="ConsentContext"/> to the existing <c>consent.context</c> table.
/// </summary>
public class ConsentContextConfiguration : IEntityTypeConfiguration<ConsentContext>
{
    public void Configure(EntityTypeBuilder<ConsentContext> builder)
    {
        builder.ToTable("context", "consent");
        builder.HasKey(x => x.ContextId);
        builder.Property(x => x.Language).IsRequired();
    }
}
