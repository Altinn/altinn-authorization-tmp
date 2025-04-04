using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.Host.Operations;

public static class ServiceResultFactory
{
    public static ServiceObjectResultConverter<TContent, TConvert> CreateConverter<TContent, TConvert>(ServiceObjectResult<TContent> result) => new(result);

    public static ServiceObjectResultConverter<TContent, IActionResult> CreateActionResult<TContent>(ServiceObjectResult<TContent> result) =>
        new ServiceObjectResultConverter<TContent, IActionResult>(result)
            .On<ServiceResultSuccess<TContent>>(res => res.ConvertToActionResult())
            .On<ServiceResultProblem<TContent>>(res => res.ConvertToActionResult())
            .On<ServiceResultValidationError<TContent>>(res => res.ConvertToActionResult());

    public static ServiceResultSuccess<T> CreateSuccess<T>(T result) => new()
    {
        Content = result,
    };

    public static ServiceResultValidationError<T> CreateValidationError<T>(ValidationErrorDescriptor descriptor) => new()
    {
        Descriptor = descriptor,
    };

    public static ServiceResultProblem<T> CreateProblem<T>(ProblemDescriptor descriptor) => new()
    {
        Descriptor = descriptor,
    };
}
