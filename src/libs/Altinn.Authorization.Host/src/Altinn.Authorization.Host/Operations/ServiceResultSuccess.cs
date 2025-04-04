using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.Host.Operations;

public class ServiceResultSuccess<T> : ServiceObjectResult<T>
{
    public int HttpStatusCode { get; init; } = StatusCodes.Status200OK;

    public override bool Success { get; init; } = true;

    public IActionResult ConvertToActionResult(Action<ObjectResult> configureObjectResult = null)
    {
        var result = new ObjectResult(Content)
        {
            StatusCode = HttpStatusCode
        };

        configureObjectResult?.Invoke(result);
        return result;
    }
}
