using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Shared;

/// <summary>
/// Common pagination DTO used across all APIs
/// </summary>
public class PaginationDto
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    [Range(1, 1000, ErrorMessage = "PageSize must be between 1 and 1000")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Paginated result wrapper
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PaginatedResultDto<T>
{
    /// <summary>
    /// The items in this page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Pagination information
    /// </summary>
    public PaginationDto Pagination { get; set; } = new();
}

/// <summary>
/// Common error DTO used across all APIs
/// </summary>
public class ErrorDto
{
    /// <summary>
    /// Error code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// API response wrapper
/// </summary>
/// <typeparam name="T">Type of the response data</typeparam>
public class ApiResponseDto<T>
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// The response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error information if request failed
    /// </summary>
    public ErrorDto? Error { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    public static ApiResponseDto<T> SuccessResult(T data) => new() { Data = data };
    public static ApiResponseDto<T> ErrorResult(string code, string message) => new()
    {
        Success = false,
        Error = new ErrorDto { Code = code, Message = message }
    };
}

/// <summary>
/// Attribute match DTO used in multiple contexts
/// </summary>
public class AttributeMatchDto
{
    /// <summary>
    /// The attribute ID
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The attribute value
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Type of matching to perform
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AttributeMatchTypeDto Type { get; set; } = AttributeMatchTypeDto.Equals;

    /// <summary>
    /// The data type of the attribute
    /// </summary>
    public string? DataType { get; set; }
}

/// <summary>
/// Base attribute DTO
/// </summary>
public class BaseAttributeDto
{
    /// <summary>
    /// The attribute ID
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The attribute value
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// The data type of the attribute
    /// </summary>
    public string? DataType { get; set; }
}

/// <summary>
/// Party information DTO
/// </summary>
public class PartyDto
{
    /// <summary>
    /// Party ID
    /// </summary>
    public int PartyId { get; set; }

    /// <summary>
    /// Party name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Organization number (if applicable)
    /// </summary>
    public string? OrganizationNumber { get; set; }

    /// <summary>
    /// Person identifier (if applicable)
    /// </summary>
    public string? PersonIdentifier { get; set; }

    /// <summary>
    /// Party type
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PartyTypeDto PartyType { get; set; }
}

/// <summary>
/// Resource reference DTO
/// </summary>
public class ResourceReferenceDto
{
    /// <summary>
    /// Type of reference
    /// </summary>
    [Required]
    public string ReferenceType { get; set; } = string.Empty;

    /// <summary>
    /// The reference value
    /// </summary>
    [Required]
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Source of the reference
    /// </summary>
    public string? ReferenceSource { get; set; }
}

/// <summary>
/// Common enums
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AttributeMatchTypeDto
{
    Equals,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PartyTypeDto
{
    Person,
    Organization,
    SubUnit
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RightSourceTypeDto
{
    Role,
    Delegation,
    AccessList,
    SystemUser,
    Maskinporten
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DelegationChangeTypeDto
{
    Created,
    Updated,
    Revoked,
    Restored
}