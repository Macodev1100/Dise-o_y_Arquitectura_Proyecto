using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using P_F.Services;
using System.Security.Claims;

namespace P_F.Attributes;

public class RequierePermisoAttribute : ActionFilterAttribute
{
    private readonly string _permiso;

    public RequierePermisoAttribute(string permiso)
    {
        _permiso = permiso;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        var authService = context.HttpContext.RequestServices.GetService<IAuthService>();
        if (authService == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        var email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            context.Result = new ForbidResult();
            return;
        }

        var tienePermiso = await authService.TienePermisoAsync(email, _permiso);
        if (!tienePermiso)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}