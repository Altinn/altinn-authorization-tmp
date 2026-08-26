using Altinn.AccessMgmt.PersistenceEF.Models.Consent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations.Consent;

/// <summary>
/// Maps <see cref="ConsentEvent"/> to the existing <c>consent.consentevent</c> table.
/// </summary>
public class ConsentEventConfiguration : IEntityTypeConfiguration<ConsentEvent>
{
    public void Configure(EntityTypeBuilder<ConsentEvent> builder)
    {
        builder.ToTable("consentevent", "consent");
        builder.HasKey(x => x.ConsentEventId);
        builder.Property(x => x.EventType).HasColumnType("consent.event_type");
    }
}
