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
    }
}
