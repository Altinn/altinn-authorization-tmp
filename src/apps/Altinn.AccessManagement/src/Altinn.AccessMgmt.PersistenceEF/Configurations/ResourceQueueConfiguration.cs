using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class ResourceQueueConfiguration : IEntityTypeConfiguration<ResourceQueue>
{
    public void Configure(EntityTypeBuilder<ResourceQueue> builder)
    {
        builder.ToDefaultTable();
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
    }
}
