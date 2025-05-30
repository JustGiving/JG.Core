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

    public static Func<string, object, bool, IDisposable> LogContextPushProperty =
        LogContext.PushProperty;

    public RequestLogger(RequestDelegate next, IOptions<RequestLoggerOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        using (LogContextPushProperty("req", GetRequest(context), true))
        {
            var sw = Stopwatch.StartNew();
            await _next.Invoke(context);
            sw.Stop();

            // backward compatibility with GG.Library.LoggingInitiator.Serilog,
            // access logs should be used instead
#pragma warning disable CS0618 // Type or member is obsolete
            if (_options.LogHandlingRequest)
            {
                var isStatusEndpoint =
                    context.Request.Path.Value?.ToLower().Contains("/status/") ?? false;

                if (isStatusEndpoint)
                {
                    return;
                }

                using (
                    LogContextPushProperty(
                        "res",
                        new
                        {
                            durationMs = sw.ElapsedMilliseconds,
                            statusCode = context.Response.StatusCode,
                        },
                        true
                    )
                )
                {
                    Log.Information(
                        "Handling request {PathValue}",
                        context.Request.Path.Value ?? "unknown"
                    );
                }
            }
#pragma warning restore CS0618 // Type or member is obsolete
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
}
