using Microsoft.AspNetCore.Mvc.Filters;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;

public class UseReadOnlyActionFilter : IAsyncActionFilter
{
    private readonly IReadOnlyHintService _hintService;

    public UseReadOnlyActionFilter(IReadOnlyHintService hintService)
    {
        _hintService = hintService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attribute = context.ActionDescriptor
            .EndpointMetadata
            .OfType<UseReadOnlyAttribute>()
            .FirstOrDefault();

        if (attribute != null)
        {
            _hintService.SetHint(attribute.Name);
        }

        await next();
    }
}
