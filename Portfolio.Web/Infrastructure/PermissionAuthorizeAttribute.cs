using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Portfolio.Business.Interfaces;

namespace Portfolio.Web.Infrastructure;

public sealed class PermissionAuthorizeAttribute : TypeFilterAttribute
{
    public PermissionAuthorizeAttribute(string permission) : base(typeof(PermissionAuthorizationFilter))
        => Arguments = [permission];
}

public sealed class PermissionAuthorizationFilter(IPermissionService permissions, string permission) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }
        if (!await permissions.HasAsync(context.HttpContext.User, permission, context.HttpContext.RequestAborted))
            context.Result = new ForbidResult();
    }
}
