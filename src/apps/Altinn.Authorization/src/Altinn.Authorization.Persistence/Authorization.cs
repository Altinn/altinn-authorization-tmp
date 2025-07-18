using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.Authorization.Persistence.Entities;

#region XACML Request/Response Entities

/// <summary>
/// Authorization request entity for XACML storage
/// </summary>
[Table("AuthorizationRequests")]
public class AuthorizationRequestEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string RequestId { get; set; } = string.Empty;
    
    [Required]
    public string RequestContent { get; set; } = string.Empty; // JSON serialized request
    
    public DateTime RequestTime { get; set; }
    public DateTime? ResponseTime { get; set; }
    
    [MaxLength(50)]
    public string? Status { get; set; }
    
    [MaxLength(50)]
    public string? Decision { get; set; }
    
    public string? ResponseContent { get; set; } // JSON serialized response
    
    // Navigation properties
    public List<AuthorizationSubjectEntity> Subjects { get; set; } = new();
    public List<AuthorizationActionEntity> Actions { get; set; } = new();
    public List<AuthorizationResourceEntity> Resources { get; set; } = new();
    public List<AuthorizationEnvironmentEntity> Environment { get; set; } = new();
    
    public class AuthorizationRequestEntityConfiguration : IEntityTypeConfiguration<AuthorizationRequestEntity>
    {
        public void Configure(EntityTypeBuilder<AuthorizationRequestEntity> builder)
        {
            builder.HasIndex(e => e.RequestId).IsUnique();
            builder.HasIndex(e => e.RequestTime);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.Decision);
            
            builder.Property(e => e.RequestContent).HasColumnType("text");
            builder.Property(e => e.ResponseContent).HasColumnType("text");
        }
    }
}

/// <summary>
/// Authorization subject entity
/// </summary>
[Table("AuthorizationSubjects")]
public class AuthorizationSubjectEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid AuthorizationRequestId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string SubjectId { get; set; } = string.Empty;
    
    // Navigation properties
    public AuthorizationRequestEntity AuthorizationRequest { get; set; } = null!;
    public List<SubjectAttributeEntity> Attributes { get; set; } = new();
    
    /// <summary>
    /// Subject attribute entity
    /// </summary>
    [Table("SubjectAttributes")]
    public class SubjectAttributeEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid SubjectId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string AttributeId { get; set; } = string.Empty;
        
        [Required]
        public string AttributeValue { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? DataType { get; set; }
        
        [MaxLength(255)]
        public string? Issuer { get; set; }
        
        // Navigation properties
        public AuthorizationSubjectEntity Subject { get; set; } = null!;
    }
    
    public class AuthorizationSubjectEntityConfiguration : IEntityTypeConfiguration<AuthorizationSubjectEntity>
    {
        public void Configure(EntityTypeBuilder<AuthorizationSubjectEntity> builder)
        {
            builder.HasIndex(e => new { e.AuthorizationRequestId, e.SubjectId });
            
            builder.HasOne(e => e.AuthorizationRequest)
                   .WithMany(r => r.Subjects)
                   .HasForeignKey(e => e.AuthorizationRequestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class SubjectAttributeEntityConfiguration : IEntityTypeConfiguration<SubjectAttributeEntity>
    {
        public void Configure(EntityTypeBuilder<SubjectAttributeEntity> builder)
        {
            builder.HasIndex(e => new { e.SubjectId, e.AttributeId });
            
            builder.HasOne(e => e.Subject)
                   .WithMany(s => s.Attributes)
                   .HasForeignKey(e => e.SubjectId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

/// <summary>
/// Authorization action entity
/// </summary>
[Table("AuthorizationActions")]
public class AuthorizationActionEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid AuthorizationRequestId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ActionId { get; set; } = string.Empty;
    
    // Navigation properties
    public AuthorizationRequestEntity AuthorizationRequest { get; set; } = null!;
    public List<ActionAttributeEntity> Attributes { get; set; } = new();
    
    /// <summary>
    /// Action attribute entity
    /// </summary>
    [Table("ActionAttributes")]
    public class ActionAttributeEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid ActionId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string AttributeId { get; set; } = string.Empty;
        
        [Required]
        public string AttributeValue { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? DataType { get; set; }
        
        // Navigation properties
        public AuthorizationActionEntity Action { get; set; } = null!;
    }
    
    public class AuthorizationActionEntityConfiguration : IEntityTypeConfiguration<AuthorizationActionEntity>
    {
        public void Configure(EntityTypeBuilder<AuthorizationActionEntity> builder)
        {
            builder.HasIndex(e => new { e.AuthorizationRequestId, e.ActionId });
            
            builder.HasOne(e => e.AuthorizationRequest)
                   .WithMany(r => r.Actions)
                   .HasForeignKey(e => e.AuthorizationRequestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class ActionAttributeEntityConfiguration : IEntityTypeConfiguration<ActionAttributeEntity>
    {
        public void Configure(EntityTypeBuilder<ActionAttributeEntity> builder)
        {
            builder.HasIndex(e => new { e.ActionId, e.AttributeId });
            
            builder.HasOne(e => e.Action)
                   .WithMany(a => a.Attributes)
                   .HasForeignKey(e => e.ActionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

/// <summary>
/// Authorization resource entity
/// </summary>
[Table("AuthorizationResources")]
public class AuthorizationResourceEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid AuthorizationRequestId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ResourceId { get; set; } = string.Empty;
    
    // Navigation properties
    public AuthorizationRequestEntity AuthorizationRequest { get; set; } = null!;
    public List<ResourceAttributeEntity> Attributes { get; set; } = new();
    
    /// <summary>
    /// Resource attribute entity
    /// </summary>
    [Table("ResourceAttributes")]
    public class ResourceAttributeEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid ResourceId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string AttributeId { get; set; } = string.Empty;
        
        [Required]
        public string AttributeValue { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? DataType { get; set; }
        
        // Navigation properties
        public AuthorizationResourceEntity Resource { get; set; } = null!;
    }
    
    public class AuthorizationResourceEntityConfiguration : IEntityTypeConfiguration<AuthorizationResourceEntity>
    {
        public void Configure(EntityTypeBuilder<AuthorizationResourceEntity> builder)
        {
            builder.HasIndex(e => new { e.AuthorizationRequestId, e.ResourceId });
            
            builder.HasOne(e => e.AuthorizationRequest)
                   .WithMany(r => r.Resources)
                   .HasForeignKey(e => e.AuthorizationRequestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class ResourceAttributeEntityConfiguration : IEntityTypeConfiguration<ResourceAttributeEntity>
    {
        public void Configure(EntityTypeBuilder<ResourceAttributeEntity> builder)
        {
            builder.HasIndex(e => new { e.ResourceId, e.AttributeId });
            
            builder.HasOne(e => e.Resource)
                   .WithMany(r => r.Attributes)
                   .HasForeignKey(e => e.ResourceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

/// <summary>
/// Authorization environment entity
/// </summary>
[Table("AuthorizationEnvironment")]
public class AuthorizationEnvironmentEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid AuthorizationRequestId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string AttributeId { get; set; } = string.Empty;
    
    [Required]
    public string AttributeValue { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? DataType { get; set; }
    
    // Navigation properties
    public AuthorizationRequestEntity AuthorizationRequest { get; set; } = null!;
    
    public class AuthorizationEnvironmentEntityConfiguration : IEntityTypeConfiguration<AuthorizationEnvironmentEntity>
    {
        public void Configure(EntityTypeBuilder<AuthorizationEnvironmentEntity> builder)
        {
            builder.HasIndex(e => new { e.AuthorizationRequestId, e.AttributeId });
            
            builder.HasOne(e => e.AuthorizationRequest)
                   .WithMany(r => r.Environment)
                   .HasForeignKey(e => e.AuthorizationRequestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

#endregion

#region Policy Entities

/// <summary>
/// Policy entity for XACML policy storage
/// </summary>
[Table("Policies")]
public class PolicyEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string PolicyId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Version { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty; // XACML policy as XML
    
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    
    public int Status { get; set; } // PolicyStatus enum
    
    public string? Description { get; set; }
    
    // Navigation properties
    public List<PolicyTagEntity> Tags { get; set; } = new();
    public List<PolicyValidationEntity> Validations { get; set; } = new();
    
    /// <summary>
    /// Policy tag entity
    /// </summary>
    [Table("PolicyTags")]
    public class PolicyTagEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid PolicyId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Tag { get; set; } = string.Empty;
        
        // Navigation properties
        public PolicyEntity Policy { get; set; } = null!;
    }
    
    public class PolicyEntityConfiguration : IEntityTypeConfiguration<PolicyEntity>
    {
        public void Configure(EntityTypeBuilder<PolicyEntity> builder)
        {
            builder.HasIndex(e => new { e.PolicyId, e.Version }).IsUnique();
            builder.HasIndex(e => e.Created);
            builder.HasIndex(e => e.Status);
            
            builder.Property(e => e.Content).HasColumnType("text");
            builder.Property(e => e.Description).HasColumnType("text");
        }
    }
    
    public class PolicyTagEntityConfiguration : IEntityTypeConfiguration<PolicyTagEntity>
    {
        public void Configure(EntityTypeBuilder<PolicyTagEntity> builder)
        {
            builder.HasIndex(e => new { e.PolicyId, e.Tag }).IsUnique();
            
            builder.HasOne(e => e.Policy)
                   .WithMany(p => p.Tags)
                   .HasForeignKey(e => e.PolicyId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

/// <summary>
/// Policy validation entity
/// </summary>
[Table("PolicyValidations")]
public class PolicyValidationEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid PolicyId { get; set; }
    
    public bool IsValid { get; set; }
    public DateTime ValidatedAt { get; set; }
    
    // Navigation properties
    public PolicyEntity Policy { get; set; } = null!;
    public List<ValidationErrorEntity> Errors { get; set; } = new();
    public List<ValidationWarningEntity> Warnings { get; set; } = new();
    
    /// <summary>
    /// Validation error entity
    /// </summary>
    [Table("ValidationErrors")]
    public class ValidationErrorEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid ValidationId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string? Location { get; set; }
        
        public int? Line { get; set; }
        public int? Column { get; set; }
        
        // Navigation properties
        public PolicyValidationEntity Validation { get; set; } = null!;
    }
    
    /// <summary>
    /// Validation warning entity
    /// </summary>
    [Table("ValidationWarnings")]
    public class ValidationWarningEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid ValidationId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string? Location { get; set; }
        
        // Navigation properties
        public PolicyValidationEntity Validation { get; set; } = null!;
    }
    
    public class PolicyValidationEntityConfiguration : IEntityTypeConfiguration<PolicyValidationEntity>
    {
        public void Configure(EntityTypeBuilder<PolicyValidationEntity> builder)
        {
            builder.HasIndex(e => e.PolicyId);
            builder.HasIndex(e => e.ValidatedAt);
            
            builder.HasOne(e => e.Policy)
                   .WithMany(p => p.Validations)
                   .HasForeignKey(e => e.PolicyId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class ValidationErrorEntityConfiguration : IEntityTypeConfiguration<ValidationErrorEntity>
    {
        public void Configure(EntityTypeBuilder<ValidationErrorEntity> builder)
        {
            builder.HasIndex(e => e.ValidationId);
            
            builder.HasOne(e => e.Validation)
                   .WithMany(v => v.Errors)
                   .HasForeignKey(e => e.ValidationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class ValidationWarningEntityConfiguration : IEntityTypeConfiguration<ValidationWarningEntity>
    {
        public void Configure(EntityTypeBuilder<ValidationWarningEntity> builder)
        {
            builder.HasIndex(e => e.ValidationId);
            
            builder.HasOne(e => e.Validation)
                   .WithMany(v => v.Warnings)
                   .HasForeignKey(e => e.ValidationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

#endregion

#region Access List Entities

/// <summary>
/// Access list authorization entity
/// </summary>
[Table("AccessListAuthorizations")]
public class AccessListAuthorizationEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public int SubjectPartyId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ResourceId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    public DateTime RequestTime { get; set; }
    public bool IsAuthorized { get; set; }
    
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    public DateTime ValidatedAt { get; set; }
    
    // Navigation properties
    public List<AccessListEntity> AccessLists { get; set; } = new();
    public List<AccessListResourceAttributeEntity> ResourceAttributes { get; set; } = new();
    
    /// <summary>
    /// Access list entity
    /// </summary>
    [Table("AccessLists")]
    public class AccessListEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid AuthorizationId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string AccessListName { get; set; } = string.Empty;
        
        // Navigation properties
        public AccessListAuthorizationEntity Authorization { get; set; } = null!;
    }
    
    /// <summary>
    /// Access list resource attribute entity
    /// </summary>
    [Table("AccessListResourceAttributes")]
    public class AccessListResourceAttributeEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid AuthorizationId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string AttributeId { get; set; } = string.Empty;
        
        [Required]
        public string AttributeValue { get; set; } = string.Empty;
        
        public int MatchType { get; set; } // AttributeMatchType enum
        
        [MaxLength(100)]
        public string? DataType { get; set; }
        
        // Navigation properties
        public AccessListAuthorizationEntity Authorization { get; set; } = null!;
    }
    
    public class AccessListAuthorizationEntityConfiguration : IEntityTypeConfiguration<AccessListAuthorizationEntity>
    {
        public void Configure(EntityTypeBuilder<AccessListAuthorizationEntity> builder)
        {
            builder.HasIndex(e => new { e.SubjectPartyId, e.ResourceId, e.Action });
            builder.HasIndex(e => e.RequestTime);
            builder.HasIndex(e => e.ValidatedAt);
        }
    }
    
    public class AccessListEntityConfiguration : IEntityTypeConfiguration<AccessListEntity>
    {
        public void Configure(EntityTypeBuilder<AccessListEntity> builder)
        {
            builder.HasIndex(e => e.AuthorizationId);
            
            builder.HasOne(e => e.Authorization)
                   .WithMany(a => a.AccessLists)
                   .HasForeignKey(e => e.AuthorizationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class AccessListResourceAttributeEntityConfiguration : IEntityTypeConfiguration<AccessListResourceAttributeEntity>
    {
        public void Configure(EntityTypeBuilder<AccessListResourceAttributeEntity> builder)
        {
            builder.HasIndex(e => new { e.AuthorizationId, e.AttributeId });
            
            builder.HasOne(e => e.Authorization)
                   .WithMany(a => a.ResourceAttributes)
                   .HasForeignKey(e => e.AuthorizationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

#endregion

#region Performance Test Entities

/// <summary>
/// Authorization performance test entity
/// </summary>
[Table("PerformanceTests")]
public class AuthorizationPerformanceTestEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public int NumberOfRequests { get; set; }
    public int ConcurrentUsers { get; set; }
    public bool IncludeComplexPolicies { get; set; }
    public bool IncludeDelegations { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    public int? TotalRequests { get; set; }
    public int? SuccessfulRequests { get; set; }
    public int? FailedRequests { get; set; }
    public double? AverageResponseTimeMs { get; set; }
    public double? MinResponseTimeMs { get; set; }
    public double? MaxResponseTimeMs { get; set; }
    public double? ThroughputRequestsPerSecond { get; set; }
    
    // Navigation properties
    public List<PerformanceTestScenarioEntity> TestScenarios { get; set; } = new();
    public List<PerformanceMetricEntity> Metrics { get; set; } = new();
    
    /// <summary>
    /// Performance test scenario entity
    /// </summary>
    [Table("PerformanceTestScenarios")]
    public class PerformanceTestScenarioEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid TestId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Scenario { get; set; } = string.Empty;
        
        // Navigation properties
        public AuthorizationPerformanceTestEntity Test { get; set; } = null!;
    }
    
    /// <summary>
    /// Performance metric entity
    /// </summary>
    [Table("PerformanceMetrics")]
    public class PerformanceMetricEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid TestId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Scenario { get; set; } = string.Empty;
        
        public double ResponseTimeMs { get; set; }
        public bool Success { get; set; }
        
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        // Navigation properties
        public AuthorizationPerformanceTestEntity Test { get; set; } = null!;
    }
    
    public class AuthorizationPerformanceTestEntityConfiguration : IEntityTypeConfiguration<AuthorizationPerformanceTestEntity>
    {
        public void Configure(EntityTypeBuilder<AuthorizationPerformanceTestEntity> builder)
        {
            builder.HasIndex(e => e.StartTime);
            builder.HasIndex(e => e.EndTime);
        }
    }
    
    public class PerformanceTestScenarioEntityConfiguration : IEntityTypeConfiguration<PerformanceTestScenarioEntity>
    {
        public void Configure(EntityTypeBuilder<PerformanceTestScenarioEntity> builder)
        {
            builder.HasIndex(e => e.TestId);
            
            builder.HasOne(e => e.Test)
                   .WithMany(t => t.TestScenarios)
                   .HasForeignKey(e => e.TestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class PerformanceMetricEntityConfiguration : IEntityTypeConfiguration<PerformanceMetricEntity>
    {
        public void Configure(EntityTypeBuilder<PerformanceMetricEntity> builder)
        {
            builder.HasIndex(e => new { e.TestId, e.Timestamp });
            builder.HasIndex(e => e.Scenario);
            
            builder.HasOne(e => e.Test)
                   .WithMany(t => t.Metrics)
                   .HasForeignKey(e => e.TestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

#endregion