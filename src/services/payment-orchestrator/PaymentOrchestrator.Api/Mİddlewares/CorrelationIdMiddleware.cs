using Serilog.Context;

namespace PaymentOrchestrator.Api.Mİddlewares;

public sealed class CorrelationIdMiddleware : IMiddleware
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // 1) Request’te correlation var mı?
        var correlationId =
            context.Request.Headers.TryGetValue(HeaderName, out var cid)
                ? cid.ToString()
                : Guid.NewGuid().ToString();

        // 2) HttpContext.Items içine koy
        context.Items[HeaderName] = correlationId;

        // 3) Response Header olarak geri yaz
        context.Response.Headers[HeaderName] = correlationId;

        // 4) LogContext'e push yap
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}