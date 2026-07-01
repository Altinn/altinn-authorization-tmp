using Altinn.AccessMgmt.PersistenceEF.Models.Consent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations.Consent;

/// <summary>
/// Maps <see cref="ConsentResourceAttribute"/> to the existing
/// <c>consent.resourceattribute</c> table, keyed on <c>consentrightid</c>.
/// </summary>
public class ConsentResourceAttributeConfiguration : IEntityTypeConfiguration<ConsentResourceAttribute>
{
    public void Configure(EntityTypeBuilder<ConsentResourceAttribute> builder)
    {
        builder.ToTable("resourceattribute", "consent");
        builder.HasKey(x => x.ConsentRightId);
    }
}
