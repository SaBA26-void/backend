using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OnlineShop.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public const string HeaderName = "X-Admin-Password";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>();

        var expected = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(expected))
        {
            context.Result = new UnauthorizedObjectResult("Admin password is not configured.");
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided)
            || !string.Equals(provided.ToString(), expected, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedObjectResult("Invalid admin password.");
        }
    }
}
