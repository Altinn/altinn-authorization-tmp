using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.Host.Operations;

public class ServiceResultProblem<T> : ServiceObjectResult<T>
{
    public ProblemDescriptor Descriptor { get; set; }

    public override bool Success { get; init; } = false;

    public IActionResult ConvertToActionResult()
    {
        return Descriptor.ToActionResult();
    }
}
