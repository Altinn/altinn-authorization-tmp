using Microsoft.AspNetCore.Mvc.Filters;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions.Hint;

public class UseHintActionFilter : IAsyncActionFilter
{
    private readonly IHintService _hintService;

    public UseHintActionFilter(IHintService hintService)
    {
        _hintService = hintService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attribute = context.ActionDescriptor
            .EndpointMetadata
            .OfType<UseHintAttribute>()
            .FirstOrDefault();

        if (attribute != null)
        {
            _hintService.SetHint(attribute.Name);
        }

        await next();
    }
}
