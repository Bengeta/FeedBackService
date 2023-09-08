using Microsoft.Net.Http.Headers;

namespace HessBackend.Middlewares;

public class TokenHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public TokenHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

        context.Items["Token"] = token;

        await _next(context);
    }

}


public static class TokenHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenHandlerMiddleware>();
    }
}