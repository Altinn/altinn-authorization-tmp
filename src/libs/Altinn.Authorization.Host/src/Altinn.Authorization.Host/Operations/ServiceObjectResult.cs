namespace Altinn.Authorization.Host.Operations;

public abstract class ServiceObjectResult<T> : ServiceResult
{
    public virtual T? Content { get; set; } = default;
}
