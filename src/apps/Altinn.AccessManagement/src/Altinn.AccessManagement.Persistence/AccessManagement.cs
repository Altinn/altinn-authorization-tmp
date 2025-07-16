using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessManagement.Persistence.Entities;

#region Delegation Entities

/// <summary>
/// Delegation database entity
/// </summary>
public class DelegationEntity
{
    public Guid Id { get; set; }
    public int OfferedByPartyId { get; set; }
    public string OfferedByName { get; set; } = string.Empty;
    public string? OfferedByOrganizationNumber { get; set; }
    public int CoveredByPartyId { get; set; }
    public string CoveredByName { get; set; } = string.Empty;
    public string? CoveredByOrganizationNumber { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public int PerformedByUserId { get; set; }
    public int Status { get; set; } // DelegationStatus enum as int
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation Properties
    public List<DelegationRightEntity> Rights { get; set; } = new();
    public List<ResourceReferenceEntity> ResourceReferences { get; set; } = new();
    public CompetentAuthorityEntity? CompetentAuthority { get; set; }

    /// <summary>
    /// Delegation right entity
    /// </summary>
    public class DelegationRightEntity
    {
        public Guid Id { get; set; }
        public Guid DelegationId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        
        // Navigation Properties
        public DelegationEntity Delegation { get; set; } = null!;
        public List<AttributeMatchEntity> AttributeMatches { get; set; } = new();

        /// <summary>
        /// Attribute match entity for delegation rights
        /// </summary>
        public class AttributeMatchEntity
        {
            public Guid Id { get; set; }
            public Guid DelegationRightId { get; set; }
            public string AttributeId { get; set; } = string.Empty;
            public string AttributeValue { get; set; } = string.Empty;
            public int MatchType { get; set; } // AttributeMatchType enum as int
            public string? DataType { get; set; }
            
            // Navigation
            public DelegationRightEntity DelegationRight { get; set; } = null!;
        }
    }

    /// <summary>
    /// Resource reference entity
    /// </summary>
    public class ResourceReferenceEntity
    {
        public Guid Id { get; set; }
        public Guid DelegationId { get; set; }
        public string ReferenceType { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string? ReferenceSource { get; set; }
        
        // Navigation
        public DelegationEntity Delegation { get; set; } = null!;
    }

    /// <summary>
    /// Competent authority entity
    /// </summary>
    public class CompetentAuthorityEntity
    {
        public Guid Id { get; set; }
        public Guid DelegationId { get; set; }
        public string? Orgcode { get; set; }
        public string? Organization { get; set; }
        public string? Name { get; set; }
        
        // Navigation
        public DelegationEntity Delegation { get; set; } = null!;
    }
}

/// <summary>
/// Delegation change event entity
/// </summary>
public class DelegationChangeEntity
{
    public Guid Id { get; set; }
    public Guid DelegationId { get; set; }
    public int ChangeType { get; set; } // DelegationChangeType enum as int
    public int PerformedByUserId { get; set; }
    public DateTime Created { get; set; }
    public string? ChangeDetails { get; set; }
    
    // Navigation
    public DelegationEntity? Delegation { get; set; }
}

#endregion

#region Authorized Party Entities

/// <summary>
/// Authorized party entity
/// </summary>
public class AuthorizedPartyEntity
{
    public Guid Id { get; set; }
    public int PartyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OrganizationNumber { get; set; }
    public string? PersonIdentifier { get; set; }
    public int PartyType { get; set; } // PartyType enum as int
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }

    // Navigation Properties
    public List<AuthorizedPartyRightEntity> Rights { get; set; } = new();
    public List<AuthorizedPartyResourceEntity> Resources { get; set; } = new();

    /// <summary>
    /// Authorized party right entity
    /// </summary>
    public class AuthorizedPartyRightEntity
    {
        public Guid Id { get; set; }
        public Guid AuthorizedPartyId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public int Source { get; set; } // RightSourceType enum as int
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        
        // Navigation Properties
        public AuthorizedPartyEntity AuthorizedParty { get; set; } = null!;
        public List<RightAttributeMatchEntity> AttributeMatches { get; set; } = new();

        /// <summary>
        /// Right attribute match entity
        /// </summary>
        public class RightAttributeMatchEntity
        {
            public Guid Id { get; set; }
            public Guid RightId { get; set; }
            public string AttributeId { get; set; } = string.Empty;
            public string AttributeValue { get; set; } = string.Empty;
            public int MatchType { get; set; } // AttributeMatchType enum as int
            public string? DataType { get; set; }
            
            // Navigation
            public AuthorizedPartyRightEntity Right { get; set; } = null!;
        }
    }

    /// <summary>
    /// Authorized party resource entity
    /// </summary>
    public class AuthorizedPartyResourceEntity
    {
        public Guid Id { get; set; }
        public Guid AuthorizedPartyId { get; set; }
        public string ResourceId { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AvailableActions { get; set; } = string.Empty; // JSON array as string
        
        // Navigation
        public AuthorizedPartyEntity AuthorizedParty { get; set; } = null!;
    }
}

#endregion

#region Entity Configurations

/// <summary>
/// EF Core configuration for DelegationEntity
/// </summary>
public class DelegationEntityConfiguration : IEntityTypeConfiguration<DelegationEntity>
{
    public void Configure(EntityTypeBuilder<DelegationEntity> builder)
    {
        builder.ToTable("Delegations", "access");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.OfferedByName)
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(e => e.OfferedByOrganizationNumber)
            .HasMaxLength(9);
            
        builder.Property(e => e.CoveredByName)
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(e => e.CoveredByOrganizationNumber)
            .HasMaxLength(9);
            
        builder.Property(e => e.ResourceId)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(e => e.ResourceType)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(e => e.RowVersion)
            .IsRowVersion();
            
        // Indexes
        builder.HasIndex(e => new { e.OfferedByPartyId, e.CoveredByPartyId, e.ResourceId })
            .HasDatabaseName("IX_Delegations_Parties_Resource");
            
        builder.HasIndex(e => e.Created)
            .HasDatabaseName("IX_Delegations_Created");
            
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Delegations_Status");

        // Navigation Properties
        builder.HasMany(e => e.Rights)
            .WithOne(r => r.Delegation)
            .HasForeignKey(r => r.DelegationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ResourceReferences)
            .WithOne(r => r.Delegation)
            .HasForeignKey(r => r.DelegationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.CompetentAuthority)
            .WithOne(c => c.Delegation)
            .HasForeignKey<DelegationEntity.CompetentAuthorityEntity>(c => c.DelegationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for DelegationRightEntity
/// </summary>
public class DelegationRightEntityConfiguration : IEntityTypeConfiguration<DelegationEntity.DelegationRightEntity>
{
    public void Configure(EntityTypeBuilder<DelegationEntity.DelegationRightEntity> builder)
    {
        builder.ToTable("DelegationRights", "access");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Action)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(e => e.Resource)
            .HasMaxLength(100)
            .IsRequired();
        
        // Index
        builder.HasIndex(e => new { e.DelegationId, e.Action, e.Resource })
            .HasDatabaseName("IX_DelegationRights_Delegation_Action_Resource");

        // Navigation
        builder.HasMany(e => e.AttributeMatches)
            .WithOne(a => a.DelegationRight)
            .HasForeignKey(a => a.DelegationRightId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for AttributeMatchEntity (Delegation Rights)
/// </summary>
public class DelegationRightAttributeMatchEntityConfiguration : IEntityTypeConfiguration<DelegationEntity.DelegationRightEntity.AttributeMatchEntity>
{
    public void Configure(EntityTypeBuilder<DelegationEntity.DelegationRightEntity.AttributeMatchEntity> builder)
    {
        builder.ToTable("DelegationRightAttributeMatches", "access");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.AttributeId)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.AttributeValue)
            .HasMaxLength(500)
            .IsRequired();
            
        builder.Property(e => e.DataType)
            .HasMaxLength(50);
        
        // Index
        builder.HasIndex(e => new { e.DelegationRightId, e.AttributeId })
            .HasDatabaseName("IX_DelegationRightAttributeMatches_Right_AttributeId");
    }
}

/// <summary>
/// EF Core configuration for ResourceReferenceEntity
/// </summary>
public class ResourceReferenceEntityConfiguration : IEntityTypeConfiguration<DelegationEntity.ResourceReferenceEntity>
{
    public void Configure(EntityTypeBuilder<DelegationEntity.ResourceReferenceEntity> builder)
    {
        builder.ToTable("ResourceReferences", "access");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ReferenceType)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(e => e.Reference)
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(e => e.ReferenceSource)
            .HasMaxLength(100);
        
        // Index
        builder.HasIndex(e => new { e.DelegationId, e.ReferenceType })
            .HasDatabaseName("IX_ResourceReferences_Delegation_Type");
    }
}

/// <summary>
/// EF Core configuration for CompetentAuthorityEntity
/// </summary>
public class CompetentAuthorityEntityConfiguration : IEntityTypeConfiguration<DelegationEntity.CompetentAuthorityEntity>
{
    public void Configure(EntityTypeBuilder<DelegationEntity.CompetentAuthorityEntity> builder)
    {
        builder.ToTable("CompetentAuthorities", "access");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Orgcode)
            .HasMaxLength(20);
        
        builder.Property(e => e.Organization)
            .HasMaxLength(255);
            
        builder.Property(e => e.Name)
            .HasMaxLength(255);
        
        // Index
        builder.HasIndex(e => e.DelegationId)
            .IsUnique()
            .HasDatabaseName("IX_CompetentAuthorities_DelegationId");
    }
}

/// <summary>
/// EF Core configuration for DelegationChangeEntity
/// </summary>
public class DelegationChangeEntityConfiguration : IEntityTypeConfiguration<DelegationChangeEntity>
{
    public void Configure(EntityTypeBuilder<DelegationChangeEntity> builder)
    {
        builder.ToTable("DelegationChanges", "access");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ChangeDetails)
            .HasMaxLength(1000);
        
        // Indexes
        builder.HasIndex(e => e.DelegationId)
            .HasDatabaseName("IX_DelegationChanges_DelegationId");
            
        builder.HasIndex(e => e.Created)
            .HasDatabaseName("IX_DelegationChanges_Created");
            
        builder.HasIndex(e => e.ChangeType)
            .HasDatabaseName("IX_DelegationChanges_ChangeType");

        // Navigation
        builder.HasOne(e => e.Delegation)
            .WithMany()
            .HasForeignKey(e => e.DelegationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// EF Core configuration for AuthorizedPartyEntity
/// </summary>
public class AuthorizedPartyEntityConfiguration : IEntityTypeConfiguration<AuthorizedPartyEntity>
{
    public void Configure(EntityTypeBuilder<AuthorizedPartyEntity> builder)
    {
        builder.ToTable("AuthorizedParties", "access");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(e => e.OrganizationNumber)
            .HasMaxLength(9);
            
        builder.Property(e => e.PersonIdentifier)
            .HasMaxLength(11);
        
        // Indexes
        builder.HasIndex(e => e.PartyId)
            .IsUnique()
            .HasDatabaseName("IX_AuthorizedParties_PartyId");
            
        builder.HasIndex(e => e.OrganizationNumber)
            .HasDatabaseName("IX_AuthorizedParties_OrganizationNumber");
            
        builder.HasIndex(e => e.PersonIdentifier)
            .HasDatabaseName("IX_AuthorizedParties_PersonIdentifier");

        // Navigation Properties
        builder.HasMany(e => e.Rights)
            .WithOne(r => r.AuthorizedParty)
            .HasForeignKey(r => r.AuthorizedPartyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Resources)
            .WithOne(r => r.AuthorizedParty)
            .HasForeignKey(r => r.AuthorizedPartyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for AuthorizedPartyRightEntity
/// </summary>
public class AuthorizedPartyRightEntityConfiguration : IEntityTypeConfiguration<AuthorizedPartyEntity.AuthorizedPartyRightEntity>
{
    public void Configure(EntityTypeBuilder<AuthorizedPartyEntity.AuthorizedPartyRightEntity> builder)
    {
        builder.ToTable("AuthorizedPartyRights", "access");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Action)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(e => e.Resource)
            .HasMaxLength(100)
            .IsRequired();
        
        // Indexes
        builder.HasIndex(e => new { e.AuthorizedPartyId, e.Action, e.Resource })
            .HasDatabaseName("IX_AuthorizedPartyRights_Party_Action_Resource");
            
        builder.HasIndex(e => e.Source)
            .HasDatabaseName("IX_AuthorizedPartyRights_Source");

        // Navigation
        builder.HasMany(e => e.AttributeMatches)
            .WithOne(a => a.Right)
            .HasForeignKey(a => a.RightId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for RightAttributeMatchEntity
/// </summary>
public class RightAttributeMatchEntityConfiguration : IEntityTypeConfiguration<AuthorizedPartyEntity.AuthorizedPartyRightEntity.RightAttributeMatchEntity>
{
    public void Configure(EntityTypeBuilder<AuthorizedPartyEntity.AuthorizedPartyRightEntity.RightAttributeMatchEntity> builder)
    {
        builder.ToTable("RightAttributeMatches", "access");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.AttributeId)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.AttributeValue)
            .HasMaxLength(500)
            .IsRequired();
            
        builder.Property(e => e.DataType)
            .HasMaxLength(50);
        
        // Index
        builder.HasIndex(e => new { e.RightId, e.AttributeId })
            .HasDatabaseName("IX_RightAttributeMatches_Right_AttributeId");
    }
}

/// <summary>
/// EF Core configuration for AuthorizedPartyResourceEntity
/// </summary>
public class AuthorizedPartyResourceEntityConfiguration : IEntityTypeConfiguration<AuthorizedPartyEntity.AuthorizedPartyResourceEntity>
{
    public void Configure(EntityTypeBuilder<AuthorizedPartyEntity.AuthorizedPartyResourceEntity> builder)
    {
        builder.ToTable("AuthorizedPartyResources", "access");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ResourceId)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.ResourceType)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(e => e.Title)
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(e => e.Description)
            .HasMaxLength(1000);
            
        builder.Property(e => e.AvailableActions)
            .HasMaxLength(2000);
        
        // Index
        builder.HasIndex(e => new { e.AuthorizedPartyId, e.ResourceId })
            .HasDatabaseName("IX_AuthorizedPartyResources_Party_Resource");
    }
}

#endregion