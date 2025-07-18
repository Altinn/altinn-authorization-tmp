using Altinn.Authorization.Shared;

namespace Altinn.Authorization.Core.Models;

#region XACML Models

/// <summary>
/// XACML authorization request model
/// </summary>
public class AuthorizationRequestModel
{
    public List<Subject> Subjects { get; private set; }
    public List<Action> Actions { get; private set; }
    public List<Resource> Resources { get; private set; }
    public List<EnvironmentAttribute> Environment { get; private set; }
    public DateTime RequestTime { get; private set; }
    public string? RequestId { get; private set; }

    private AuthorizationRequestModel()
    {
        Subjects = new List<Subject>();
        Actions = new List<Action>();
        Resources = new List<Resource>();
        Environment = new List<EnvironmentAttribute>();
    }

    public AuthorizationRequestModel(
        List<Subject> subjects,
        List<Action> actions,
        List<Resource> resources,
        List<EnvironmentAttribute>? environment = null,
        string? requestId = null)
    {
        if (subjects == null || !subjects.Any())
            throw new ArgumentException("At least one subject is required", nameof(subjects));
        if (actions == null || !actions.Any())
            throw new ArgumentException("At least one action is required", nameof(actions));
        if (resources == null || !resources.Any())
            throw new ArgumentException("At least one resource is required", nameof(resources));

        Subjects = subjects;
        Actions = actions;
        Resources = resources;
        Environment = environment ?? new List<EnvironmentAttribute>();
        RequestTime = DateTime.UtcNow;
        RequestId = requestId ?? Guid.NewGuid().ToString();
    }

    /// <summary>
    /// XACML Subject
    /// </summary>
    public class Subject
    {
        public string Id { get; private set; }
        public List<XacmlAttribute> Attributes { get; private set; }

        private Subject() 
        { 
            Attributes = new List<XacmlAttribute>();
        }

        public Subject(string id, List<XacmlAttribute> attributes)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Subject ID cannot be null or empty", nameof(id));

            Id = id;
            Attributes = attributes ?? new List<XacmlAttribute>();
        }

        public void AddAttribute(XacmlAttribute attribute)
        {
            if (Attributes.Any(a => a.AttributeId == attribute.AttributeId))
                throw new InvalidOperationException($"Attribute {attribute.AttributeId} already exists");

            Attributes.Add(attribute);
        }

        public XacmlAttribute? GetAttribute(string attributeId)
        {
            return Attributes.FirstOrDefault(a => a.AttributeId == attributeId);
        }
    }

    /// <summary>
    /// XACML Action
    /// </summary>
    public class Action
    {
        public string Id { get; private set; }
        public List<XacmlAttribute> Attributes { get; private set; }

        private Action() 
        { 
            Attributes = new List<XacmlAttribute>();
        }

        public Action(string id, List<XacmlAttribute> attributes)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Action ID cannot be null or empty", nameof(id));

            Id = id;
            Attributes = attributes ?? new List<XacmlAttribute>();
        }

        public void AddAttribute(XacmlAttribute attribute)
        {
            if (Attributes.Any(a => a.AttributeId == attribute.AttributeId))
                throw new InvalidOperationException($"Attribute {attribute.AttributeId} already exists");

            Attributes.Add(attribute);
        }
    }

    /// <summary>
    /// XACML Resource
    /// </summary>
    public class Resource
    {
        public string Id { get; private set; }
        public List<XacmlAttribute> Attributes { get; private set; }

        private Resource() 
        { 
            Attributes = new List<XacmlAttribute>();
        }

        public Resource(string id, List<XacmlAttribute> attributes)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Resource ID cannot be null or empty", nameof(id));

            Id = id;
            Attributes = attributes ?? new List<XacmlAttribute>();
        }

        public void AddAttribute(XacmlAttribute attribute)
        {
            if (Attributes.Any(a => a.AttributeId == attribute.AttributeId))
                throw new InvalidOperationException($"Attribute {attribute.AttributeId} already exists");

            Attributes.Add(attribute);
        }

        public ResourceId GetResourceId()
        {
            var resourceIdAttr = GetAttribute("urn:altinn:resource");
            if (resourceIdAttr == null)
                throw new InvalidOperationException("Resource ID attribute not found");

            return new ResourceId(resourceIdAttr.Value);
        }

        private XacmlAttribute? GetAttribute(string attributeId)
        {
            return Attributes.FirstOrDefault(a => a.AttributeId == attributeId);
        }
    }

    /// <summary>
    /// XACML Environment Attribute
    /// </summary>
    public class EnvironmentAttribute
    {
        public string AttributeId { get; private set; }
        public string Value { get; private set; }
        public string? DataType { get; private set; }

        private EnvironmentAttribute() { }

        public EnvironmentAttribute(string attributeId, string value, string? dataType = null)
        {
            if (string.IsNullOrWhiteSpace(attributeId))
                throw new ArgumentException("Attribute ID cannot be null or empty", nameof(attributeId));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or empty", nameof(value));

            AttributeId = attributeId;
            Value = value;
            DataType = dataType;
        }
    }
}

/// <summary>
/// XACML authorization response model
/// </summary>
public class AuthorizationResponseModel
{
    public Decision Decision { get; private set; }
    public List<Obligation> Obligations { get; private set; }
    public List<Advice> Advice { get; private set; }
    public List<Status> Status { get; private set; }
    public DateTime ResponseTime { get; private set; }
    public string? RequestId { get; private set; }

    private AuthorizationResponseModel()
    {
        Obligations = new List<Obligation>();
        Advice = new List<Advice>();
        Status = new List<Status>();
    }

    public AuthorizationResponseModel(
        Decision decision,
        List<Obligation>? obligations = null,
        List<Advice>? advice = null,
        List<Status>? status = null,
        string? requestId = null)
    {
        Decision = decision;
        Obligations = obligations ?? new List<Obligation>();
        Advice = advice ?? new List<Advice>();
        Status = status ?? new List<Status>();
        ResponseTime = DateTime.UtcNow;
        RequestId = requestId;
    }

    public static AuthorizationResponseModel Permit(string? requestId = null)
    {
        return new AuthorizationResponseModel(Decision.Permit, requestId: requestId);
    }

    public static AuthorizationResponseModel Deny(string? requestId = null, string? reason = null)
    {
        var status = reason != null ? new List<Status> { new("Deny", reason) } : null;
        return new AuthorizationResponseModel(Decision.Deny, status: status, requestId: requestId);
    }

    public static AuthorizationResponseModel Indeterminate(string? requestId = null, string? reason = null)
    {
        var status = reason != null ? new List<Status> { new("Indeterminate", reason) } : null;
        return new AuthorizationResponseModel(Decision.Indeterminate, status: status, requestId: requestId);
    }

    public static AuthorizationResponseModel NotApplicable(string? requestId = null)
    {
        return new AuthorizationResponseModel(Decision.NotApplicable, requestId: requestId);
    }

    /// <summary>
    /// XACML Obligation
    /// </summary>
    public class Obligation
    {
        public string Id { get; private set; }
        public List<AttributeAssignment> AttributeAssignments { get; private set; }

        private Obligation() 
        { 
            AttributeAssignments = new List<AttributeAssignment>();
        }

        public Obligation(string id, List<AttributeAssignment>? attributeAssignments = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Obligation ID cannot be null or empty", nameof(id));

            Id = id;
            AttributeAssignments = attributeAssignments ?? new List<AttributeAssignment>();
        }
    }

    /// <summary>
    /// XACML Advice
    /// </summary>
    public class Advice
    {
        public string Id { get; private set; }
        public List<AttributeAssignment> AttributeAssignments { get; private set; }

        private Advice() 
        { 
            AttributeAssignments = new List<AttributeAssignment>();
        }

        public Advice(string id, List<AttributeAssignment>? attributeAssignments = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Advice ID cannot be null or empty", nameof(id));

            Id = id;
            AttributeAssignments = attributeAssignments ?? new List<AttributeAssignment>();
        }
    }

    /// <summary>
    /// XACML Attribute Assignment
    /// </summary>
    public class AttributeAssignment
    {
        public string AttributeId { get; private set; }
        public string Value { get; private set; }
        public string? DataType { get; private set; }

        private AttributeAssignment() { }

        public AttributeAssignment(string attributeId, string value, string? dataType = null)
        {
            if (string.IsNullOrWhiteSpace(attributeId))
                throw new ArgumentException("Attribute ID cannot be null or empty", nameof(attributeId));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or empty", nameof(value));

            AttributeId = attributeId;
            Value = value;
            DataType = dataType;
        }
    }

    /// <summary>
    /// XACML Status
    /// </summary>
    public class Status
    {
        public string Code { get; private set; }
        public string? Message { get; private set; }
        public string? Detail { get; private set; }

        private Status() { }

        public Status(string code, string? message = null, string? detail = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Status code cannot be null or empty", nameof(code));

            Code = code;
            Message = message;
            Detail = detail;
        }
    }
}

/// <summary>
/// XACML Attribute
/// </summary>
public class XacmlAttribute : IEquatable<XacmlAttribute>
{
    public string AttributeId { get; private set; }
    public string Value { get; private set; }
    public string? DataType { get; private set; }
    public string? Issuer { get; private set; }

    private XacmlAttribute() { }

    public XacmlAttribute(string attributeId, string value, string? dataType = null, string? issuer = null)
    {
        if (string.IsNullOrWhiteSpace(attributeId))
            throw new ArgumentException("Attribute ID cannot be null or empty", nameof(attributeId));
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));

        AttributeId = attributeId;
        Value = value;
        DataType = dataType;
        Issuer = issuer;
    }

    public bool Equals(XacmlAttribute? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return AttributeId == other.AttributeId && Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as XacmlAttribute);
    public override int GetHashCode() => HashCode.Combine(AttributeId, Value);
}

#endregion

#region Policy Models

/// <summary>
/// Policy model for XACML policies
/// </summary>
public class PolicyModel
{
    public string PolicyId { get; private set; }
    public string Version { get; private set; }
    public string Content { get; private set; } // XACML policy as XML
    public DateTime Created { get; private set; }
    public DateTime? Updated { get; private set; }
    public PolicyStatus Status { get; private set; }
    public List<string> Tags { get; private set; }
    public string? Description { get; private set; }

    private PolicyModel() 
    { 
        Tags = new List<string>();
    }

    public PolicyModel(
        string policyId,
        string version,
        string content,
        PolicyStatus status = PolicyStatus.Active,
        List<string>? tags = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(policyId))
            throw new ArgumentException("Policy ID cannot be null or empty", nameof(policyId));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version cannot be null or empty", nameof(version));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty", nameof(content));

        PolicyId = policyId;
        Version = version;
        Content = content;
        Status = status;
        Tags = tags ?? new List<string>();
        Description = description;
        Created = DateTime.UtcNow;
    }

    public void UpdateContent(string newContent, string newVersion)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Content cannot be null or empty", nameof(newContent));
        if (string.IsNullOrWhiteSpace(newVersion))
            throw new ArgumentException("Version cannot be null or empty", nameof(newVersion));

        Content = newContent;
        Version = newVersion;
        Updated = DateTime.UtcNow;
    }

    public void UpdateStatus(PolicyStatus newStatus)
    {
        Status = newStatus;
        Updated = DateTime.UtcNow;
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be null or empty", nameof(tag));

        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
            Updated = DateTime.UtcNow;
        }
    }

    public void RemoveTag(string tag)
    {
        if (Tags.Remove(tag))
        {
            Updated = DateTime.UtcNow;
        }
    }

    public bool IsActive => Status == PolicyStatus.Active;
}

/// <summary>
/// Policy validation result model
/// </summary>
public class PolicyValidationModel
{
    public bool IsValid { get; private set; }
    public List<ValidationError> Errors { get; private set; }
    public List<ValidationWarning> Warnings { get; private set; }
    public DateTime ValidatedAt { get; private set; }

    private PolicyValidationModel()
    {
        Errors = new List<ValidationError>();
        Warnings = new List<ValidationWarning>();
    }

    public PolicyValidationModel(
        bool isValid,
        List<ValidationError>? errors = null,
        List<ValidationWarning>? warnings = null)
    {
        IsValid = isValid;
        Errors = errors ?? new List<ValidationError>();
        Warnings = warnings ?? new List<ValidationWarning>();
        ValidatedAt = DateTime.UtcNow;
    }

    public static PolicyValidationModel Valid()
    {
        return new PolicyValidationModel(true);
    }

    public static PolicyValidationModel Invalid(List<ValidationError> errors)
    {
        return new PolicyValidationModel(false, errors);
    }

    /// <summary>
    /// Policy validation error
    /// </summary>
    public class ValidationError
    {
        public string Code { get; private set; }
        public string Message { get; private set; }
        public string? Location { get; private set; }
        public int? Line { get; private set; }
        public int? Column { get; private set; }

        private ValidationError() { }

        public ValidationError(string code, string message, string? location = null, int? line = null, int? column = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Code cannot be null or empty", nameof(code));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));

            Code = code;
            Message = message;
            Location = location;
            Line = line;
            Column = column;
        }
    }

    /// <summary>
    /// Policy validation warning
    /// </summary>
    public class ValidationWarning
    {
        public string Code { get; private set; }
        public string Message { get; private set; }
        public string? Location { get; private set; }

        private ValidationWarning() { }

        public ValidationWarning(string code, string message, string? location = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Code cannot be null or empty", nameof(code));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));

            Code = code;
            Message = message;
            Location = location;
        }
    }
}

#endregion

#region Access List Models

/// <summary>
/// Access list authorization model
/// </summary>
public class AccessListAuthorizationModel
{
    public PartyId SubjectPartyId { get; private set; }
    public ResourceId ResourceId { get; private set; }
    public string Action { get; private set; }
    public List<AttributeMatch> ResourceAttributes { get; private set; }
    public DateTime RequestTime { get; private set; }

    private AccessListAuthorizationModel()
    {
        ResourceAttributes = new List<AttributeMatch>();
    }

    public AccessListAuthorizationModel(
        PartyId subjectPartyId,
        ResourceId resourceId,
        string action,
        List<AttributeMatch>? resourceAttributes = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or empty", nameof(action));

        SubjectPartyId = subjectPartyId;
        ResourceId = resourceId;
        Action = action;
        ResourceAttributes = resourceAttributes ?? new List<AttributeMatch>();
        RequestTime = DateTime.UtcNow;
    }
}

/// <summary>
/// Access list authorization result model
/// </summary>
public class AccessListAuthorizationResultModel
{
    public bool IsAuthorized { get; private set; }
    public List<string> AccessLists { get; private set; }
    public string? Reason { get; private set; }
    public DateTime ValidatedAt { get; private set; }

    private AccessListAuthorizationResultModel()
    {
        AccessLists = new List<string>();
    }

    public AccessListAuthorizationResultModel(
        bool isAuthorized,
        List<string>? accessLists = null,
        string? reason = null)
    {
        IsAuthorized = isAuthorized;
        AccessLists = accessLists ?? new List<string>();
        Reason = reason;
        ValidatedAt = DateTime.UtcNow;
    }

    public static AccessListAuthorizationResultModel Authorized(List<string> accessLists)
    {
        return new AccessListAuthorizationResultModel(true, accessLists);
    }

    public static AccessListAuthorizationResultModel Denied(string reason)
    {
        return new AccessListAuthorizationResultModel(false, reason: reason);
    }
}

#endregion

#region Performance Models

/// <summary>
/// Authorization performance test model
/// </summary>
public class AuthorizationPerformanceTestModel
{
    public int NumberOfRequests { get; private set; }
    public int ConcurrentUsers { get; private set; }
    public bool IncludeComplexPolicies { get; private set; }
    public bool IncludeDelegations { get; private set; }
    public List<string> TestScenarios { get; private set; }
    public DateTime StartTime { get; private set; }

    private AuthorizationPerformanceTestModel()
    {
        TestScenarios = new List<string>();
    }

    public AuthorizationPerformanceTestModel(
        int numberOfRequests,
        int concurrentUsers,
        bool includeComplexPolicies = false,
        bool includeDelegations = false,
        List<string>? testScenarios = null)
    {
        if (numberOfRequests <= 0)
            throw new ArgumentException("Number of requests must be greater than 0", nameof(numberOfRequests));
        if (concurrentUsers <= 0)
            throw new ArgumentException("Concurrent users must be greater than 0", nameof(concurrentUsers));

        NumberOfRequests = numberOfRequests;
        ConcurrentUsers = concurrentUsers;
        IncludeComplexPolicies = includeComplexPolicies;
        IncludeDelegations = includeDelegations;
        TestScenarios = testScenarios ?? new List<string>();
        StartTime = DateTime.UtcNow;
    }
}

/// <summary>
/// Authorization performance result model
/// </summary>
public class AuthorizationPerformanceResultModel
{
    public int TotalRequests { get; private set; }
    public int SuccessfulRequests { get; private set; }
    public int FailedRequests { get; private set; }
    public double AverageResponseTimeMs { get; private set; }
    public double MinResponseTimeMs { get; private set; }
    public double MaxResponseTimeMs { get; private set; }
    public double ThroughputRequestsPerSecond { get; private set; }
    public DateTime TestStarted { get; private set; }
    public DateTime TestCompleted { get; private set; }
    public TimeSpan TestDuration { get; private set; }
    public List<PerformanceMetric> Metrics { get; private set; }

    private AuthorizationPerformanceResultModel()
    {
        Metrics = new List<PerformanceMetric>();
    }

    public AuthorizationPerformanceResultModel(
        int totalRequests,
        int successfulRequests,
        int failedRequests,
        List<PerformanceMetric> metrics,
        DateTime testStarted,
        DateTime testCompleted)
    {
        TotalRequests = totalRequests;
        SuccessfulRequests = successfulRequests;
        FailedRequests = failedRequests;
        Metrics = metrics ?? new List<PerformanceMetric>();
        TestStarted = testStarted;
        TestCompleted = testCompleted;
        TestDuration = testCompleted - testStarted;

        CalculateStatistics();
    }

    private void CalculateStatistics()
    {
        if (!Metrics.Any())
        {
            AverageResponseTimeMs = 0;
            MinResponseTimeMs = 0;
            MaxResponseTimeMs = 0;
            ThroughputRequestsPerSecond = 0;
            return;
        }

        var responseTimes = Metrics.Select(m => m.ResponseTimeMs).ToList();
        AverageResponseTimeMs = responseTimes.Average();
        MinResponseTimeMs = responseTimes.Min();
        MaxResponseTimeMs = responseTimes.Max();
        ThroughputRequestsPerSecond = TestDuration.TotalSeconds > 0 ? TotalRequests / TestDuration.TotalSeconds : 0;
    }

    /// <summary>
    /// Individual performance metric
    /// </summary>
    public class PerformanceMetric
    {
        public string Scenario { get; private set; }
        public double ResponseTimeMs { get; private set; }
        public bool Success { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DateTime Timestamp { get; private set; }

        private PerformanceMetric() { }

        public PerformanceMetric(
            string scenario,
            double responseTimeMs,
            bool success,
            string? errorMessage = null)
        {
            if (string.IsNullOrWhiteSpace(scenario))
                throw new ArgumentException("Scenario cannot be null or empty", nameof(scenario));

            Scenario = scenario;
            ResponseTimeMs = responseTimeMs;
            Success = success;
            ErrorMessage = errorMessage;
            Timestamp = DateTime.UtcNow;
        }
    }
}

#endregion

#region Enums

public enum Decision
{
    Permit,
    Deny,
    Indeterminate,
    NotApplicable
}

public enum PolicyStatus
{
    Active,
    Inactive,
    Draft,
    Deprecated
}

public enum AuthorizationRequestDirection
{
    Inbound,
    Outbound
}

public enum AuthorizationRequestStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

#endregion

#region Exceptions

public class AuthorizationException : Exception
{
    public string? ErrorCode { get; }

    public AuthorizationException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }

    public AuthorizationException(string message, Exception innerException, string? errorCode = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public class PolicyValidationException : AuthorizationException
{
    public List<PolicyValidationModel.ValidationError> ValidationErrors { get; }

    public PolicyValidationException(string message, List<PolicyValidationModel.ValidationError> validationErrors) 
        : base(message, "POLICY_VALIDATION_ERROR")
    {
        ValidationErrors = validationErrors;
    }
}

#endregion