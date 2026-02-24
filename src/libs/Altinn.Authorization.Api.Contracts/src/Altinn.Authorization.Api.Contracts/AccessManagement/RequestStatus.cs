namespace Altinn.Authorization.Api.Contracts.AccessManagement;

public enum RequestStatus
{
    None = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Withdrawn = 4
}

public sealed record RequestStatusDto(int Id, string Name, string Description)
{
    private readonly RequestStatus value;

    internal RequestStatusDto(RequestStatus value, string name, string description) : this((int)value, name, description)
    {
        this.value = value;
    }

    public static implicit operator RequestStatusDto(RequestStatus status) => RequestStatusMapping.ToDto(status);

    public static implicit operator RequestStatus(RequestStatusDto dto) => (RequestStatus)dto.Id;
}

public static class RequestStatusMapping
{
    private static readonly Dictionary<RequestStatus, RequestStatusDto> Map =
        new()
        {
            {
                RequestStatus.None,
                new(RequestStatus.None, "none", "Request has no status")
            },
            {
                RequestStatus.Pending,
                new(RequestStatus.Pending, "pending", "Request is awaiting processing")
            },
            {
                RequestStatus.Approved,
                new(RequestStatus.Approved, "approved", "Request has been approved")
            },
            {
                RequestStatus.Rejected,
                new(RequestStatus.Rejected, "rejected", "Request has been rejected")
            },
            {
                RequestStatus.Withdrawn,
                new(RequestStatus.Withdrawn, "withdrawn", "Request has been withdrawn")
            }
        };

    private static readonly Dictionary<int, RequestStatusDto> IdMap = Map.ToDictionary(x => (int)x.Key, x => x.Value);

    public static RequestStatusDto ToDto(RequestStatus status) => Map[status];

    public static IReadOnlyCollection<RequestStatusDto> All => Map.Values;
}
