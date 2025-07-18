using System.Text.Json;
using Altinn.Authorization.Api.Contracts.Authorization;
using Altinn.Authorization.Core.Models;
using Altinn.Authorization.Persistence.Entities;
using Altinn.Authorization.Shared;

namespace Altinn.Authorization.Application.Mappers;

#region API Mappers (DTO ↔ Model)

/// <summary>
/// Maps between authorization request DTOs and models
/// </summary>
public static class AuthorizationRequestApiMapper
{
    public static AuthorizationRequestModel ToModel(AuthorizationRequestDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new AuthorizationRequestModel(
            subjects: dto.Subjects.Select(ToModel).ToList(),
            actions: dto.Actions.Select(ToModel).ToList(),
            resources: dto.Resources.Select(ToModel).ToList(),
            environment: dto.Environment?.Select(ToModel).ToList()
        );
    }

    public static AuthorizationRequestDto ToDto(AuthorizationRequestModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new AuthorizationRequestDto
        {
            Subjects = model.Subjects.Select(ToDto).ToList(),
            Actions = model.Actions.Select(ToDto).ToList(),
            Resources = model.Resources.Select(ToDto).ToList(),
            Environment = model.Environment.Select(ToDto).ToList()
        };
    }

    private static AuthorizationRequestModel.Subject ToModel(AuthorizationRequestDto.SubjectDto dto)
    {
        return new AuthorizationRequestModel.Subject(
            dto.Id,
            dto.Attributes.Select(ToModel).ToList()
        );
    }

    private static AuthorizationRequestDto.SubjectDto ToDto(AuthorizationRequestModel.Subject model)
    {
        return new AuthorizationRequestDto.SubjectDto
        {
            Id = model.Id,
            Attributes = model.Attributes.Select(ToDto).ToList()
        };
    }

    private static AuthorizationRequestModel.Action ToModel(AuthorizationRequestDto.ActionDto dto)
    {
        return new AuthorizationRequestModel.Action(
            dto.Id,
            dto.Attributes.Select(ToModel).ToList()
        );
    }

    private static AuthorizationRequestDto.ActionDto ToDto(AuthorizationRequestModel.Action model)
    {
        return new AuthorizationRequestDto.ActionDto
        {
            Id = model.Id,
            Attributes = model.Attributes.Select(ToDto).ToList()
        };
    }

    private static AuthorizationRequestModel.Resource ToModel(AuthorizationRequestDto.ResourceDto dto)
    {
        return new AuthorizationRequestModel.Resource(
            dto.Id,
            dto.Attributes.Select(ToModel).ToList()
        );
    }

    private static AuthorizationRequestDto.ResourceDto ToDto(AuthorizationRequestModel.Resource model)
    {
        return new AuthorizationRequestDto.ResourceDto
        {
            Id = model.Id,
            Attributes = model.Attributes.Select(ToDto).ToList()
        };
    }

    private static AuthorizationRequestModel.EnvironmentAttribute ToModel(AuthorizationRequestDto.EnvironmentDto dto)
    {
        return new AuthorizationRequestModel.EnvironmentAttribute(
            dto.AttributeId,
            dto.Value,
            dto.DataType
        );
    }

    private static AuthorizationRequestDto.EnvironmentDto ToDto(AuthorizationRequestModel.EnvironmentAttribute model)
    {
        return new AuthorizationRequestDto.EnvironmentDto
        {
            AttributeId = model.AttributeId,
            Value = model.Value,
            DataType = model.DataType
        };
    }

    private static XacmlAttribute ToModel(AuthorizationRequestDto.SubjectDto.AttributeDto dto)
    {
        return new XacmlAttribute(dto.AttributeId, dto.Value, dto.DataType);
    }

    private static XacmlAttribute ToModel(AuthorizationRequestDto.ActionDto.AttributeDto dto)
    {
        return new XacmlAttribute(dto.AttributeId, dto.Value, dto.DataType);
    }

    private static XacmlAttribute ToModel(AuthorizationRequestDto.ResourceDto.AttributeDto dto)
    {
        return new XacmlAttribute(dto.AttributeId, dto.Value, dto.DataType);
    }

    private static AuthorizationRequestDto.SubjectDto.AttributeDto ToDto(XacmlAttribute model)
    {
        return new AuthorizationRequestDto.SubjectDto.AttributeDto
        {
            AttributeId = model.AttributeId,
            Value = model.Value,
            DataType = model.DataType
        };
    }
}

/// <summary>
/// Maps between authorization response DTOs and models
/// </summary>
public static class AuthorizationResponseApiMapper
{
    public static AuthorizationResponseDto ToDto(AuthorizationResponseModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new AuthorizationResponseDto
        {
            Decision = ToDto(model.Decision),
            Obligations = model.Obligations.Select(ToDto).ToList(),
            Advice = model.Advice.Select(ToDto).ToList(),
            Status = model.Status.Select(ToDto).ToList()
        };
    }

    private static DecisionDto ToDto(Decision decision)
    {
        return decision switch
        {
            Decision.Permit => DecisionDto.Permit,
            Decision.Deny => DecisionDto.Deny,
            Decision.Indeterminate => DecisionDto.Indeterminate,
            Decision.NotApplicable => DecisionDto.NotApplicable,
            _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, "Unknown decision")
        };
    }

    private static AuthorizationResponseDto.ObligationDto ToDto(AuthorizationResponseModel.Obligation model)
    {
        return new AuthorizationResponseDto.ObligationDto
        {
            Id = model.Id,
            AttributeAssignments = model.AttributeAssignments.Select(ToDto).ToList()
        };
    }

    private static AuthorizationResponseDto.AdviceDto ToDto(AuthorizationResponseModel.Advice model)
    {
        return new AuthorizationResponseDto.AdviceDto
        {
            Id = model.Id,
            AttributeAssignments = model.AttributeAssignments.Select(ToDto).ToList()
        };
    }

    private static AuthorizationResponseDto.StatusDto ToDto(AuthorizationResponseModel.Status model)
    {
        return new AuthorizationResponseDto.StatusDto
        {
            Code = model.Code,
            Message = model.Message,
            Detail = model.Detail
        };
    }

    private static AuthorizationResponseDto.ObligationDto.AttributeAssignmentDto ToDto(AuthorizationResponseModel.AttributeAssignment model)
    {
        return new AuthorizationResponseDto.ObligationDto.AttributeAssignmentDto
        {
            AttributeId = model.AttributeId,
            Value = model.Value,
            DataType = model.DataType
        };
    }
}

/// <summary>
/// Maps between policy DTOs and models
/// </summary>
public static class PolicyApiMapper
{
    public static PolicyModel ToModel(PolicyDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new PolicyModel(
            dto.PolicyId,
            dto.Version,
            dto.Content,
            ToModel(dto.Status),
            dto.Tags,
            null // Description not in DTO
        );
    }

    public static PolicyDto ToDto(PolicyModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new PolicyDto
        {
            PolicyId = model.PolicyId,
            Version = model.Version,
            Content = model.Content,
            Created = model.Created,
            Updated = model.Updated,
            Status = ToDto(model.Status),
            Tags = model.Tags.ToList()
        };
    }

    public static PolicyValidationDto ToDto(PolicyValidationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new PolicyValidationDto
        {
            IsValid = model.IsValid,
            Errors = model.Errors.Select(ToDto).ToList(),
            Warnings = model.Warnings.Select(ToDto).ToList()
        };
    }

    private static PolicyStatus ToModel(PolicyStatusDto status)
    {
        return status switch
        {
            PolicyStatusDto.Active => PolicyStatus.Active,
            PolicyStatusDto.Inactive => PolicyStatus.Inactive,
            PolicyStatusDto.Draft => PolicyStatus.Draft,
            PolicyStatusDto.Deprecated => PolicyStatus.Deprecated,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown policy status")
        };
    }

    private static PolicyStatusDto ToDto(PolicyStatus status)
    {
        return status switch
        {
            PolicyStatus.Active => PolicyStatusDto.Active,
            PolicyStatus.Inactive => PolicyStatusDto.Inactive,
            PolicyStatus.Draft => PolicyStatusDto.Draft,
            PolicyStatus.Deprecated => PolicyStatusDto.Deprecated,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown policy status")
        };
    }

    private static PolicyValidationDto.ValidationErrorDto ToDto(PolicyValidationModel.ValidationError model)
    {
        return new PolicyValidationDto.ValidationErrorDto
        {
            Code = model.Code,
            Message = model.Message,
            Location = model.Location,
            Line = model.Line,
            Column = model.Column
        };
    }

    private static PolicyValidationDto.ValidationWarningDto ToDto(PolicyValidationModel.ValidationWarning model)
    {
        return new PolicyValidationDto.ValidationWarningDto
        {
            Code = model.Code,
            Message = model.Message,
            Location = model.Location
        };
    }
}

/// <summary>
/// Maps between access list authorization DTOs and models
/// </summary>
public static class AccessListAuthorizationApiMapper
{
    public static AccessListAuthorizationModel ToModel(AccessListAuthorizationDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new AccessListAuthorizationModel(
            new PartyId(dto.SubjectPartyId),
            new ResourceId(dto.ResourceId),
            dto.Action,
            dto.ResourceAttributes?.Select(ToModel).ToList()
        );
    }

    public static AccessListAuthorizationResponseDto ToDto(AccessListAuthorizationResultModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new AccessListAuthorizationResponseDto
        {
            IsAuthorized = model.IsAuthorized,
            AccessLists = model.AccessLists.ToList(),
            Reason = model.Reason,
            ValidatedAt = model.ValidatedAt
        };
    }

    private static AttributeMatch ToModel(AttributeMatchDto dto)
    {
        return new AttributeMatch(
            dto.Id,
            dto.Value,
            ToModel(dto.Type),
            dto.DataType
        );
    }

    private static AttributeMatchType ToModel(AttributeMatchTypeDto type)
    {
        return type switch
        {
            AttributeMatchTypeDto.Equals => AttributeMatchType.Equals,
            AttributeMatchTypeDto.Contains => AttributeMatchType.Contains,
            AttributeMatchTypeDto.StartsWith => AttributeMatchType.StartsWith,
            AttributeMatchTypeDto.EndsWith => AttributeMatchType.EndsWith,
            AttributeMatchTypeDto.GreaterThan => AttributeMatchType.GreaterThan,
            AttributeMatchTypeDto.LessThan => AttributeMatchType.LessThan,
            AttributeMatchTypeDto.GreaterThanOrEqual => AttributeMatchType.GreaterThanOrEqual,
            AttributeMatchTypeDto.LessThanOrEqual => AttributeMatchType.LessThanOrEqual,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown attribute match type")
        };
    }
}

/// <summary>
/// Maps between performance test DTOs and models
/// </summary>
public static class PerformanceTestApiMapper
{
    public static AuthorizationPerformanceTestModel ToModel(AuthorizationPerformanceTestDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new AuthorizationPerformanceTestModel(
            dto.NumberOfRequests,
            dto.ConcurrentUsers,
            dto.IncludeComplexPolicies,
            dto.IncludeDelegations,
            dto.TestScenarios
        );
    }

    public static AuthorizationPerformanceResultDto ToDto(AuthorizationPerformanceResultModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new AuthorizationPerformanceResultDto
        {
            TotalRequests = model.TotalRequests,
            SuccessfulRequests = model.SuccessfulRequests,
            FailedRequests = model.FailedRequests,
            AverageResponseTimeMs = model.AverageResponseTimeMs,
            MinResponseTimeMs = model.MinResponseTimeMs,
            MaxResponseTimeMs = model.MaxResponseTimeMs,
            ThroughputRequestsPerSecond = model.ThroughputRequestsPerSecond,
            TestStarted = model.TestStarted,
            TestCompleted = model.TestCompleted,
            TestDuration = model.TestDuration,
            Metrics = model.Metrics.Select(ToDto).ToList()
        };
    }

    private static AuthorizationPerformanceResultDto.PerformanceMetricDto ToDto(AuthorizationPerformanceResultModel.PerformanceMetric model)
    {
        return new AuthorizationPerformanceResultDto.PerformanceMetricDto
        {
            Scenario = model.Scenario,
            ResponseTimeMs = model.ResponseTimeMs,
            Success = model.Success,
            ErrorMessage = model.ErrorMessage,
            Timestamp = model.Timestamp
        };
    }
}

#endregion

#region Persistence Mappers (Model ↔ Entity)

/// <summary>
/// Maps between authorization request models and entities
/// </summary>
public static class AuthorizationRequestPersistenceMapper
{
    public static AuthorizationRequestEntity ToEntity(AuthorizationRequestModel model, string requestId)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new AuthorizationRequestEntity
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            RequestContent = JsonSerializer.Serialize(model),
            RequestTime = model.RequestTime,
            Status = "Pending",
            Subjects = model.Subjects.Select(s => ToEntity(s, Guid.NewGuid())).ToList(),
            Actions = model.Actions.Select(a => ToEntity(a, Guid.NewGuid())).ToList(),
            Resources = model.Resources.Select(r => ToEntity(r, Guid.NewGuid())).ToList(),
            Environment = model.Environment.Select(e => ToEntity(e, Guid.NewGuid())).ToList()
        };
    }

    public static void UpdateEntityWithResponse(AuthorizationRequestEntity entity, AuthorizationResponseModel response)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(response);

        entity.ResponseTime = response.ResponseTime;
        entity.Decision = response.Decision.ToString();
        entity.Status = "Completed";
        entity.ResponseContent = JsonSerializer.Serialize(response);
    }

    private static AuthorizationSubjectEntity ToEntity(AuthorizationRequestModel.Subject model, Guid requestId)
    {
        return new AuthorizationSubjectEntity
        {
            Id = Guid.NewGuid(),
            AuthorizationRequestId = requestId,
            SubjectId = model.Id,
            Attributes = model.Attributes.Select(a => ToEntity(a, Guid.NewGuid())).ToList()
        };
    }

    private static AuthorizationActionEntity ToEntity(AuthorizationRequestModel.Action model, Guid requestId)
    {
        return new AuthorizationActionEntity
        {
            Id = Guid.NewGuid(),
            AuthorizationRequestId = requestId,
            ActionId = model.Id,
            Attributes = model.Attributes.Select(a => ToEntity(a, Guid.NewGuid())).ToList()
        };
    }

    private static AuthorizationResourceEntity ToEntity(AuthorizationRequestModel.Resource model, Guid requestId)
    {
        return new AuthorizationResourceEntity
        {
            Id = Guid.NewGuid(),
            AuthorizationRequestId = requestId,
            ResourceId = model.Id,
            Attributes = model.Attributes.Select(a => ToEntity(a, Guid.NewGuid())).ToList()
        };
    }

    private static AuthorizationEnvironmentEntity ToEntity(AuthorizationRequestModel.EnvironmentAttribute model, Guid requestId)
    {
        return new AuthorizationEnvironmentEntity
        {
            Id = Guid.NewGuid(),
            AuthorizationRequestId = requestId,
            AttributeId = model.AttributeId,
            AttributeValue = model.Value,
            DataType = model.DataType
        };
    }

    private static AuthorizationSubjectEntity.SubjectAttributeEntity ToEntity(XacmlAttribute model, Guid subjectId)
    {
        return new AuthorizationSubjectEntity.SubjectAttributeEntity
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            AttributeId = model.AttributeId,
            AttributeValue = model.Value,
            DataType = model.DataType,
            Issuer = model.Issuer
        };
    }

    private static AuthorizationActionEntity.ActionAttributeEntity ToEntity(XacmlAttribute model, Guid actionId)
    {
        return new AuthorizationActionEntity.ActionAttributeEntity
        {
            Id = Guid.NewGuid(),
            ActionId = actionId,
            AttributeId = model.AttributeId,
            AttributeValue = model.Value,
            DataType = model.DataType
        };
    }

    private static AuthorizationResourceEntity.ResourceAttributeEntity ToEntity(XacmlAttribute model, Guid resourceId)
    {
        return new AuthorizationResourceEntity.ResourceAttributeEntity
        {
            Id = Guid.NewGuid(),
            ResourceId = resourceId,
            AttributeId = model.AttributeId,
            AttributeValue = model.Value,
            DataType = model.DataType
        };
    }
}

/// <summary>
/// Maps between policy models and entities
/// </summary>
public static class PolicyPersistenceMapper
{
    public static PolicyEntity ToEntity(PolicyModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new PolicyEntity
        {
            Id = Guid.NewGuid(),
            PolicyId = model.PolicyId,
            Version = model.Version,
            Content = model.Content,
            Created = model.Created,
            Updated = model.Updated,
            Status = (int)model.Status,
            Description = model.Description,
            Tags = model.Tags.Select(tag => new PolicyEntity.PolicyTagEntity
            {
                Id = Guid.NewGuid(),
                Tag = tag
            }).ToList()
        };
    }

    public static PolicyModel ToModel(PolicyEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new PolicyModel(
            entity.PolicyId,
            entity.Version,
            entity.Content,
            (PolicyStatus)entity.Status,
            entity.Tags.Select(t => t.Tag).ToList(),
            entity.Description
        );
    }

    public static PolicyValidationEntity ToEntity(PolicyValidationModel model, Guid policyId)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new PolicyValidationEntity
        {
            Id = Guid.NewGuid(),
            PolicyId = policyId,
            IsValid = model.IsValid,
            ValidatedAt = model.ValidatedAt,
            Errors = model.Errors.Select(e => new PolicyValidationEntity.ValidationErrorEntity
            {
                Id = Guid.NewGuid(),
                Code = e.Code,
                Message = e.Message,
                Location = e.Location,
                Line = e.Line,
                Column = e.Column
            }).ToList(),
            Warnings = model.Warnings.Select(w => new PolicyValidationEntity.ValidationWarningEntity
            {
                Id = Guid.NewGuid(),
                Code = w.Code,
                Message = w.Message,
                Location = w.Location
            }).ToList()
        };
    }
}

/// <summary>
/// Maps between performance test models and entities
/// </summary>
public static class PerformanceTestPersistenceMapper
{
    public static AuthorizationPerformanceTestEntity ToEntity(AuthorizationPerformanceTestModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new AuthorizationPerformanceTestEntity
        {
            Id = Guid.NewGuid(),
            NumberOfRequests = model.NumberOfRequests,
            ConcurrentUsers = model.ConcurrentUsers,
            IncludeComplexPolicies = model.IncludeComplexPolicies,
            IncludeDelegations = model.IncludeDelegations,
            StartTime = model.StartTime,
            TestScenarios = model.TestScenarios.Select(scenario => new AuthorizationPerformanceTestEntity.PerformanceTestScenarioEntity
            {
                Id = Guid.NewGuid(),
                Scenario = scenario
            }).ToList()
        };
    }

    public static void UpdateEntityWithResults(AuthorizationPerformanceTestEntity entity, AuthorizationPerformanceResultModel results)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(results);

        entity.EndTime = results.TestCompleted;
        entity.TotalRequests = results.TotalRequests;
        entity.SuccessfulRequests = results.SuccessfulRequests;
        entity.FailedRequests = results.FailedRequests;
        entity.AverageResponseTimeMs = results.AverageResponseTimeMs;
        entity.MinResponseTimeMs = results.MinResponseTimeMs;
        entity.MaxResponseTimeMs = results.MaxResponseTimeMs;
        entity.ThroughputRequestsPerSecond = results.ThroughputRequestsPerSecond;

        entity.Metrics.AddRange(results.Metrics.Select(m => new AuthorizationPerformanceTestEntity.PerformanceMetricEntity
        {
            Id = Guid.NewGuid(),
            Scenario = m.Scenario,
            ResponseTimeMs = m.ResponseTimeMs,
            Success = m.Success,
            ErrorMessage = m.ErrorMessage,
            Timestamp = m.Timestamp
        }));
    }
}

#endregion

#region Extension Methods for Fluent API

/// <summary>
/// Extension methods for authorization request mapping
/// </summary>
public static class AuthorizationRequestMappingExtensions
{
    // API → Core
    public static AuthorizationRequestModel ToModel(this AuthorizationRequestDto dto)
        => AuthorizationRequestApiMapper.ToModel(dto);

    // Core → API
    public static AuthorizationRequestDto ToDto(this AuthorizationRequestModel model)
        => AuthorizationRequestApiMapper.ToDto(model);

    public static AuthorizationResponseDto ToDto(this AuthorizationResponseModel model)
        => AuthorizationResponseApiMapper.ToDto(model);

    // Core → Database
    public static AuthorizationRequestEntity ToEntity(this AuthorizationRequestModel model, string requestId)
        => AuthorizationRequestPersistenceMapper.ToEntity(model, requestId);
}

/// <summary>
/// Extension methods for policy mapping
/// </summary>
public static class PolicyMappingExtensions
{
    // API → Core
    public static PolicyModel ToModel(this PolicyDto dto)
        => PolicyApiMapper.ToModel(dto);

    // Core → API
    public static PolicyDto ToDto(this PolicyModel model)
        => PolicyApiMapper.ToDto(model);

    public static PolicyValidationDto ToDto(this PolicyValidationModel model)
        => PolicyApiMapper.ToDto(model);

    // Core → Database
    public static PolicyEntity ToEntity(this PolicyModel model)
        => PolicyPersistenceMapper.ToEntity(model);

    // Database → Core
    public static PolicyModel ToModel(this PolicyEntity entity)
        => PolicyPersistenceMapper.ToModel(entity);
}

/// <summary>
/// Extension methods for access list authorization mapping
/// </summary>
public static class AccessListAuthorizationMappingExtensions
{
    // API → Core
    public static AccessListAuthorizationModel ToModel(this AccessListAuthorizationDto dto)
        => AccessListAuthorizationApiMapper.ToModel(dto);

    // Core → API
    public static AccessListAuthorizationResponseDto ToDto(this AccessListAuthorizationResultModel model)
        => AccessListAuthorizationApiMapper.ToDto(model);
}

/// <summary>
/// Extension methods for performance test mapping
/// </summary>
public static class PerformanceTestMappingExtensions
{
    // API → Core
    public static AuthorizationPerformanceTestModel ToModel(this AuthorizationPerformanceTestDto dto)
        => PerformanceTestApiMapper.ToModel(dto);

    // Core → API
    public static AuthorizationPerformanceResultDto ToDto(this AuthorizationPerformanceResultModel model)
        => PerformanceTestApiMapper.ToDto(model);

    // Core → Database
    public static AuthorizationPerformanceTestEntity ToEntity(this AuthorizationPerformanceTestModel model)
        => PerformanceTestPersistenceMapper.ToEntity(model);
}

#endregion