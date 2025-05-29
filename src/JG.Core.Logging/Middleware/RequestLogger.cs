using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;

namespace JG.Core.Logging.Middleware;

public class RequestLogger
{
    private static readonly HashSet<string> SensitiveHeaders = new(
        [
            nameof(HttpRequestHeader.Cookie).ToLower(),
            nameof(HttpRequestHeader.Authorization).ToLower(),
            "x-forwarded-auth",
        ]
    );

    private readonly RequestDelegate _next;
    private readonly RequestLoggerOptions _options;

    private static readonly ILogger Log = Serilog.Log.ForContext<RequestLogger>();

    public RequestLogger(RequestDelegate next, IOptions<RequestLoggerOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("req", GetRequest(context), true))
        using (LogContext.PushProperty("RequestId", GetRequestId(context), true))
        {
            var sw = Stopwatch.StartNew();
            await _next.Invoke(context);
            sw.Stop();

            if (_options.LogHandlingRequest)
            {
                var isStatusEndpoint =
                    context.Request.Path.Value?.ToLower().Contains("/status/") ?? false;

                if (isStatusEndpoint && !_options.LogStatusEndpoints)
                {
                    return;
                }

                using (
                    LogContext.PushProperty(
                        "res",
                        GetResponse(context, sw.ElapsedMilliseconds),
                        true
                    )
                )
                {
                    Log.Information(
                        "Handled request {PathValue}",
                        context.Request.Path.Value ?? "unknown"
                    );
                }
            }
        }
    }

    private static object GetRequest(HttpContext context)
    {
        var nonSensitiveHeaders = context.Request.Headers.Where(h =>
            !SensitiveHeaders.Contains(h.Key.ToLower())
        );

        return new
        {
            method = context.Request.Method,
            path = context.Request.Path.Value,
            queryString = context.Request.QueryString.Value,
            headers = nonSensitiveHeaders.ToDictionary(
                h => h.Key.ToLower(),
                h => h.Value.ToString()
            ),
            referer = context.Request.Headers.Referer.FirstOrDefault(),
            clientIp = context.Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
        };
    }

    private static object GetRequestId(HttpContext context)
    {
        return context.Request.Headers.FirstOrDefault(h => h.Key == "x-request-id");
    }

    private static object GetResponse(HttpContext context, long ms)
    {
        return new { durationMs = ms, statusCode = context.Response.StatusCode };
    }
}
