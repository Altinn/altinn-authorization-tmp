namespace Altinn.Authorization.Host.Operations;

public static class ServiceResultExtensions
{
    /// <summary>
    /// Maps value only if the resposne is successful
    /// </summary>
    /// <typeparam name="TContent"></typeparam>
    /// <typeparam name="TNew"></typeparam>
    /// <param name="objectResult"></param>
    /// <param name="mapper"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static ServiceObjectResult<TNew> MapContent<TContent, TNew>(this ServiceObjectResult<TContent> objectResult, Func<TContent, TNew> mapper)
    {
        if (objectResult is ServiceResultSuccess<TContent> successResult)
        {
            var content = mapper(objectResult.Content);
            return new ServiceResultSuccess<TNew>
            {
                Content = content,
                Success = successResult.Success,
                HttpStatusCode = successResult.HttpStatusCode,
            };
        }

        if (objectResult is ServiceResultProblem<TContent> problemResult)
        {
            return new ServiceResultProblem<TNew>
            {
                Content = default,
                Success = problemResult.Success,
                Descriptor = problemResult.Descriptor,
            };
        }

        if (objectResult is ServiceResultValidationError<TContent> validationErrorResult)
        {
            return new ServiceResultValidationError<TNew>
            {
                Content = default,
                Success = validationErrorResult.Success,
                Descriptor = validationErrorResult.Descriptor,
                HttpStatusCode = validationErrorResult.HttpStatusCode,
            };
        }

        throw new InvalidOperationException($"No converter registered for object result of type '{objectResult.GetType().Name}'");
    }
}
