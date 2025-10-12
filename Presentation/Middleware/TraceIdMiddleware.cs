using Domain.Common;

namespace Presentation.Middleware;

public class TraceIdMiddleware
{
    private readonly RequestDelegate _next;

    public TraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.Request.Headers["X-Trace-Id"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(traceId))
        {
            traceId = Guid.NewGuid().ToString();
        }
        
        TraceId.SetTraceId(traceId);
        
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Trace-Id"] = traceId;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}