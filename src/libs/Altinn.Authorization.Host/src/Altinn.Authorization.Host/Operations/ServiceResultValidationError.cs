using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.Host.Operations;

public class ServiceResultValidationError<T> : ServiceObjectResult<T>
{
    public int HttpStatusCode { get; init; } = StatusCodes.Status400BadRequest;

    public ValidationErrorDescriptor Descriptor { get; init; }

    public override bool Success { get; init; } = false;

    public IActionResult ConvertToActionResult(Action<ObjectResult> configureObjectResult = null)
    {
        var result = new ObjectResult(Descriptor.ToValidationError())
        {
            StatusCode = HttpStatusCode
        };

        configureObjectResult?.Invoke(result);
        return result;
    }
}
