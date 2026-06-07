using Serilog.Context;
using System.Security.Claims;

namespace AppCoel.Core.API.Middlewares
{
    public class SerilogUserEnricherMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            var userName = context.User?.Identity?.IsAuthenticated == true
                ? context.User.FindFirstValue(ClaimTypes.Name) ?? context.User.FindFirstValue("name") ?? "Anonymous"
                : "Anonymous";

            using (LogContext.PushProperty("UserName", userName))
            {
                await next(context);
            }
        }
    }
}
