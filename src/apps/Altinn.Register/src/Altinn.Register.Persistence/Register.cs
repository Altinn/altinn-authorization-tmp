using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.Register.Persistence.Entities;

#region Party Entities

/// <summary>
/// Party entity for individuals and organizations
/// </summary>
[Table("Parties")]
public class PartyEntity
{
    [Key]
    public int PartyId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public int PartyType { get; set; } // PartyType enum
    
    [MaxLength(20)]
    public string? OrganizationNumber { get; set; }
    
    [MaxLength(20)]
    public string? PersonIdentifier { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    
    // Navigation properties
    public List<PartyContactEntity> Contacts { get; set; } = new();
    public List<PartyAddressEntity> Addresses { get; set; } = new();
    public PartyDetailsEntity? Details { get; set; }
    public List<PartyRelationshipEntity> FromRelationships { get; set; } = new();
    public List<PartyRelationshipEntity> ToRelationships { get; set; } = new();
    
    /// <summary>
    /// Party contact entity
    /// </summary>
    [Table("PartyContacts")]
    public class PartyContactEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public int PartyId { get; set; }
        
        public int ContactType { get; set; } // ContactType enum
        
        [Required]
        [MaxLength(255)]
        public string Value { get; set; } = string.Empty;
        
        public bool IsPrimary { get; set; }
        
        // Navigation properties
        public PartyEntity Party { get; set; } = null!;
    }
    
    /// <summary>
    /// Party address entity
    /// </summary>
    [Table("PartyAddresses")]
    public class PartyAddressEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public int PartyId { get; set; }
        
        public int AddressType { get; set; } // AddressType enum
        
        [MaxLength(255)]
        public string? StreetAddress { get; set; }
        
        [MaxLength(20)]
        public string? PostalCode { get; set; }
        
        [MaxLength(100)]
        public string? City { get; set; }
        
        [MaxLength(100)]
        public string? Country { get; set; }
        
        public bool IsPrimary { get; set; }
        
        // Navigation properties
        public PartyEntity Party { get; set; } = null!;
    }
    
    /// <summary>
    /// Party details entity
    /// </summary>
    [Table("PartyDetails")]
    public class PartyDetailsEntity
    {
        [Key]
        public int PartyId { get; set; }
        
        public string? Description { get; set; }
        
        [MaxLength(255)]
        public string? Website { get; set; }
        
        [MaxLength(100)]
        public string? Industry { get; set; }
        
        public int? EmployeeCount { get; set; }
        public DateTime? FoundedDate { get; set; }
        
        public string? CustomAttributes { get; set; } // JSON serialized dictionary
        
        // Navigation properties
        public PartyEntity Party { get; set; } = null!;
    }
    
    public class PartyEntityConfiguration : IEntityTypeConfiguration<PartyEntity>
    {
        public void Configure(EntityTypeBuilder<PartyEntity> builder)
        {
            builder.HasIndex(e => e.OrganizationNumber).IsUnique();
            builder.HasIndex(e => e.PersonIdentifier).IsUnique();
            builder.HasIndex(e => e.Name);
            builder.HasIndex(e => e.PartyType);
            builder.HasIndex(e => e.IsDeleted);
            
            builder.HasOne(e => e.Details)
                   .WithOne(d => d.Party)
                   .HasForeignKey<PartyDetailsEntity>(d => d.PartyId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class PartyContactEntityConfiguration : IEntityTypeConfiguration<PartyContactEntity>
    {
        public void Configure(EntityTypeBuilder<PartyContactEntity> builder)
        {
            builder.HasIndex(e => new { e.PartyId, e.ContactType, e.IsPrimary });
            
            builder.HasOne(e => e.Party)
                   .WithMany(p => p.Contacts)
                   .HasForeignKey(e => e.PartyId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class PartyAddressEntityConfiguration : IEntityTypeConfiguration<PartyAddressEntity>
    {
        public void Configure(EntityTypeBuilder<PartyAddressEntity> builder)
        {
            builder.HasIndex(e => new { e.PartyId, e.AddressType, e.IsPrimary });
            
            builder.HasOne(e => e.Party)
                   .WithMany(p => p.Addresses)
                   .HasForeignKey(e => e.PartyId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class PartyDetailsEntityConfiguration : IEntityTypeConfiguration<PartyDetailsEntity>
    {
        public void Configure(EntityTypeBuilder<PartyDetailsEntity> builder)
        {
            builder.Property(e => e.Description).HasColumnType("text");
            builder.Property(e => e.CustomAttributes).HasColumnType("text");
        }
    }
}

/// <summary>
/// Party relationship entity
/// </summary>
[Table("PartyRelationships")]
public class PartyRelationshipEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public int FromPartyId { get; set; }
    public int ToPartyId { get; set; }
    
    public int RelationshipType { get; set; } // RelationshipType enum
    
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    
    // Navigation properties
    public PartyEntity FromParty { get; set; } = null!;
    public PartyEntity ToParty { get; set; } = null!;
    
    public class PartyRelationshipEntityConfiguration : IEntityTypeConfiguration<PartyRelationshipEntity>
    {
        public void Configure(EntityTypeBuilder<PartyRelationshipEntity> builder)
        {
            builder.HasIndex(e => new { e.FromPartyId, e.ToPartyId, e.RelationshipType }).IsUnique();
            builder.HasIndex(e => e.ValidFrom);
            builder.HasIndex(e => e.ValidTo);
            builder.HasIndex(e => e.IsActive);
            
            builder.HasOne(e => e.FromParty)
                   .WithMany(p => p.FromRelationships)
                   .HasForeignKey(e => e.FromPartyId)
                   .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.ToParty)
                   .WithMany(p => p.ToRelationships)
                   .HasForeignKey(e => e.ToPartyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

#endregion

#region User Entities

/// <summary>
/// User profile entity
/// </summary>
[Table("UserProfiles")]
public class UserProfileEntity
{
    [Key]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Email { get; set; }
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(10)]
    public string? PreferredLanguage { get; set; }
    
    public DateTime Created { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public List<UserPartyRelationEntity> PartyRelations { get; set; } = new();
    public UserPreferencesEntity? Preferences { get; set; }
    
    /// <summary>
    /// User-party relationship entity
    /// </summary>
    [Table("UserPartyRelations")]
    public class UserPartyRelationEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public int UserId { get; set; }
        public int PartyId { get; set; }
        
        public int RelationType { get; set; } // UserPartyRelationType enum
        
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        
        // Navigation properties
        public UserProfileEntity User { get; set; } = null!;
        public PartyEntity Party { get; set; } = null!;
    }
    
    /// <summary>
    /// User preferences entity
    /// </summary>
    [Table("UserPreferences")]
    public class UserPreferencesEntity
    {
        [Key]
        public int UserId { get; set; }
        
        [MaxLength(50)]
        public string? TimeZone { get; set; }
        
        [MaxLength(20)]
        public string? DateFormat { get; set; }
        
        [MaxLength(20)]
        public string? NumberFormat { get; set; }
        
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        
        public string? CustomSettings { get; set; } // JSON serialized dictionary
        
        // Navigation properties
        public UserProfileEntity User { get; set; } = null!;
    }
    
    public class UserProfileEntityConfiguration : IEntityTypeConfiguration<UserProfileEntity>
    {
        public void Configure(EntityTypeBuilder<UserProfileEntity> builder)
        {
            builder.HasIndex(e => e.UserName).IsUnique();
            builder.HasIndex(e => e.Email);
            builder.HasIndex(e => e.IsActive);
            
            builder.HasOne(e => e.Preferences)
                   .WithOne(p => p.User)
                   .HasForeignKey<UserPreferencesEntity>(p => p.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class UserPartyRelationEntityConfiguration : IEntityTypeConfiguration<UserPartyRelationEntity>
    {
        public void Configure(EntityTypeBuilder<UserPartyRelationEntity> builder)
        {
            builder.HasIndex(e => new { e.UserId, e.PartyId, e.RelationType });
            builder.HasIndex(e => e.ValidFrom);
            builder.HasIndex(e => e.ValidTo);
            builder.HasIndex(e => e.IsActive);
            
            builder.HasOne(e => e.User)
                   .WithMany(u => u.PartyRelations)
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(e => e.Party)
                   .WithMany()
                   .HasForeignKey(e => e.PartyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
    
    public class UserPreferencesEntityConfiguration : IEntityTypeConfiguration<UserPreferencesEntity>
    {
        public void Configure(EntityTypeBuilder<UserPreferencesEntity> builder)
        {
            builder.Property(e => e.CustomSettings).HasColumnType("text");
        }
    }
}

#endregion

#region Role Entities

/// <summary>
/// Role definition entity
/// </summary>
[Table("Roles")]
public class RoleEntity
{
    [Key]
    [MaxLength(50)]
    public string RoleCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public int RoleType { get; set; } // RoleType enum
    
    public bool IsDelegable { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    
    // Navigation properties
    public List<RoleRightEntity> Rights { get; set; } = new();
    public List<RoleRequiredRoleEntity> RequiredRoles { get; set; } = new();
    public List<RoleAssignmentEntity> Assignments { get; set; } = new();
    
    /// <summary>
    /// Role right entity
    /// </summary>
    [Table("RoleRights")]
    public class RoleRightEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        [MaxLength(50)]
        public string RoleCode { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Resource { get; set; } = string.Empty;
        
        public bool IsMandatory { get; set; }
        
        // Navigation properties
        public RoleEntity Role { get; set; } = null!;
        public List<RoleRightConditionEntity> Conditions { get; set; } = new();
        
        /// <summary>
        /// Role right condition entity
        /// </summary>
        [Table("RoleRightConditions")]
        public class RoleRightConditionEntity
        {
            [Key]
            public Guid Id { get; set; }
            
            public Guid RightId { get; set; }
            
            [Required]
            [MaxLength(255)]
            public string AttributeId { get; set; } = string.Empty;
            
            [Required]
            public string AttributeValue { get; set; } = string.Empty;
            
            public int MatchType { get; set; } // AttributeMatchType enum
            
            [MaxLength(100)]
            public string? DataType { get; set; }
            
            // Navigation properties
            public RoleRightEntity Right { get; set; } = null!;
        }
    }
    
    /// <summary>
    /// Role required role entity
    /// </summary>
    [Table("RoleRequiredRoles")]
    public class RoleRequiredRoleEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        [MaxLength(50)]
        public string RoleCode { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string RequiredRoleCode { get; set; } = string.Empty;
        
        // Navigation properties
        public RoleEntity Role { get; set; } = null!;
        public RoleEntity RequiredRole { get; set; } = null!;
    }
    
    public class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            builder.HasIndex(e => e.Name);
            builder.HasIndex(e => e.RoleType);
            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.ValidFrom);
            builder.HasIndex(e => e.ValidTo);
            
            builder.Property(e => e.Description).HasColumnType("text");
        }
    }
    
    public class RoleRightEntityConfiguration : IEntityTypeConfiguration<RoleRightEntity>
    {
        public void Configure(EntityTypeBuilder<RoleRightEntity> builder)
        {
            builder.HasIndex(e => new { e.RoleCode, e.Action, e.Resource }).IsUnique();
            
            builder.HasOne(e => e.Role)
                   .WithMany(r => r.Rights)
                   .HasForeignKey(e => e.RoleCode)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class RoleRightConditionEntityConfiguration : IEntityTypeConfiguration<RoleRightEntity.RoleRightConditionEntity>
    {
        public void Configure(EntityTypeBuilder<RoleRightEntity.RoleRightConditionEntity> builder)
        {
            builder.HasIndex(e => new { e.RightId, e.AttributeId });
            
            builder.HasOne(e => e.Right)
                   .WithMany(r => r.Conditions)
                   .HasForeignKey(e => e.RightId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class RoleRequiredRoleEntityConfiguration : IEntityTypeConfiguration<RoleRequiredRoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleRequiredRoleEntity> builder)
        {
            builder.HasIndex(e => new { e.RoleCode, e.RequiredRoleCode }).IsUnique();
            
            builder.HasOne(e => e.Role)
                   .WithMany(r => r.RequiredRoles)
                   .HasForeignKey(e => e.RoleCode)
                   .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(e => e.RequiredRole)
                   .WithMany()
                   .HasForeignKey(e => e.RequiredRoleCode)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

/// <summary>
/// Role assignment entity
/// </summary>
[Table("RoleAssignments")]
public class RoleAssignmentEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public int PartyId { get; set; }
    public int UserId { get; set; }
    
    [MaxLength(50)]
    public string RoleCode { get; set; } = string.Empty;
    
    public DateTime AssignedDate { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int AssignedByUserId { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public PartyEntity Party { get; set; } = null!;
    public UserProfileEntity User { get; set; } = null!;
    public RoleEntity Role { get; set; } = null!;
    public UserProfileEntity AssignedBy { get; set; } = null!;
    
    public class RoleAssignmentEntityConfiguration : IEntityTypeConfiguration<RoleAssignmentEntity>
    {
        public void Configure(EntityTypeBuilder<RoleAssignmentEntity> builder)
        {
            builder.HasIndex(e => new { e.PartyId, e.UserId, e.RoleCode });
            builder.HasIndex(e => e.AssignedDate);
            builder.HasIndex(e => e.ValidFrom);
            builder.HasIndex(e => e.ValidTo);
            builder.HasIndex(e => e.IsActive);
            
            builder.HasOne(e => e.Party)
                   .WithMany()
                   .HasForeignKey(e => e.PartyId)
                   .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.User)
                   .WithMany()
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.Role)
                   .WithMany(r => r.Assignments)
                   .HasForeignKey(e => e.RoleCode)
                   .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.AssignedBy)
                   .WithMany()
                   .HasForeignKey(e => e.AssignedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

#endregion

#region Main Unit Entities

/// <summary>
/// Main unit entity for organizations
/// </summary>
[Table("MainUnits")]
public class MainUnitEntity
{
    [Key]
    [MaxLength(20)]
    public string OrganizationNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? BusinessAddress { get; set; }
    
    [MaxLength(500)]
    public string? PostalAddress { get; set; }
    
    [MaxLength(20)]
    public string? IndustryCode { get; set; }
    
    [MaxLength(255)]
    public string? IndustryDescription { get; set; }
    
    public int? EmployeeCount { get; set; }
    public DateTime? FoundedDate { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public List<SubUnitEntity> SubUnits { get; set; } = new();
    public CompanyDetailsEntity? CompanyDetails { get; set; }
    
    /// <summary>
    /// Sub-unit entity
    /// </summary>
    [Table("SubUnits")]
    public class SubUnitEntity
    {
        [Key]
        [MaxLength(20)]
        public string OrganizationNumber { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string MainUnitOrganizationNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? BusinessAddress { get; set; }
        
        public bool IsActive { get; set; }
        public DateTime RegistrationDate { get; set; }
        
        // Navigation properties
        public MainUnitEntity MainUnit { get; set; } = null!;
    }
    
    /// <summary>
    /// Company details entity
    /// </summary>
    [Table("CompanyDetails")]
    public class CompanyDetailsEntity
    {
        [Key]
        [MaxLength(20)]
        public string OrganizationNumber { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? CompanyForm { get; set; }
        
        [MaxLength(50)]
        public string? ShareCapital { get; set; }
        
        [MaxLength(10)]
        public string? Currency { get; set; }
        
        public DateTime? LastAccountsDate { get; set; }
        
        public string? BusinessCodes { get; set; } // JSON serialized list
        public string? RegisterDetails { get; set; } // JSON serialized dictionary
        
        // Navigation properties
        public MainUnitEntity MainUnit { get; set; } = null!;
    }
    
    public class MainUnitEntityConfiguration : IEntityTypeConfiguration<MainUnitEntity>
    {
        public void Configure(EntityTypeBuilder<MainUnitEntity> builder)
        {
            builder.HasIndex(e => e.Name);
            builder.HasIndex(e => e.IndustryCode);
            builder.HasIndex(e => e.RegistrationDate);
            builder.HasIndex(e => e.IsActive);
            
            builder.HasOne(e => e.CompanyDetails)
                   .WithOne(d => d.MainUnit)
                   .HasForeignKey<CompanyDetailsEntity>(d => d.OrganizationNumber)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class SubUnitEntityConfiguration : IEntityTypeConfiguration<SubUnitEntity>
    {
        public void Configure(EntityTypeBuilder<SubUnitEntity> builder)
        {
            builder.HasIndex(e => e.MainUnitOrganizationNumber);
            builder.HasIndex(e => e.Name);
            builder.HasIndex(e => e.IsActive);
            
            builder.HasOne(e => e.MainUnit)
                   .WithMany(m => m.SubUnits)
                   .HasForeignKey(e => e.MainUnitOrganizationNumber)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class CompanyDetailsEntityConfiguration : IEntityTypeConfiguration<CompanyDetailsEntity>
    {
        public void Configure(EntityTypeBuilder<CompanyDetailsEntity> builder)
        {
            builder.Property(e => e.BusinessCodes).HasColumnType("text");
            builder.Property(e => e.RegisterDetails).HasColumnType("text");
        }
    }
}

#endregion