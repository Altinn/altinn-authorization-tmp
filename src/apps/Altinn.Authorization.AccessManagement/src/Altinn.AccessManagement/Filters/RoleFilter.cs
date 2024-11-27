using Altinn.AccessManagement.Core.Constants;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Altinn.AccessManagement.Filters;

public class RoleFilter : IActionFilter
{
    public RoleFilter()
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        throw new NotImplementedException();
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        throw new NotImplementedException();
    }
}