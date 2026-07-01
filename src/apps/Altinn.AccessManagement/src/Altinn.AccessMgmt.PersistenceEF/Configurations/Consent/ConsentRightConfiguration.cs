using Altinn.AccessMgmt.PersistenceEF.Models.Consent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations.Consent;

/// <summary>
/// Maps <see cref="ConsentRight"/> to the existing <c>consent.consentright</c> table.
/// </summary>
public class ConsentRightConfiguration : IEntityTypeConfiguration<ConsentRight>
{
    public void Configure(EntityTypeBuilder<ConsentRight> builder)
    {
        builder.ToTable("consentright", "consent");
        builder.HasKey(x => x.ConsentRightId);
    }
}
