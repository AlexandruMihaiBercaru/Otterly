using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Proj.Identity;

public class AllowAnonymousOnlyAttribute(string redirectTo) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            context.Result = new RedirectResult(redirectTo);
        }

        base.OnActionExecuting(context);
    }
}