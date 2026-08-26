using Altinn.AccessMgmt.PersistenceEF.Models.Consent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations.Consent;

/// <summary>
/// Maps <see cref="ConsentRequest"/> to the existing <c>consent.consentrequest</c> table.
/// </summary>
public class ConsentRequestConfiguration : IEntityTypeConfiguration<ConsentRequest>
{
    public void Configure(EntityTypeBuilder<ConsentRequest> builder)
    {
        builder.ToTable("consentrequest", "consent");
        builder.HasKey(x => x.ConsentRequestId);

        builder.Property(x => x.RequestMessage).HasColumnType("hstore");
        builder.Property(x => x.TemplateId).IsRequired();
        builder.Property(x => x.Status).HasColumnType("consent.status_type");
        builder.Property(x => x.PortalViewMode).HasColumnType("consent.portal_view_mode");

        // Both columns have database defaults that apply when an insert leaves them
        // out (created = CURRENT_TIMESTAMP, isdeleted = false); declare them so EF
        // omits unset properties instead of sending sentinel values.
        builder.Property(x => x.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.IsDeleted).HasDefaultValueSql("false");
    }
}
