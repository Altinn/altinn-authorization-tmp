using System.ComponentModel.DataAnnotations;

namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// Performance metric DTO
/// </summary>
public class PerformanceMetricDto
{
    public string Scenario { get; set; } = string.Empty;
    public double ResponseTimeMs { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Authorization performance test request DTO
/// </summary>
public class AuthorizationPerformanceTestDto
{
    [Required]
    [Range(1, 10000)]
    public int NumberOfRequests { get; set; }
    
    [Required]
    [Range(1, 100)]
    public int ConcurrentUsers { get; set; }
    
    public bool IncludeComplexPolicies { get; set; } = false;
    public bool IncludeDelegations { get; set; } = false;
    public List<string>? TestScenarios { get; set; }
}

/// <summary>
/// Authorization performance test result DTO
/// </summary>
public class AuthorizationPerformanceResultDto
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public double MinResponseTimeMs { get; set; }
    public double MaxResponseTimeMs { get; set; }
    public double ThroughputRequestsPerSecond { get; set; }
    public DateTime TestStarted { get; set; }
    public DateTime TestCompleted { get; set; }
    public TimeSpan TestDuration { get; set; }
    public List<PerformanceMetricDto> Metrics { get; set; } = [];
}