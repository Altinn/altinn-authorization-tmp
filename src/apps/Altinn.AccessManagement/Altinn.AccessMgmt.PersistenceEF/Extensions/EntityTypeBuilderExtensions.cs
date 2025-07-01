using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static EntityTypeBuilder EnableAudit(this EntityTypeBuilder builder)
    {
        return builder.HasAnnotation("EnableAudit", true);
    }
    public static EntityTypeBuilder EnableTranslation(this EntityTypeBuilder builder)
    {
        return builder.HasAnnotation("EnableTranslation", true);
    }
}
