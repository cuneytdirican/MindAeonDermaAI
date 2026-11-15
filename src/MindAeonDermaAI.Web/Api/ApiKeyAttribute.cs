using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MindAeonDermaAI.Web.Api;

/// <summary>Requires a valid <c>X-Api-Key</c> header (matched against config <c>Api:Key</c>) for the public API.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiKeyAttribute : Attribute, IAsyncActionFilter
{
    public const string HeaderName = "X-Api-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cfg = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expected = cfg["Api:Key"];
        if (string.IsNullOrWhiteSpace(expected))
        {
            context.Result = new ObjectResult(new { error = "API is disabled: no Api:Key configured." }) { StatusCode = 503 };
            return;
        }
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided) || provided != expected)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid or missing X-Api-Key header." });
            return;
        }
        await next();
    }
}
